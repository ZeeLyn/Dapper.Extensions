namespace Dapper.Extensions
{
    public class DapperBuilder
    {
        public string ConnectionName { get; set; } = "DefaultConnection";

        public string ServiceKey { get; set; }

        public bool EnableMasterSlave { get; set; }
    }
}
