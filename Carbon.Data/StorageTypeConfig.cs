namespace Carbon.Data
{
    public class StorageTypeConfig
    {
        public static string GetConnectionStringName()
        {
            if (IsMySql())
            {
                return "mysql";
            }
            return "sql";
        }

        public static bool IsMySql()
        {
#if MYSQL
            return true;
#else
            return false;
#endif
        }
    }
}