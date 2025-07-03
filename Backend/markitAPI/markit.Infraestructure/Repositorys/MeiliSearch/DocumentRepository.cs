using markit.Application.Contracts.MeiliSearch;
using markit.Application.Helpers;
using markit.Application.Models.Authentication.MeiliSearch;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Infraestructure.Persistence.MeiliSearch.Helpers;
using Meilisearch;
using Index = Meilisearch.Index;

namespace markit.Infraestructure.Repositorys.MeiliSearch
{
    public class DocumentRepository<T> : IDocumentRepository<T> where T : Document
    {
        private readonly MeilisearchClient _client;
        private readonly string _indexUid;

        public DocumentRepository(MeiliSearchAuthSettings authSettings, string indexUid) {
            _indexUid = indexUid;
            _client = new MeilisearchClient(authSettings.UrlServer, authSettings.ApiKey);
        }

        public async Task<T> GetByIdAsync(string documentId)
        {
            var index = await GetIndexAsync();
            return await index.GetDocumentAsync<T>(documentId);
        }

        public async Task AddAsync(T document)
        {
            var index = await GetIndexAsync();
            var unfinishedTask = await index.AddDocumentsAsync(
                [document],
                GeneralConstant.MeiliSearch.INDEX_PRIMARY_KEY_NAME
            );
            await HandleUnfinishedTask(unfinishedTask.TaskUid);
        }

        public async Task UpdateAsync(T document)
        {
            var index = await GetIndexAsync();
            var unfinishedTask = await index.UpdateDocumentsAsync([document]);
            await HandleUnfinishedTask(unfinishedTask.TaskUid);
        }

        public async Task DeleteAsync(string documentId)
        {
            var index = await GetIndexAsync();
            var unfinishedTask = await index.DeleteOneDocumentAsync(documentId);
            await HandleUnfinishedTask(unfinishedTask.TaskUid);
        }

        private async Task<Index> GetIndexAsync()
        {
            // Check server availability
            var isServerHealthy = await _client.IsHealthyAsync();
            if (!isServerHealthy)
            {
                throw new Exception("It hasn't been posible to connect with the MileiSearch server");
            }

            // Check index existency
            var currentIndexes = (await _client.GetStatsAsync()).Indexes;
            if (!currentIndexes.ContainsKey(_indexUid))
            {
                throw new Exception($"The index with id: { _indexUid } doesn't exist.");
            }

            return await _client.GetIndexAsync(_indexUid);
        }

        private async Task HandleUnfinishedTask(int taskUid)
        {
            var finishedTask = await _client.WaitForTaskAsync(taskUid);
            MeiliSearchHelper.EnsureTaskSucceeded(finishedTask);
        }
    }
}
