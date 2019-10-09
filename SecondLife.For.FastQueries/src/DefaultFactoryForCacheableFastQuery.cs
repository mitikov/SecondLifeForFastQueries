using System;
using System.Collections.Concurrent;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;

namespace SecondLife.For.FastQueries
{
    public sealed class DefaultFactoryForCacheableFastQuery : DefaultFactory
    {
        private static readonly char[] ForbiddenChars = "[\\\"*^';&></=]".ToCharArray();

        private readonly ConcurrentDictionary<string, Database> _databases;

        public DefaultFactoryForCacheableFastQuery(BaseComparerFactory comparerFactory, IServiceProvider serviceProvider)
            : base(comparerFactory, serviceProvider)
        {
            _databases = new ConcurrentDictionary<string, Database>(StringComparer.OrdinalIgnoreCase);
        }

        public override Database GetDatabase(string name, bool assert)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            if (name.IndexOfAny(ForbiddenChars) >= 0)
            {
                Assert.IsFalse(assert, nameof(assert));
                return null;
            }

            var database = _databases.GetOrAdd(name, dbName =>
            {
                var configPath = "fastQueryDatabases/database[@id='" + dbName + "']";
                if (CreateObject(configPath, assert: false) is Database result)
                {
                    return result;
                }

                return base.GetDatabase(dbName, assert: false);
            });

            if (assert && database == null)
            {
                throw new InvalidOperationException($"Could not create database: {name}");
            }

            return database;
        }
    }
}
