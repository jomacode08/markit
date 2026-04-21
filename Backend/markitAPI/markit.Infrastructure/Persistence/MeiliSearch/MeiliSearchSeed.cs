using markit.Application.Helpers;
using markit.Application.Models.Authentication.MeiliSearch;
using markit.Application.Models.MeiliSearch;
using markit.infrastructure.Persistence.MeiliSearch.Helpers;
using Meilisearch;

namespace markit.infrastructure.Persistence.MeiliSearch
{
    public static class MeiliSearchSeed
    {
        private static readonly List<MeiliSearchIndex> indexes = [
            new MeiliSearchIndex(GeneralConstant.MeiliSearch.COLLECTION_INDEX_UID, [
                new MeiliSearchAttributeIndex("id", Displayed : true),
                new MeiliSearchAttributeIndex("name", Displayed : true, Searchable: true),
                new MeiliSearchAttributeIndex("collectionId", Displayed : true),
                new MeiliSearchAttributeIndex("creatorId", Displayed : true, Filterable: true),
                new MeiliSearchAttributeIndex("enabled", Displayed : true, Filterable: true),
            ]),
            new MeiliSearchIndex(GeneralConstant.MeiliSearch.MARK_INDEX_UID, [
                new MeiliSearchAttributeIndex("id", Displayed : true),
                new MeiliSearchAttributeIndex("name", Displayed : true, Searchable: true),
                new MeiliSearchAttributeIndex("markId", Displayed : true),
                new MeiliSearchAttributeIndex("creatorId", Displayed : true, Filterable: true),
                new MeiliSearchAttributeIndex("enabled", Displayed : true, Filterable: true),
            ]),
        ];

        public static async Task SeedAsync(MeiliSearchAuthSettings settings)
        {
            var client = new MeilisearchClient(settings.UrlServer, settings.ApiKey);

            // Check server health
            bool serverActive = await client.IsHealthyAsync();
            if (!serverActive) {
                throw new Exception($"It hasn't been posible to connect with the MileiSearch server: {settings.UrlServer}");
            }

            await CreateIndexes(client);
        }

        private static async Task CreateIndexes(MeilisearchClient client)
        {
            var currentIndexes = (await client.GetStatsAsync()).Indexes;

            foreach (var index in indexes)
            {
                if (!currentIndexes.ContainsKey(index.Uid))
                {
                    var displayedAttributes = index.Attributes.Where(a => a.Displayed).Select(a => a.Name);
                    var searchableAttributes = index.Attributes.Where(a => a.Searchable).Select(a => a.Name);
                    var filterableAttributes = index.Attributes.Where(a => a.Filterable).Select(a => a.Name);

                    Settings settings = new()
                    {
                        DisplayedAttributes = displayedAttributes,
                        SearchableAttributes = searchableAttributes,
                        FilterableAttributes = filterableAttributes
                    };

                    var unfinishedTask = await client.Index(index.Uid).UpdateSettingsAsync(settings);
                    var finishedTask = await client.WaitForTaskAsync(unfinishedTask.TaskUid);
                    MeiliSearchHelper.EnsureTaskSucceeded(finishedTask);
                }
            }
        }
    }
}
