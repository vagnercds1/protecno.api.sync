using Dapper;
using Dapper.Contrib.Extensions;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.interfaces.repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace protecno.api.sync.infrastructure.repositories
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly IDbConnection _dbConnection;
        public HistoryRepository(IDbConnection dbConnection) => _dbConnection = dbConnection;

        public async Task<List<History>> GetHistoryListAsync(History history)
        {
            string query = @" SELECT h.TipoRegistro 
		                            ,h.RegistroId 
		                            ,h.CampoAlterado 
		                            ,h.De 
		                            ,h.Para 
		                            ,h.Mensagem 
		                            ,h.DataAtualizacao 
		                            ,h.UsuarioId  
                                    ,u.Email Usuario
		                            ,h.OrigemId 
                                    ,o.Nome Origem
                                    ,h.TransactionId
	                            FROM db_protecno.historico h
                          INNER JOIN db_protecno.usuario u on h.UsuarioId = u.Id
                          INNER JOIN db_protecno.origem o on h.OrigemId = o.Id
                               WHERE TipoRegistro = @TipoRegistro
                                 AND RegistroId = @RegistroId ";

            return (List<History>)await _dbConnection.QueryAsync<History>(query, new { TipoRegistro = history.TipoRegistro, RegistroId = history.RegistroId });
        }

        public void Insert(History item)
        {
            _dbConnection.Insert(item);
        }
    }
}
