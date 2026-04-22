using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Application.Features.Reports.Dashboard.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Reports.Dashboard.Queries
{
    public class DashboardReportQuery(int creatorId) : IRequest<DashboardReportVm>
    {
        public int CreatorId { get; set; } = creatorId;
    }

    public class DashboardReportQueryHandler : IRequestHandler<DashboardReportQuery, DashboardReportVm>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DashboardReportQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DashboardReportVm> Handle(DashboardReportQuery request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistencyAsync(request.CreatorId);
            List<MarkViewModel> recentMarks = await GetRecentMarksAsync(request.CreatorId);
            List<CollectionViewModel> starredCollections = await GetStarredCollectionsAsync(request.CreatorId);
            var activityStats = await GetActivityStatsAsync(request.CreatorId);

            return new DashboardReportVm(
                CreatorId : request.CreatorId,
                CreatedAt : DateTime.Now,
                recentMarks,
                starredCollections,
                activityStats
            );
        }

        private async Task ValidateCreatorExistencyAsync(int creatorId) {
            _ = await _unitOfWork.CreatorRepository.GetByIdAsync(creatorId)
            ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<List<MarkViewModel>> GetRecentMarksAsync(int creatorId)
        {
            const int LIMIT = 5;
            List<Mark> marks = await _unitOfWork.MarkRepository.GetMostRecentAsync(creatorId, LIMIT);
            return _mapper.Map<List<MarkViewModel>>(marks);
        }

        private async Task<List<CollectionViewModel>> GetStarredCollectionsAsync(int creatorId)
        {
            IReadOnlyList<Collection> collections = await _unitOfWork.CollectionRepository
                .GetAsync(c => 
                    c.CreatorId.Equals(creatorId) && c.IsFavorite,
                    c => c.OrderByDescending(c => c.CreatedDate)
                );
            return _mapper.Map<List<CollectionViewModel>>(collections);
        }

        private async Task<ActivityStats> GetActivityStatsAsync(int creatorId)
        {
            int DAYS_OF_THE_WEEK = Enum.GetValues(typeof(DayOfWeek)).Length;
            // Get global counts by creator
            int marksCount = await _unitOfWork.MarkRepository.CountByCreatorIdAsync(creatorId);
            int collectionsCount = await _unitOfWork.CollectionRepository.CountByCreatorIdAsync(creatorId);

            // Get mark stats of the week
            IReadOnlyList<Mark> marksOfTheWeek = await GetMarksOfTheCurrentWeekAsync(creatorId);
            Dictionary<DayOfWeek, int> dailyMarkActivity = new() {
                { DayOfWeek.Sunday, 0 },
                { DayOfWeek.Monday, 0 },
                { DayOfWeek.Tuesday, 0 },
                { DayOfWeek.Wednesday, 0 },
                { DayOfWeek.Thursday, 0 },
                { DayOfWeek.Friday, 0 },
                { DayOfWeek.Saturday, 0 }
            };

            var marksByDay = marksOfTheWeek
                    .Where(m => m.CreatedDate != null)
                    .GroupBy(m => ((DateTime)m.CreatedDate!).DayOfWeek)
                    .ToDictionary(g => g.Key, g => g.Count());

            // Update dailyMarkActivity with actual counts
            foreach (var kvp in marksByDay)
            {
                dailyMarkActivity[kvp.Key] = kvp.Value;
            }

            return new ActivityStats(
                marksCount,
                collectionsCount,
                TodayMarksCount : dailyMarkActivity[DateTime.Now.DayOfWeek], 
                WeekMarksCount  : dailyMarkActivity.Sum(md => md.Value),
                new WeeklyMarkActivity
                (
                    HighestTotal: dailyMarkActivity.Max(md => md.Value),
                    dailyMarkActivity
                )
            );
        }

        private async Task<IReadOnlyList<Mark>> GetMarksOfTheCurrentWeekAsync(int creatorId)
        {
            DateTime today = GetDateZeroTime(DateTime.UtcNow);
            DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
            return await _unitOfWork.MarkRepository
            .GetAsync(
                m => m.Collection != null && m.Collection.CreatorId.Equals(creatorId) && m.CreatedDate >= weekStart,
                m => m.OrderByDescending(m => m.CreatedDate)
            );
        }

        private static DateTime GetDateZeroTime(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}
