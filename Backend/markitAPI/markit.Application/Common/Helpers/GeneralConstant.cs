namespace markit.Application.Helpers
{
    public static class GeneralConstant
    {
        public static class Configuration
        {
            public static readonly string jwtSectionName = "JwtSettings";
            public static readonly string connStringSectionName = "ConnectionString";
            public static readonly string userDefaultSectionName = "UserDefaultSettings";
            public static readonly string googleAuthSectionName = "GoogleAuthSettings";
            public static readonly int DEFAULT_PAGINATION_PAGE_SIZE = 20;
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
            public static readonly string MARK_DEFAULT_PREVIEW = "New mark";
            public static readonly string MAIN_COLLECTION_NAME = "Workplace";
        }
    }
}
