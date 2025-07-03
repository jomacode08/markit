using System.Linq.Expressions;
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

        public string ScheduleAddAsync(T document, Expression<Func<Task>>? continueWith = null)
        {
            string parentJobId = _jobClient.Enqueue<IDocumentRepository<T>>(x => x.AddAsync(document));
            return continueWith != null
                ? _jobClient.ContinueJobWith(parentJobId, continueWith)
                : parentJobId;
        }

        public string ScheduleUpdateAsync(T document, Expression<Func<Task>>? continueWith = null)
        {
            string parentJobId = _jobClient.Enqueue<IDocumentRepository<T>>(x => x.UpdateAsync(document));
            return continueWith != null
                ? _jobClient.ContinueJobWith(parentJobId, continueWith)
                : parentJobId;
        }

        public string ScheduleDeleteAsync(string documentId, Expression<Func<Task>>? continueWith = null)
        {
            string parentJobId = _jobClient.Enqueue<IDocumentRepository<T>>(x => x.DeleteAsync(documentId));
            return continueWith != null
                ? _jobClient.ContinueJobWith(parentJobId, continueWith)
                : parentJobId;
        }
    }
}
