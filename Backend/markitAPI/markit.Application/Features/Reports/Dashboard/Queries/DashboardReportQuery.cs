using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Application.Features.Reports.Dashboard.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace markit.Application.Features.Reports.Dashboard.Queries
{
    public class DashboardReportQuery(string userId, string clientTimeZone) : IRequest<DashboardReportVm>
    {
        public string UserId { get; private set; } = userId;
        public string ClientTimeZone { get; private set; } = clientTimeZone;
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
            List<NotebookViewModel> recentNotebooks = await GetRecentNotebooksAsync(request.UserId);
            List<CollectionViewModel> starredCollections = await GetStarredCollectionsAsync(request.UserId);
            var activityStats = await GetActivityStatsAsync(request.UserId, request.ClientTimeZone);

            return new DashboardReportVm(
                UserId : request.UserId,
                CreatedAt : DateTime.Now,
                recentNotebooks,
                starredCollections,
                activityStats
            );
        }

        private async Task ValidateUserExistence(string userId)
        {
            _ = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("AppUser", userId);
        }

        private async Task<List<NotebookViewModel>> GetRecentNotebooksAsync(string userId)
        {
            const int LIMIT = 5;
            List<Notebook> notebooks = await _unitOfWork.NotebookRepository.GetMostRecentAsync(userId, LIMIT);
            return _mapper.Map<List<NotebookViewModel>>(notebooks);
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

        private async Task<ActivityStats> GetActivityStatsAsync(string userId, string timeZone)
        {
            int DAYS_OF_THE_WEEK = Enum.GetValues<DayOfWeek>().Length;
            
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out TimeZoneInfo? clientTimeZoneInfo))
            {
                clientTimeZoneInfo = TimeZoneInfo.Utc;
            }
            
            // Get global counts by user
            int notebooksCount = await _unitOfWork.NotebookRepository.CountByUserIdAsync(userId);
            int collectionsCount = await _unitOfWork.CollectionRepository.CountByUserIdAsync(userId);

            // Get notebook stats of the week
            IReadOnlyList<Notebook> notebooksOfTheWeek = await GetNotebooksOfTheCurrentWeekAsync(userId, clientTimeZoneInfo);
            Dictionary<DayOfWeek, int> dailyNotebookActivity = new() {
                { DayOfWeek.Sunday, 0 },
                { DayOfWeek.Monday, 0 },
                { DayOfWeek.Tuesday, 0 },
                { DayOfWeek.Wednesday, 0 },
                { DayOfWeek.Thursday, 0 },
                { DayOfWeek.Friday, 0 },
                { DayOfWeek.Saturday, 0 }
            };

            IDictionary<DayOfWeek, int> notebooksByDay = notebooksOfTheWeek
                    .Where(m => m.CreatedDate != null)
                    .GroupBy(m => TimeZoneInfo.ConvertTimeFromUtc(m.CreatedDate!.Value, clientTimeZoneInfo).DayOfWeek)
                    .ToDictionary(g => g.Key, g => g.Count());

            // Update dailyNotebookActivity with actual counts
            foreach (var kvp in notebooksByDay)
            {
                dailyNotebookActivity[kvp.Key] = kvp.Value;
            }

            return new ActivityStats(
                notebooksCount,
                collectionsCount,
                TodayNotebooksCount : dailyNotebookActivity[DateTime.Now.DayOfWeek], 
                WeekNotebooksCount  : dailyNotebookActivity.Sum(md => md.Value),
                new WeeklyNotebookActivity
                (
                    HighestTotal: dailyNotebookActivity.Max(md => md.Value),
                    dailyNotebookActivity
                )
            );
        }

        private async Task<IReadOnlyList<Notebook>> GetNotebooksOfTheCurrentWeekAsync(string userId, TimeZoneInfo clientTimeZoneinfo)
        {
            DateTime clientNow = GetDateZeroTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clientTimeZoneinfo));
            DateTime clientWeekStart = DateTime.SpecifyKind(clientNow.AddDays(-(int)clientNow.DayOfWeek), DateTimeKind.Unspecified);
            DateTime weekStartUtc = TimeZoneInfo.ConvertTimeToUtc(clientWeekStart, clientTimeZoneinfo);

            return await _unitOfWork.NotebookRepository
            .GetAsync(
                m => m.Collection != null && m.Collection.UserId.Equals(userId) && m.CreatedDate >= weekStartUtc,
                m => m.OrderByDescending(m => m.CreatedDate)
            );
        }

        private static DateTime GetDateZeroTime(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}
