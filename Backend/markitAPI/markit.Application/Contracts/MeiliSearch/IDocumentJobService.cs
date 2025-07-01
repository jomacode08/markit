using markit.Application.Models.MeiliSearch.Documents;

namespace markit.Application.Contracts.MeiliSearch
{
    public interface IDocumentJobService<T> where T : Document
    {
        public string ScheduleAddAsync(T document);
        public string ScheduleUpdateAsync(T document);
        public string ScheduleDeleteAsync(string documentId);
    }
}
