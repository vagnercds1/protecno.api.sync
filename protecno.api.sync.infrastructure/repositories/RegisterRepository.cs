using Dapper;
using Dapper.Contrib.Extensions;
using protecno.api.sync.domain.common;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.register;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace protecno.api.sync.infrastructure.repositories
{
    public class RegisterRepository : IRepository<Register, RegisterPaginateRequest>
    {
        private readonly IDbConnection _dbConnection;
        public RegisterRepository(IDbConnection dbConnection) => _dbConnection = dbConnection;

        public async Task<List<Register>> GetPagintateAsync(RegisterPaginateRequest registerPaginateRequest, string sqlFilter, DynamicParameters dbArgs)
        {
            string pagination = PaginationExtensions.Paginate(
                pageSize: (int)registerPaginateRequest.PageSize,
                page: (int)registerPaginateRequest.Page,
                orderField: registerPaginateRequest.OrderField,
                order: registerPaginateRequest.Order);

            string query = $@" SELECT T.Id, 
                                      T.Codigo, 
                                      T.Descricao, 
                                      T.Ativo, 
                                      T.BaseInventarioId, 
                                      T.UsuarioId,
                                      U.Email Usuario,                                
                                      T.OrigemId,
                                      O.Nome Origem,
                                      T.DataCadastro,                  
                                      T.DataAtualizacao                                                                                 
	                          FROM db_protecno.{Enum.GetName(typeof(ERegisterType), registerPaginateRequest.TipoRegistroId)} T 
                      INNER JOIN db_protecno.usuario U on T.UsuarioId = U.Id 
                      INNER JOIN db_protecno.origem O on T.OrigemId = O.Id  
                      {sqlFilter} {pagination}; ";

            List<Register> listItens = (List<Register>)await _dbConnection.QueryAsync<Register>(sql: query, param: dbArgs);

            return listItens;
        }

        public async Task<List<Register>> GetListAsync(RegisterPaginateRequest filterRQ)
        {
            List<Register> listRegister = new();

            string sqlFilter = string.Empty;
            DynamicParameters dbArgs;
            FiltersExtentions<RegisterPaginateRequest>.FilterBuilder(filterRQ, CommonLists.BlackListFilterBuilderRegister.ToList(), out dbArgs, out sqlFilter);

            string query = @$"    SELECT T.Id, 
                                         T.Codigo, 
                                         T.Descricao, 
                                         T.Ativo, 
                                         T.BaseInventarioId, 
                                         T.UsuarioId,
                                         U.Email Usuario,
                                         T.OrigemId,
                                         O.Nome Origem,
                                         T.DataCadastro,                  
                                         T.DataAtualizacao                                                                                 
	                             FROM db_protecno.{Enum.GetName(typeof(ERegisterType), filterRQ.TipoRegistroId)} T 
                           INNER JOIN db_protecno.usuario U on T.UsuarioId = U.Id 
                           INNER JOIN db_protecno.origem O on T.OrigemId = O.Id ";
            query += $" {sqlFilter}  ";

            listRegister = (List<Register>)await _dbConnection.QueryAsync<Register>(sql: query, param: dbArgs);

            var aa = _dbConnection.State;
            return listRegister;
        }

        public List<Register> GetList(RegisterPaginateRequest filterRQ)
        {
            List<Register> listRegister = new();

            string sqlFilter = string.Empty;
            DynamicParameters dbArgs;
            FiltersExtentions<RegisterPaginateRequest>.FilterBuilder(filterRQ, CommonLists.BlackListFilterBuilderRegister.ToList(), out dbArgs, out sqlFilter);

            string query = @$"    SELECT T.Id, 
                                         T.Codigo, 
                                         T.Descricao, 
                                         T.Ativo, 
                                         T.BaseInventarioId, 
                                         T.UsuarioId,
                                         U.Email Usuario,
                                         T.OrigemId,
                                         O.Nome Origem,
                                         T.DataCadastro,                  
                                         T.DataAtualizacao                                                                                 
	                             FROM db_protecno.{Enum.GetName(typeof(ERegisterType), filterRQ.TipoRegistroId)} T 
                           INNER JOIN db_protecno.usuario U on T.UsuarioId = U.Id 
                           INNER JOIN db_protecno.origem O on T.OrigemId = O.Id ";
            query += $" {sqlFilter}  ";

            listRegister = (List<Register>)_dbConnection.Query<Register>(sql: query, param: dbArgs);

            var aa = _dbConnection.State;
            return listRegister;
        }
 
        public void Update(Register register)
        { 
                register.DataSync = DateTime.Now;
                string sql = $@" UPDATE db_protecno.{Enum.GetName(typeof(ERegisterType), register.TipoRegistroId)}  
                               SET Codigo = @Codigo, Descricao = @Descricao, Ativo = @Ativo, DataAtualizacao = @DataAtualizacao, DataSync = @DataSync
                             WHERE Id = @Id and BaseInventarioId = @BaseInventarioId";

                _dbConnection.Execute(sql, register);            
        }

        public int Insert(Register register)
        {
            register.DataSync = DateTime.Now;
            string insertQuery = $@" INSERT INTO db_protecno.{Enum.GetName(typeof(ERegisterType), register.TipoRegistroId)} 
                                     (Codigo, Descricao, Ativo, DataCadastro, BaseInventarioId, UsuarioId, OrigemId, DataSync) 
                                      VALUES
                                     (@Codigo, @Descricao, @Ativo, @DataCadastro, @BaseInventarioId, @UsuarioId, @OrigemId, @DataSync);
                                    
                                     SELECT LAST_INSERT_ID(); SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);";

            int id = _dbConnection.ExecuteScalar<int>(insertQuery, register);

            return id;
        }

        public int Delete(Register register)
        {
            string deleteQuery = $@" DELETE FROM db_protecno.{Enum.GetName(typeof(ERegisterType), register.TipoRegistroId)} WHERE ID = @Id ";

            return _dbConnection.Execute(deleteQuery, new { Id = register.Id });
        }
    }
}