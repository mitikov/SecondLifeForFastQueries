using System.Collections.Generic;
using System.Threading;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Archiving;
using Sitecore.Data.Clones;
using Sitecore.Data.DataProviders;
using Sitecore.Data.Eventing;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Workflows;
using Version = Sitecore.Data.Version;

namespace SecondLife.For.FastQueries
{    
    public sealed class ReuseFastQueryResultsDatabase : Database
    {
        private readonly Database _database;

        public ReuseFastQueryResultsDatabase(Database database)
        {
            Assert.ArgumentNotNull(database, nameof(database));
            _database = database;
        }

        #region Useful code

        public override Item SelectSingleItem(string query)
        {
            return _database.SelectSingleItem(query);
        }

        public override Item[] SelectItems(string query)
        {
            return _database.SelectItems(query);
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
