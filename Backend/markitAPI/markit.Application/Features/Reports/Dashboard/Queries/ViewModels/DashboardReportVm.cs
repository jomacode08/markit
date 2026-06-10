using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Notebooks.Queries.ViewModels;

namespace markit.Application.Features.Reports.Dashboard.Queries.ViewModels
{
    public record DashboardReportVm(
        string UserId,
        DateTime CreatedAt,
        List<NotebookViewModel> RecentNotebooks,
        List<CollectionViewModel> StarredCollections,
        ActivityStats Stats
    );

    public record ActivityStats(
        int NotebooksCount,
        int CollectionsCount,
        int TodayNotebooksCount,
        int WeekNotebooksCount,
        WeeklyNotebookActivity WeeklyNotebookActivity
    );

    public record WeeklyNotebookActivity(
        int HighestTotal,
        IDictionary<DayOfWeek, int> DailyActivity
    );
}
