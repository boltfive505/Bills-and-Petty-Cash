using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.SQLite.EF6;

namespace System.Data.SQLite.EF6.Configuration
{
    internal class SQLiteDbDependencyResolver : IDbDependencyResolver
    {
        public object GetService(Type type, object key)
        {
            if (type == typeof(IProviderInvariantName)) return SQLiteProviderInvariantName.Instance;
            if (type == typeof(DbProviderFactory)) return SQLiteProviderFactory.Instance;
            return SQLiteProviderFactory.Instance.GetService(type);
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            var service = GetService(type, key);
            if (service != null) yield return service;
            //
            if (type == typeof(IDbProviderFactoryResolver)) yield return SQLiteDbProviderFactoryResolver.Instance;
        }
    }
}
