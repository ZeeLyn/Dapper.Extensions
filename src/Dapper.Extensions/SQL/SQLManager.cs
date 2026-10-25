using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Dapper.Extensions.SQL
{
    public class SQLManager : ISQLManager, IDisposable
    {
        private ConcurrentDictionary<string, string[]> sqlCache;
        private readonly object reloadLock = new object();
        private IDisposable subscription;

        private ILogger Logger { get; }

        private SQLSeparateConfigure Configure { get; }

        private IFileProvider FileProvider { get; }

        public SQLManager(SQLSeparateConfigure configure, ILogger<SQLManager> logger)
        {
            Logger = logger;
            Configure = configure;
            FileProvider = new PhysicalFileProvider(Configure.RootDir);
            sqlCache = new ConcurrentDictionary<string, string[]>();
            try { Initialize(); }
            catch { (FileProvider as IDisposable)?.Dispose(); throw; }
        }

        private void Initialize()
        {
            ReadXmlFiles(sqlCache);
            subscription = ChangeToken.OnChange(() => FileProvider.Watch("**/*.xml"), () =>
            {
                Thread.Sleep(500);
                lock (reloadLock)
                {
                    try
                    {
                        var snapshot = new ConcurrentDictionary<string, string[]>();
                        ReadXmlFiles(snapshot);
                        Volatile.Write(ref sqlCache, snapshot);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Sql separate xml file error:{ex.Message}");
                    }
                }
            });
        }

        private void ReadXmlFiles(ConcurrentDictionary<string, string[]> dic)
        {
            void ReadFiles(string dir)
            {
                using var fileProvider = new PhysicalFileProvider(dir);
                foreach (var file in fileProvider.GetDirectoryContents(""))
                {
                    if (file.IsDirectory)
                    {
                        ReadFiles(file.PhysicalPath);
                    }
                    else if (string.Equals(Path.GetExtension(file.PhysicalPath), ".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        ReadXml(file.PhysicalPath, dic);
                    }
                }

            }
            ReadFiles(Configure.RootDir);
        }

        private void ReadXml(string url, ConcurrentDictionary<string, string[]> dic)
        {
            var elements = DeserializeXml<SQLElement>(url);
            foreach (var ele in elements.SQL)
            {
                if (dic.ContainsKey(ele.Name))
                    throw new ArgumentException($"Duplicate name \"{ele.Name}\".");
                var element = new[] { ele.SQL };
                dic.AddOrUpdate(ele.Name, element, (document, oldValue) => oldValue);
            }

            foreach (var ele in elements.Paging)
            {
                if (dic.ContainsKey(ele.Name))
                    throw new ArgumentException($"Duplicate name \"{ele.Name}\".");
                var element = new[] { ele.CountSQL, ele.QuerySQL };
                dic.AddOrUpdate(ele.Name, element, (document, oldValue) => oldValue);
            }
        }


        private T DeserializeXml<T>(string url)
        {
            using var reader = new StringReader(File.ReadAllText(url));
            var xz = new XmlSerializer(typeof(T));
            return (T)xz.Deserialize(reader);
        }

        void IDisposable.Dispose()
        {
            subscription?.Dispose();
            (FileProvider as IDisposable)?.Dispose();
        }

        public string GetSQL(string name)
        {
            if (Volatile.Read(ref sqlCache).TryGetValue(name, out var value))
                return value[0];
            throw new KeyNotFoundException($"SQL element with name {name} is not found.");
        }

        public (string CountSQL, string QuerySQL) GetPagingSQL(string name)
        {
            if (!Volatile.Read(ref sqlCache).TryGetValue(name, out var value))
                throw new KeyNotFoundException($"SQL element with name {name} is not found.");
            if (value.Length != 2)
                throw new ArgumentException("Wrong paging SQL configuration.");
            return (value[0], value[1]);
        }
    }
}
