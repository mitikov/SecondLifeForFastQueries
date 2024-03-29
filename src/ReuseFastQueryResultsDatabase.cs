﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Archiving;
using Sitecore.Data.Clones;
using Sitecore.Data.DataProviders;
using Sitecore.Data.Eventing;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.SecurityModel;
using Sitecore.Workflows;
using Version = Sitecore.Data.Version;

namespace SecondLife.For.FastQueries
{
    /// <summary>
    /// Optionally provides caching layer for fast query results on top of inner <see cref="Database"/>.
    /// <para>Caching is controlled over <see cref="CacheFastQueryResults"/> config property.</para>
    /// <para>The caching layer is scavenged when publish ends.</para>
    /// </summary>
    public sealed class ReuseFastQueryResultsDatabase : Database
    {
        private readonly Database _database;

        private readonly ConcurrentDictionary<string, Item> _singleItems = new ConcurrentDictionary<string, Item>(StringComparer.OrdinalIgnoreCase);
        private readonly LockSet _singleItemLocks = new LockSet();

        private readonly ConcurrentDictionary<string, IReadOnlyCollection<Item>> _multipleItems = new ConcurrentDictionary<string, IReadOnlyCollection<Item>>(StringComparer.OrdinalIgnoreCase);
        private readonly LockSet _multipleItemsLock = new LockSet();

        public ReuseFastQueryResultsDatabase(Database database)
        {
            Assert.ArgumentNotNull(database, nameof(database));
            _database = database;
        }

        [UsedImplicitly]
        public bool CacheFastQueryResults { get; private set; }

        #region Useful code

        public override Item SelectSingleItem(string query)
        {
            if (!CacheFastQueryResults || !IsFast(query))
            {
                return _database.SelectSingleItem(query);
            }

            if (!_singleItems.TryGetValue(query, out var cached))
            {
                lock (_singleItemLocks.GetLock(query))
                {
                    if (!_singleItems.TryGetValue(query, out cached))
                    {
                        using (new SecurityDisabler())
                        {
                            cached = _database.SelectSingleItem(query);
                        }

                        _singleItems.TryAdd(query, cached);
                    }
                }
            }

            if (cached?.Access.CanRead() == true)
            {
                var copy = new Item(cached.ID, cached.InnerData, this);
                return copy;
            }

            return null;
        }

        public override Item[] SelectItems(string query)
        {
            if (!CacheFastQueryResults || !IsFast(query))
            {
                return _database.SelectItems(query);
            }

            if (!_multipleItems.TryGetValue(query, out var cached))
            {
                lock (_multipleItemsLock.GetLock(query))
                {
                    if (!_multipleItems.TryGetValue(query, out cached))
                    {
                        using (new SecurityDisabler())
                        {
                            cached = _database.SelectItems(query);
                        }

                        _multipleItems.TryAdd(query, cached);
                    }
                }
            }

            var results = from item in cached ?? Array.Empty<Item>()
                          where item.Access.CanRead()
                          select new Item(item.ID, item.InnerData, this);

            return results.ToArray();
        }

        private static bool IsFast(string query) => query?.StartsWith("fast:/") == true;

        protected override void OnConstructed(XmlNode configuration)
        {
            if (!CacheFastQueryResults)
            {
                return;
            }

            Event.Subscribe("publish:end", PublishEnd);
            Event.Subscribe("publish:end:remote", PublishEnd);
        }

        private void PublishEnd(object sender, EventArgs e)
        {
            _singleItems.Clear();
            _multipleItems.Clear();
        }

        #endregion

        #region Boilerplate to decorate database impl

        public override bool CleanupDatabase() => _database.CleanupDatabase();

        public override Item CreateItemPath(string path) => _database.CreateItemPath(path);

        public override Item CreateItemPath(string path, TemplateItem template) => _database.CreateItemPath(path, template);

        public override Item CreateItemPath(string path, TemplateItem folderTemplate, TemplateItem itemTemplate) => _database.CreateItemPath(path, folderTemplate, itemTemplate);

        public override DataProvider[] GetDataProviders() => _database.GetDataProviders();

        public override long GetDataSize(int minEntitySize, int maxEntitySize) => _database.GetDataSize(minEntitySize, maxEntitySize);

        public override long GetDictionaryEntryCount() => _database.GetDictionaryEntryCount();

        public override Item GetItem(ID itemId) => _database.GetItem(itemId);

        public override Item GetItem(ID itemId, Language language) => _database.GetItem(itemId, language);

        public override Item GetItem(ID itemId, Language language, Version version) => _database.GetItem(itemId, language, version);

        public override Item GetItem(string path) => _database.GetItem(path);

        public override Item GetItem(string path, Language language) => _database.GetItem(path, language);

        public override Item GetItem(string path, Language language, Version version) => _database.GetItem(path, language, version);

        public override Item GetItem(DataUri uri) => _database.GetItem(uri);

        public override LanguageCollection GetLanguages() => _database.GetLanguages();

        public override Item GetRootItem() => _database.GetRootItem();

        public override Item GetRootItem(Language language) => _database.GetRootItem(language);

        public override TemplateItem GetTemplate(ID templateId) => _database.GetTemplate(templateId);

        public override TemplateItem GetTemplate(string fullName) => _database.GetTemplate(fullName);

        public override ItemList SelectItemsUsingXPath(string query) => _database.SelectItemsUsingXPath(query);

        public override Item SelectSingleItemUsingXPath(string query) => _database.SelectSingleItemUsingXPath(query);

        public override AliasResolver Aliases => _database.Aliases;

        public override List<string> ArchiveNames => _database.ArchiveNames;

        public override DataArchives Archives => _database.Archives;

        public override DatabaseCaches Caches => _database.Caches;

        public override string ConnectionStringName
        {
            get => _database.ConnectionStringName;
            set => _database.ConnectionStringName = value;
        }

        public override DataManager DataManager => _database.DataManager;

        public override DatabaseEngines Engines => _database.Engines;

        public override bool HasContentItem => _database.HasContentItem;

        public override string Icon
        {
            get => _database.Icon;
            set => _database.Icon = value;
        }

        public override ItemRecords Items => _database.Items;

        public override Language[] Languages => _database.Languages;

        public override BranchRecords Branches => _database.Branches;

        public override string Name => _database.Name;

        public override DatabaseProperties Properties => _database.Properties;

        public override bool Protected
        {
            get => _database.Protected;
            set => _database.Protected = value;
        }

        public override bool PublishVirtualItems
        {
            get => _database.PublishVirtualItems;
            set => _database.PublishVirtualItems = value;
        }

        public override bool ReadOnly
        {
            get => _database.ReadOnly;
            set => _database.ReadOnly = value;
        }

        public override DatabaseRemoteEvents RemoteEvents => _database.RemoteEvents;

        public override ResourceItems Resources => _database.Resources;

        public override bool SecurityEnabled
        {
            get => _database.SecurityEnabled;
            set => _database.SecurityEnabled = value;
        }

        public override Item SitecoreItem => _database.SitecoreItem;

        public override TemplateRecords Templates => _database.Templates;

        public override IWorkflowProvider WorkflowProvider
        {
            get => _database.WorkflowProvider;
            set => _database.WorkflowProvider = value;
        }

        public override NotificationProvider NotificationProvider
        {
            get => _database.NotificationProvider;
            set => _database.NotificationProvider = value;
        }

        #endregion

        #region Sitecore technical debt - internal prop in abstract class =\

        private volatile DataProviderCollection _dataProviders;
        private readonly object _lock = new object();

        protected override DataProviderCollection DataProviders
        {
            get
            {
                var copy = _dataProviders;

                if (copy != null)
                {
                    return copy;
                }

                lock (_lock)
                {
                    if (_dataProviders != null)
                    {
                        return _dataProviders;
                    }

                    var providers = _database.GetDataProviders();

                    var collection = new DataProviderCollection();

                    foreach (var provider in providers)
                    {
                        collection.Add(provider);
                    }

                    Interlocked.CompareExchange(ref _dataProviders, collection, comparand: null);
                }

                return _dataProviders;
            }
        }

        #endregion
    }
}
