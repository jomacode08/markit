namespace markit.Application.Helpers
{
    public static class GeneralConstant
    {
        public static class Configuration
        {
            public static readonly string JWT_SECTION_NAME = "JwtSettings";
            public static readonly string CONN_STRING_SECTION_NAME = "ConnectionString";
            public static readonly string USER_DEFAULT_SECTION_NAME = "UserDefaultSettings";
            public static readonly string GOOGLE_AUTH_SECTION_NAME = "GoogleAuthSettings";
            public static readonly string GITHUB_AUTH_SECTION_NAME = "GitHubAuthSettings";
            public static readonly string MEILISEARCH_SECTION_NAME = "MeiliSearchSettings";
            public static readonly string SPA_SECTION_NAME = "SpaSettings";
        }

        public static class AuthenticationItems
        {
            public static readonly string LOGIN_PURPOSE_KEY = "LoginPurpose";
            public static readonly string CURRENT_USERID_KEY = "CurrentUserId";
        }

        public static class Token
        {
            public static readonly string ACCESS_TOKEN_NAME = "access_token";
            public static readonly string REFRESH_TOKEN_NAME = "refresh_token";
            public static readonly string EXPIRES_AT_TOKEN_NAME = "expires_at";
        }

        public static class Role
        {
            public const string admin = "Admin";
            public const string general = "General";

            public static readonly string adminUuid = "5335f3ce-37dd-11ee-be56-0242ac120002";
            public static readonly string generalUuid = "6ff7edb4-37dd-11ee-be56-0242ac120002";
        }

        public static class CustomClaimType
        {
            public static readonly string ProfilePictureUrl = "prof_pic_url";
            public static readonly string CreatorId = "creatorId";
        }

        public static class Marks
        {
            public static readonly string COLLECTION_DEFAULT_PREVIEW = "0 marks";
            public static readonly string MARK_PLACEHOLDER = "New mark";
            public static readonly string MAIN_COLLECTION_NAME = "My marks";
        }

        public static class MeiliSearch
        {
            public static readonly string INDEX_PRIMARY_KEY_NAME = "id";
            public static readonly string COLLECTION_INDEX_UID = "collections";
            public static readonly string MARK_INDEX_UID = "marks";
        }
    }
}
