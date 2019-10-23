using System.IO;

namespace Dapper.Extensions.SQL
{
    public class SQLSeparateConfigure
    {
        private string _rootDir;
        public string RootDir
        {
            get => _rootDir; set
            {
                if (!Directory.Exists(value))
                    throw new DirectoryNotFoundException($"Directory not found '{value}'.");
                _rootDir = value;
            }
        }
    }
}
