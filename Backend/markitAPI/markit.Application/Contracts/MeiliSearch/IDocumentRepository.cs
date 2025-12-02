using markit.Application.Models.MeiliSearch.Documents;
using markit.Application.Models.MeiliSearch.Search;

namespace markit.Application.Contracts.MeiliSearch
{
    public interface IDocumentRepository<T> where T : Document
    {
        public Task<IReadOnlyList<T>> FormattedSearchAsync(FormattedDocumentSearch search);
        public Task<IReadOnlyList<T>> SearchAsync(DocumentSearch search);
        public Task<T> GetByIdAsync(string documentId);
        public Task AddAsync(T document);
        public Task UpdateAsync(T document);
        public Task DeleteAsync(string documentId);
    }
}
