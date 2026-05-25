using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;

namespace markit.Application.Features.Reports.Dashboard.Queries.ViewModels
{
    public record DashboardReportVm(
        string UserId,
        DateTime CreatedAt,
        List<MarkViewModel> RecentMarks,
        List<CollectionViewModel> StarredCollections,
        ActivityStats Stats
    );

    public record ActivityStats(
        int MarksCount,
        int CollectionsCount,
        int TodayMarksCount,
        int WeekMarksCount,
        WeeklyMarkActivity WeeklyMarkActivity
    );

    public record WeeklyMarkActivity(
        int HighestTotal,
        IDictionary<DayOfWeek, int> DailyActivity
    );
}
