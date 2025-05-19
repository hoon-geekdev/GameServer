namespace Constants
{
    public static class RedisDefine
    {
        public const int DB_ACCOUNT = 0;
        public const int DB_CHARACTER = 1;
        public const int DB_TABLE_DATA = 15;
    }

    public static class RedisKey
    {
        public static Func<string, string> JWT = (accountName) => $"{accountName}:JWT";
    }
}