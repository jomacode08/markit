using Hangfire;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Models.MeiliSearch.Documents;

namespace markit.Infraestructure.Persistence.MeiliSearch.Services
{
    public class DocumentJobService<T> : IDocumentJobService<T> where T : Document
    {
        private readonly IBackgroundJobClient _jobClient;

        public DocumentJobService(IBackgroundJobClient jobClient)
        {
            _jobClient = jobClient;
        }

        public string ScheduleAddAsync(T document)
        {
            return _jobClient.Enqueue<IDocumentRepository<T>>(x => x.AddAsync(document));
        }

        public string ScheduleUpdateAsync(T document)
        {
            return _jobClient.Enqueue<IDocumentRepository<T>>(x => x.UpdateAsync(document));
        }

        public string ScheduleDeleteAsync(string documentId)
        {
            return _jobClient.Enqueue<IDocumentRepository<T>>(x => x.DeleteAsync(documentId));
        }
    }
}
