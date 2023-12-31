﻿using Dapper;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.interfaces.repositories
{
    public interface ICountRepository
    {
        Task<int> GetCountItensAsync(string entity,string sqlFilter, DynamicParameters dbArgs, string serializedQuest, int userId);
    }
}
