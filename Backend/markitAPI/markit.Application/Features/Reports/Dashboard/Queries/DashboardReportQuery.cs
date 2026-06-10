using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Application.Features.Reports.Dashboard.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace markit.Application.Features.Reports.Dashboard.Queries
{
    public class DashboardReportQuery(string userId) : IRequest<DashboardReportVm>
    {
        public string UserId { get; set; } = userId;
    }

    public class DashboardReportQueryHandler : IRequestHandler<DashboardReportQuery, DashboardReportVm>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public DashboardReportQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<DashboardReportVm> Handle(DashboardReportQuery request, CancellationToken cancellationToken)
        {
            await ValidateUserExistence(request.UserId);
            List<MarkViewModel> recentMarks = await GetRecentMarksAsync(request.UserId);
            List<CollectionViewModel> starredCollections = await GetStarredCollectionsAsync(request.UserId);
            var activityStats = await GetActivityStatsAsync(request.UserId);

            return new DashboardReportVm(
                UserId : request.UserId,
                CreatedAt : DateTime.Now,
                recentMarks,
                starredCollections,
                activityStats
            );
        }

        private async Task ValidateUserExistence(string userId)
        {
            _ = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("AppUser", userId);
        }

        private async Task<List<MarkViewModel>> GetRecentMarksAsync(string userId)
        {
            const int LIMIT = 5;
            List<Notebook> marks = await _unitOfWork.MarkRepository.GetMostRecentAsync(userId, LIMIT);
            return _mapper.Map<List<MarkViewModel>>(marks);
        }

        private async Task<List<CollectionViewModel>> GetStarredCollectionsAsync(string userId)
        {
            IReadOnlyList<Collection> collections = await _unitOfWork.CollectionRepository
                .GetAsync(c => 
                    c.UserId.Equals(userId) && c.IsFavorite,
                    c => c.OrderByDescending(c => c.CreatedDate)
                );
            return _mapper.Map<List<CollectionViewModel>>(collections);
        }

        private async Task<ActivityStats> GetActivityStatsAsync(string userId)
        {
            int DAYS_OF_THE_WEEK = Enum.GetValues(typeof(DayOfWeek)).Length;
            // Get global counts by user
            int marksCount = await _unitOfWork.MarkRepository.CountByUserIdAsync(userId);
            int collectionsCount = await _unitOfWork.CollectionRepository.CountByUserIdAsync(userId);

            // Get mark stats of the week
            IReadOnlyList<Notebook> marksOfTheWeek = await GetMarksOfTheCurrentWeekAsync(userId);
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

        private async Task<IReadOnlyList<Notebook>> GetMarksOfTheCurrentWeekAsync(string userId)
        {
            DateTime today = GetDateZeroTime(DateTime.UtcNow);
            DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
            return await _unitOfWork.MarkRepository
            .GetAsync(
                m => m.Collection != null && m.Collection.UserId.Equals(userId) && m.CreatedDate >= weekStart,
                m => m.OrderByDescending(m => m.CreatedDate)
            );
        }

        private static DateTime GetDateZeroTime(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}
