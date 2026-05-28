﻿using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Contracts.Settings;
using markit.Application.Exceptions;
using markit.Application.Features.Account.Commands.CreateAccount;
using markit.Application.Features.Accounts.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Demo;
using markit.Application.Models.Authentication.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Services.Demo
{
    public class DemoService : IDemoService
    {
        private readonly ILogger<DemoService> _logger;
        private readonly IJwtService _jwtService;
        private readonly ISettingsService _settingsService;
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        private const int ACCOUNT_EXPIRATION_GRACE_PERIOD_IN_MINUTES = 5;

        private const string CONFIGURATION_ERROR_MESSAGE = "Demo mode is not available due to a configuration error.";
        private const string DEMO_MODE_DISABLED_ERROR_MESSAGE = "Demo mode is currently disabled.";

        private const string DEMO_SESSION_ATTEMPT_MESSAGE = "Demo session attempt.";
        private const string SUCCESSFUL_DEMO_SESSION_MESSAGE = "Successful demo session created.";
        private const string DEMO_MODE_DISABLED_MESSAGE = "Demo mode was disabled due to user demo availability or a role configuration error.";

        public DemoService(
            ILogger<DemoService> logger,
            IJwtService jwtService,
            ISettingsService settingsService,
            UserManager<AppUser> userManager,
            IMediator mediator
        )
        {
            _logger = logger;
            _jwtService = jwtService;
            _settingsService = settingsService;
            _userManager = userManager;
            _mediator = mediator;
        }

        public async Task<AccountVm> CreateSessionAsync(string guestName, HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(guestName))
                throw new CustomValidationException("Guest name is required.");

            CreateLogEntry(DEMO_SESSION_ATTEMPT_MESSAGE);
            if (!await IsDemoEnabledAsync()) throw new CustomValidationException(DEMO_MODE_DISABLED_ERROR_MESSAGE);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            AccountVm account = await CreateGuestAccountAsync(guestName);
            AppUser user = await _userManager.FindByIdAsync(account.UserId)
                ?? throw new NotFoundException("AppUsers", account.UserId);
            await EnsureGuestUserIsValidAsync(user);
            await AuthenticateDemoUserAsync(user, context);
            scope.Complete();

            return account;
        }

        public async Task<DemoStatus> GetStatusAsync()
        {
            bool isDemoEnabled = await IsDemoEnabledAsync();
            string message = isDemoEnabled 
                ? "Welcome to Demo!"
                : "Demo not available.";            
            return new DemoStatus(
                Available : isDemoEnabled,
                message
            );
        }

        #region Helpers
        private async Task<DateTime> GetSessionExpirationDateAsync()
        {
            string? demoTokenDurationInMinutesValue = await _settingsService
                .GetValueAsync(SystemConfigKeys.DEMO_SESSION_DURATION_IN_MINUTES);

            if (!int.TryParse(demoTokenDurationInMinutesValue, out int demoTokenDurationInMinutes))
                throw new CustomValidationException(CONFIGURATION_ERROR_MESSAGE);

            return DateTime.UtcNow.AddMinutes(demoTokenDurationInMinutes);
        }

        private async Task AuthenticateDemoUserAsync(AppUser user, HttpContext context)
        {
            DateTime expiresAt = await GetSessionExpirationDateAsync();
            await _jwtService.IssueAccessTokenAsync(user, context, expiresAt);
            CreateLogEntry(SUCCESSFUL_DEMO_SESSION_MESSAGE);
        }

        private async Task<AccountVm> CreateGuestAccountAsync(string name)
        {
            DateTime sessionExpiresAt = await GetSessionExpirationDateAsync();
            CreateAccountCommand command = new()
            {
                Name = name,
                UserName = EmailGenerator.GenerateDummyEmail(usernameLength: 10),
                AccessType = AccessType.External,
                Roles = [Role.GUEST_NAME],
                Enabled = true,
                ExpiresAt = sessionExpiresAt.AddMinutes(ACCOUNT_EXPIRATION_GRACE_PERIOD_IN_MINUTES)
            };

            return await _mediator.Send(command);
        }

        private async Task EnsureGuestUserIsValidAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Role.GUEST_NAME) || roles.Count != 1 || !user.Enabled)
            {
                await DisableDemoModeAsync();
                throw new CustomValidationException(CONFIGURATION_ERROR_MESSAGE);
            }
        }

        private async Task<bool> IsDemoEnabledAsync()
        {
            string? isDemoEnabledValue = await _settingsService.GetValueAsync(SystemConfigKeys.IS_DEMO_ENABLED_KEY);
            if (!bool.TryParse(isDemoEnabledValue, out bool isDemoEnabled)) return false;
            return isDemoEnabled;
        }

        private async Task DisableDemoModeAsync()
        {
            await _settingsService.UpdateAsync(SystemConfigKeys.IS_DEMO_ENABLED_KEY, "false");
            CreateLogEntry(DEMO_MODE_DISABLED_MESSAGE, LogLevel.Warning);
        }

        private void CreateLogEntry(string message, LogLevel level = LogLevel.Information)
        {
            _logger.Log(level, "{LogMessage}", message);
        }
        #endregion
    }
}
