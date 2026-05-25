﻿namespace markit.Application.Helpers
{
    public static class GeneralConstant
    {
        public static class AuthenticationItems
        {
            public static readonly string LOGIN_PURPOSE_KEY = "LoginPurpose";
            public static readonly string CURRENT_USERID_KEY = "CurrentUserId";
        }

        public static class AuthorizationPolicies
        {
            public const string ADMIN_ONLY = "AdminOnly";
            public const string CAN_ESCALATE = "CanEscalate";
            public const string DEMO_ONLY = "DemoOnly";
        }

        public static class Configuration
        {
            public static readonly string JWT_SECTION_NAME = "JwtSettings";
            public static readonly string CONN_STRING_SECTION_NAME = "ConnectionString";
            public static readonly string USER_DEFAULT_SECTION_NAME = "UserDefaultSettings";
            public static readonly string GOOGLE_AUTH_SECTION_NAME = "GoogleAuthSettings";
            public static readonly string GITHUB_AUTH_SECTION_NAME = "GitHubAuthSettings";
            public static readonly string SPA_SECTION_NAME = "SpaSettings";
            public static readonly string DEMO_SECTION_NAME = "DemoSettings";
            public static readonly string RATE_LIMITING = "RateLimiting";
        }

        public static class CustomClaimType
        {
            public static readonly string ProfilePictureUrl = "prof_pic_url";
        }

        public static class Marks
        {
            public static readonly string COLLECTION_DEFAULT_PREVIEW = "0 marks";
            public static readonly string MARK_PLACEHOLDER = "New mark";
            public static readonly string MAIN_COLLECTION_NAME = "My marks";
        }

        public static class RateLimiterPolicies
        {
            public const string DEMO_LOGIN_QUOTA = "DemoLoginQuota";
        }

        public static class Role
        {
            public const string ADMIN_NAME = "Admin";
            public const string GENERAL_NAME = "General";
            public const string DEMO_NAME = "Demo";

            public static readonly string ADMIN_UUID = "5335f3ce-37dd-11ee-be56-0242ac120002";
            public static readonly string GENERAL_UUID = "6ff7edb4-37dd-11ee-be56-0242ac120002";
            public static readonly string DEMO_UUID = "3c2cba5a-562c-4098-9598-864e96f15397";

            public static readonly IReadOnlyList<string> All = new List<string>()
            {
                ADMIN_NAME,
                GENERAL_NAME,
                DEMO_NAME
            }.AsReadOnly();
        }

        public static class SystemConfigKeys
        {
            public const string IS_DEMO_ENABLED_KEY = "Demo:IsEnabled";
            public const string DEMO_USER_ID_KEY = "Demo:UserId";
            public const string DEMO_TOKEN_DURATION_IN_MINUTES_KEY = "Demo:TokenDurationInMinutes";
            public const string AUTH_IS_GOOGLE_ENABLED_KEY = "Auth:IsGoogleEnabled";
            public const string AUTH_IS_GITHUB_ENABLED_KEY = "Auth:IsGitHubEnabled";
        }

        public static class Token
        {
            public static readonly string ACCESS_TOKEN_NAME = "access_token";
            public static readonly string REFRESH_TOKEN_NAME = "refresh_token";
            public static readonly string EXPIRES_AT_TOKEN_NAME = "expires_at";
            public static readonly string LONG_LIVED_TOKEN_EXPIRES_AT_VALUE = "long-lived";
        }
    }
}
