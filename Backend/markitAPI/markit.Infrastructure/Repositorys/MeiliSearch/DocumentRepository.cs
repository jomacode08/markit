using markit.Application.Contracts.MeiliSearch;
using markit.Application.Helpers;
using markit.Application.Models.Authentication.MeiliSearch;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Application.Models.MeiliSearch.Search;
using markit.infrastructure.Persistence.MeiliSearch.Helpers;
using Meilisearch;
using Index = Meilisearch.Index;

namespace markit.infrastructure.Repositorys.MeiliSearch
{
    public class DocumentRepository<T> : IDocumentRepository<T> where T : Document
    {
        private readonly MeilisearchClient _client;
        private readonly string _indexUid;

        public DocumentRepository(MeiliSearchAuthSettings authSettings, string indexUid) {
            _indexUid = indexUid;
            _client = new MeilisearchClient(authSettings.UrlServer, authSettings.ApiKey);
        }

        public async Task<IReadOnlyList<T>> FormattedSearchAsync(FormattedDocumentSearch search)
        {
            if (search.AttributesToHighlight == null || search.AttributesToHighlight.Length == 0)
                throw new ArgumentException(nameof(search.AttributesToHighlight));

            var index = await GetIndexAsync();
            ISearchable<FormattedDocument<T>> result = await index.SearchAsync<FormattedDocument<T>>(
                search.Query,
                searchAttributes: new SearchQuery
                {
                    IndexUid = _indexUid,
                    Filter = GetGlobalFilter(search.CreatorId),
                    Limit = search.Limit,
                    AttributesToHighlight = search.AttributesToHighlight,
                    HighlightPreTag = "<span>",
                    HighlightPostTag = "</span>",
                }
            );
            return [.. result.Hits.Select(fd => fd.Formatted)];
        }

        public async Task<IReadOnlyList<T>> SearchAsync(DocumentSearch search)
        {
            var index = await GetIndexAsync();
            ISearchable<T> result = await index.SearchAsync<T>(
                search.Query,
                searchAttributes: new SearchQuery {
                    IndexUid = _indexUid,
                    Filter = GetGlobalFilter(search.CreatorId),
                    Limit = search.Limit,
                }
            );
            return [.. result.Hits];
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
            return await _client.GetIndexAsync(_indexUid);
        }

        private async Task HandleUnfinishedTask(int taskUid)
        {
            var finishedTask = await _client.WaitForTaskAsync(taskUid);
            MeiliSearchHelper.EnsureTaskSucceeded(finishedTask);
        }

        private static string GetGlobalFilter(int creatorId) => $"enabled = true AND creatorId = {creatorId}";
    }
}
