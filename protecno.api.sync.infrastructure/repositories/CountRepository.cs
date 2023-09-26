using Dapper;
using protecno.api.sync.domain.common;
using protecno.api.sync.domain.helpers;
using protecno.api.sync.domain.interfaces.repositories;
using System;
using System.Data;
using System.Threading.Tasks;

namespace protecno.api.sync.infrastructure.repositories
{
    public class CountRepository : ICountRepository
    {
        private readonly IDbConnection _dbConnection;

        private readonly ICacheRepository _cacheRepository;

        public CountRepository(IDbConnection dbConnection, ICacheRepository cacheRepository)
        {
            _dbConnection = dbConnection;
            _cacheRepository = cacheRepository;
        }

        public async Task<int> GetCountItensAsync(string entity,string sqlFilter, DynamicParameters dbArgs, string serializedQuest, int userId)
        {
            int count = 0;
            string key = $"count{entity}:{userId}-{HashHelper.CreateHash<string>(serializedQuest)}";

            var result = _cacheRepository.GetKeyInMemory(key);

            if (!string.IsNullOrEmpty(result))
                return Convert.ToInt32(result);
                   
            string queryCount = $@" SELECT COUNT(*) FROM db_protecno.{entity} T {sqlFilter}";

            count = await _dbConnection.ExecuteScalarAsync<int>(sql: queryCount, param: dbArgs);

            _cacheRepository.SetKeyInMemory(key, count.ToString(), Constants.TTL_COUNT_MINUTES);

            return count;
        }
    }
}