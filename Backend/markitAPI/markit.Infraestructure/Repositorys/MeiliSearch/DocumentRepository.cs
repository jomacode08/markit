using markit.Application.Contracts.MeiliSearch;
using markit.Application.Models.Authentication.MeiliSearch;
using markit.Application.Models.MeiliSearch.Documents;
using Meilisearch;
using Index = Meilisearch.Index;

namespace markit.Infraestructure.Repositorys.MeiliSearch
{
    public class DocumentRepository<T> : IDocumentRepository<T> where T : Document
    {
        private readonly MeiliSearchAuthSettings _settings;
        private readonly string _indexUid;

        public DocumentRepository(MeiliSearchAuthSettings authSettings, string indexUid) {
            _settings = authSettings;
            _indexUid = indexUid;
        }

        public async Task<T> GetByIdAsync(string documentId)
        {
            var index = await GetIndexAsync();
            return await index.GetDocumentAsync<T>(documentId);
        }

        public async Task AddAsync(T document)
        {
            var index = await GetIndexAsync();
            await index.AddDocumentsAsync([document]);
        }

        public async Task UpdateAsync(T document)
        {
            var index = await GetIndexAsync();
            await index.UpdateDocumentsAsync([document]);
        }

        public async Task DeleteAsync(string documentId)
        {
            var index = await GetIndexAsync();
            await index.DeleteOneDocumentAsync(documentId);
        }

        private async Task<Index> GetIndexAsync()
        {
            var client = new MeilisearchClient(_settings.UrlServer, _settings.ApiKey);

            // Check server availability
            var isServerHealthy = await client.IsHealthyAsync();
            if (!isServerHealthy)
            {
                throw new Exception("It hasn't been posible to connect with the MileiSearch server");
            }

            // Check index existency
            var currentIndexes = (await client.GetStatsAsync()).Indexes;
            if (!currentIndexes.ContainsKey(_indexUid))
            {
                throw new Exception($"The index with id: { _indexUid } doesn't exist.");
            }

            return await client.GetIndexAsync(_indexUid);
        }
    }
}
