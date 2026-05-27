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
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        private const string CONFIGURATION_ERROR_MESSAGE = "Demo mode is not available due to a configuration error.";
        private const string DEMO_MODE_DISABLED_ERROR_MESSAGE = "Demo mode is currently disabled.";

        private const string DEMO_SESSION_ATTEMPT_MESSAGE = "Demo session attempt.";
        private const string SUCCESSFUL_DEMO_SESSION_MESSAGE = "Successful demo session created.";
        private const string DEMO_MODE_DISABLED_MESSAGE = "Demo mode was disabled due to user demo availability or a role configuration error.";

        public DemoService(
            ILogger<DemoService> logger,
            IJwtService jwtService,
            ISettingsService settingsService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IMediator mediator
        )
        {
            _logger = logger;
            _jwtService = jwtService;
            _settingsService = settingsService;
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
        }

        public async Task<AppUser> CreateSessionAsync(string hostName, HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new CustomValidationException("Host name is required.");

            CreateLogEntry(DEMO_SESSION_ATTEMPT_MESSAGE);
            if (!await IsDemoEnabledAsync()) throw new CustomValidationException(DEMO_MODE_DISABLED_ERROR_MESSAGE);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            AccountVm account = await CreateHostAccountAsync(hostName);
            AppUser user = await _userManager.FindByIdAsync(account.UserId)
                ?? throw new NotFoundException("AppUsers", account.UserId);
            await EnsureDemoUserIsValidAsync(user);
            await AuthenticateDemoUserAsync(user, context);
            scope.Complete();

            return user;
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

        private async Task AuthenticateDemoUserAsync(AppUser user, HttpContext context)
        {
            await _jwtService.IssueDemoTokenAsync(user, context);
            await _signInManager.SignInAsync(user, isPersistent: false);
            CreateLogEntry(SUCCESSFUL_DEMO_SESSION_MESSAGE);
        }

        private async Task<AccountVm> CreateHostAccountAsync(string name)
        {
            CreateAccountCommand command = new()
            {
                Name = name,
                UserName = EmailGenerator.GenerateDummyEmail(usernameLength: 10),
                AccessType = AccessType.External,
                Roles = [Role.DEMO_NAME],
                Enabled = true
            };

            return await _mediator.Send(command);
        }

        private async Task EnsureDemoUserIsValidAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Role.DEMO_NAME) || roles.Count != 1 || !user.Enabled)
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
    }
}
