using System.Linq.Expressions;
using markit.Application.Models.MeiliSearch.Documents;

namespace markit.Application.Contracts.MeiliSearch
{
    public interface IDocumentJobService<T> where T : Document
    {
        public string ScheduleAddAsync(T document, Expression<Func<Task>>? continueWith = null);
        public string ScheduleUpdateAsync(T document, Expression<Func<Task>>? continueWith = null);
        public string ScheduleDeleteAsync(string documentId, Expression<Func<Task>>? continueWith = null);
    }
}
