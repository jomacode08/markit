﻿using markit.Application.Common.Helpers;
using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Contracts.Settings;
using markit.Application.Exceptions;
using markit.Application.Features.Settings.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Features.Settings.Commands.UpdateDemoSettings
{
    public class UpdateDemoSettingsCommand : IRequest<DemoSettings>
    {
        public bool IsEnabled { get; set; }
        public int SessionDurationInMinutes { get; set; }
        public string UserTemplateId { get; set; } = string.Empty;
    }

    public class UpdateDemoSettingsCommandHandler : IRequestHandler<UpdateDemoSettingsCommand, DemoSettings>
    {
        private readonly ISettingsService _settingsService;
        private readonly IDemoCleanUpWorker _demoCleanUpWorker;
        private readonly UserManager<AppUser> _userManager;

        public UpdateDemoSettingsCommandHandler(
            ISettingsService settingsService,
            UserManager<AppUser> userManager, IDemoCleanUpWorker demoCleanUpWorker)
        {
            _settingsService = settingsService;
            _userManager = userManager;
            _demoCleanUpWorker = demoCleanUpWorker;
        }

        public async Task<DemoSettings> Handle(UpdateDemoSettingsCommand request, CancellationToken cancellationToken)
        {
            IReadOnlyList<SystemConfig> existentConfigs = await _settingsService.GetForDemoAsync();
            bool isEnabledSettingHasChanged = HasIsEnabledSettingChanged(existentConfigs, newValue: request.IsEnabled.ToString());
            Dictionary<string, string> requestedConfigValues = new()
            {
                { SystemConfigKeys.IS_DEMO_ENABLED_KEY, request.IsEnabled.ToString() },
                { SystemConfigKeys.DEMO_SESSION_DURATION_IN_MINUTES, request.SessionDurationInMinutes.ToString() },
                { SystemConfigKeys.DEMO_USER_TEMPLATE_ID, request.UserTemplateId }
            };

            if (!string.IsNullOrWhiteSpace(request.UserTemplateId))
            {
                await ValidateUserTemplate(userId: request.UserTemplateId);
            }

            await SyncSystemConfigs(existentConfigs, requestedConfigValues);

            if (isEnabledSettingHasChanged) _demoCleanUpWorker.Trigger();

            return new DemoSettings
            {
                IsEnabled = request.IsEnabled,
                SessionDurationInMinutes = request.SessionDurationInMinutes,
                UserTemplateId = request.UserTemplateId
            };        
        }

        private async Task ValidateUserTemplate(string userId)
        {
            AppUser user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("Users", userId);
            IList<string> roles = await _userManager.GetRolesAsync(user);

            if (!roles.Contains(Role.GUEST_NAME) || roles.Count != 1 || !user.Enabled)
            {
                throw new CustomValidationException("The provided user is not available or is invalid.");
            }
        }

        private async Task SyncSystemConfigs(
            IReadOnlyList<SystemConfig> existentConfigs,
            Dictionary<string,string> requestedConfigValues
        )
        {
            List<SystemConfig> configsToAdd = [];
            List<SystemConfig> configsToUpdate = [];

            foreach (var (key, value) in requestedConfigValues)
            {
                AddOrUpdateConfigInLists(
                    key, 
                    value,
                    existentConfigs,
                    configsToAdd, 
                    configsToUpdate
                );
            }

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            if (configsToAdd.Count > 0)
                await _settingsService.AddRangeAsync(configsToAdd);
            if (configsToUpdate.Count > 0)
                await _settingsService.UpdateRangeAsync(configsToUpdate);
            scope.Complete();
        }

        private static void AddOrUpdateConfigInLists(
            string key,
            string value,
            IReadOnlyList<SystemConfig> existentConfigs,
            List<SystemConfig> configsToAdd,
            List<SystemConfig> configsToUpdate
        )
        {
            SystemConfig? config = GetConfig(key, existentConfigs);
            // Config was not found, add is required.
            if (config is null) {
               config = ConstructConfig(key, value);
               configsToAdd.Add(config);
            }
            // Config was found and its value changed, update is required.
            else if (config.Value != value)
            {
                config.Value = value;
                configsToUpdate.Add(config);
            }
        }

        private static SystemConfig? GetConfig(string key, IReadOnlyList<SystemConfig> configs)
        {
            return configs.FirstOrDefault(s => s.Id.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        private static SystemConfig ConstructConfig(string key, string value) {
            return new SystemConfig
            {
                Id = key,
                Value = value,
                Description = Utilities.GetSystemConfigDescription(key)
            };
        }

        private static bool HasIsEnabledSettingChanged(IReadOnlyList<SystemConfig> existentConfigs, string newValue)
        {
            SystemConfig? isEnabled = existentConfigs.FirstOrDefault(c => c.Id == SystemConfigKeys.IS_DEMO_ENABLED_KEY);
            if (isEnabled == null) return newValue.Equals("true", StringComparison.OrdinalIgnoreCase);
            return isEnabled.Value != newValue;
        }
    }
}
