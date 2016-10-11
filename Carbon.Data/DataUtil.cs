using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
#if MYSQL
using MySql.Data.MySqlClient;
#endif
using System.Data.SqlClient;
using Carbon.Business;

namespace Carbon.Data
{    
    public class DataUtil
    {
        private readonly AppSettings _appSettings;

        public DataUtil(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public static DbConnection GetConnection(string providerName, string connectionString)
        {
            if (providerName != null)
            {
                var providerExists = DbProviderFactories.GetFactoryClasses().Rows.Cast<DataRow>().Any(r => r[2].Equals(providerName));
                if (providerExists)
                {
                    var factory = DbProviderFactories.GetFactory(providerName);
                    var connection = factory.CreateConnection();

                    connection.ConnectionString = connectionString;

                    return connection;
                }                
            }
#if MYSQL
            return new MySqlConnection(connectionString);
#else 
            return new SqlConnection(connectionString);
#endif
        }
        public void Execute(string sqlText, Dictionary<string, object> parameters = null)
        {
            Execute(sqlText, _appSettings.DbProviderName, GetConnectionString(), parameters);
        }

        public void Execute(string sqlText, string connectionString, Dictionary<string, object> parameters = null)
        {
            Execute(sqlText, _appSettings.DbProviderName, connectionString, parameters);
        }

        public void Execute(string sqlText, string providerName, string connectionString, Dictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection(providerName, connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = sqlText;
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        var param = command.CreateParameter();
                        param.ParameterName = p.Key;
                        param.Value = p.Value;
                        command.Parameters.Add(param);
                    }
                }
                command.ExecuteNonQuery();
            }
        }

        public string GetConnectionString()
        {
            return _appSettings.GetConnectionString(StorageTypeConfig.GetConnectionStringName());
        }
    }
}
