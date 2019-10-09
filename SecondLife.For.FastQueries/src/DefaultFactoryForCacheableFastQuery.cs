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

            if (_databases.TryGetValue(name, out var cached))
            {
                if (assert && cached == null)
                {
                    throw new InvalidOperationException($"Could not create database: {name}");
                }
            }

            var configPath = "fastQueryDatabases/database[@id='" + name + "']";

            if (CreateObject(configPath, assert: false) is Database database)
            {
                _databases.TryAdd(name, database);
                return database;
            }

            database = base.GetDatabase(name, assert: false);
            _databases.TryAdd(name, database);

            if (assert && database == null)
            {
                throw new InvalidOperationException($"Could not create database: {name}");
            }

            return database;
        }
    }
}
