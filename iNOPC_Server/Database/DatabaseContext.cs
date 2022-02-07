using iNOPC.Server.Database.Models;
using LinqToDB;
using LinqToDB.Data;

namespace iNOPC.Server.Database
{
    class DatabaseContext : DataConnection
    {
        public DatabaseContext() : base(ProviderName.SqlServer, 
            "Data Source=" + Program.Configuration.Database.ServerName + "; " +
            "Initial Catalog=iNOPC; " +
            "Persist Security Info=True; " +
            "User ID=" + Program.Configuration.Database.User + "; " +
            "Password=" + Program.Configuration.Database.Password + "") { }

        public ITable<Point> Points
            => GetTable<Point>();
    }
}