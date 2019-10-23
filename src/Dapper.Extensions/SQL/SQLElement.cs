using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dapper.Extensions.SQL
{
    [XmlRoot("sql-set")]
    public class SQLElement
    {
        [XmlElement("sql")]
        public List<SQLCommand> SQL { get; set; }

        [XmlElement("paging-sql")]
        public List<PagingSQLCommand> Paging { get; set; }
    }

    public class SQLCommand
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string SQL { get; set; }
    }

    public class PagingSQLCommand
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("count")]
        public string CountSQL { get; set; }

        [XmlElement("query")]
        public string QuerySQL { get; set; }
    }
}
