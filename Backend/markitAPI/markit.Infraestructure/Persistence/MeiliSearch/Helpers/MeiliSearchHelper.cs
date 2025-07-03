using Meilisearch;

namespace markit.Infraestructure.Persistence.MeiliSearch.Helpers
{
    public static class MeiliSearchHelper
    {
        public static void EnsureTaskSucceeded(TaskResource finishedTask)
        {
            if (!finishedTask.Status.Equals(TaskInfoStatus.Succeeded))
            {
                string[] errorMessages = [.. finishedTask.Error.Values];
                string message = errorMessages.Length > 0
                    ? string.Join(",", errorMessages)
                    : $"Something went wrong while processing meilisearch task, Uid: {finishedTask.Uid}.";

                throw new Exception(message);
            }
        }
    }
}
