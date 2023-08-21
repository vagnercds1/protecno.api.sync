using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace protecno.api.sync.infrastructure.Factories
{
    public interface IDbConnectionFactory
    {
        IDbConnection GetConnection(string connectionStringName);
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        readonly IConfiguration _configuration;
        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection GetConnection(string connectionStringName)
        {
            if (string.IsNullOrEmpty(connectionStringName) || string.IsNullOrEmpty(_configuration.GetConnectionString(connectionStringName)))
                throw new Exception("Connection not identified");
          
            return new MySqlConnection(_configuration.GetConnectionString(connectionStringName));
        }
    }
}
