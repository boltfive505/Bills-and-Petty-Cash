using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;

namespace System.Data.SQLite.EF6.Configuration
{
    internal class SQLiteConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string connectionString)
        {
            var conn = new SQLiteConnection(connectionString, true);
            return conn;
        }
    }
}
