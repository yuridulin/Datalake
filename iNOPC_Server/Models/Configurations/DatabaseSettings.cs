namespace iNOPC.Server.Models.Configurations
{
    public class DatabaseSettings
    {
        public bool UseDatabase { get; set; } = true;

        public string ServerName { get; set; } = "localhost\\SQLEXPRESS";

        public string User { get; set; } = "inopc";

        public string Password { get; set; } = "inopc";

        public int StoreIntervalS { get; set; } = 5;
    }
}