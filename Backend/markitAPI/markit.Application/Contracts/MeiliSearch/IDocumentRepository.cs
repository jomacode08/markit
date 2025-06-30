using markit.Application.Models.MeiliSearch.Documents;

namespace markit.Application.Contracts.MeiliSearch
{
    public interface IDocumentRepository<T> where T : Document
    {
        public Task<T> GetByIdAsync(string documentId);

        public Task AddAsync(T document);

        public Task UpdateAsync(T document);

        public Task DeleteAsync(string documentId);

    }
}
