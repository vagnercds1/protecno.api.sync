using Dapper;
using Dapper.Contrib.Extensions;
using protecno.api.sync.domain.common;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.inventory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace protecno.api.sync.infrastructure.repositories
{
    public class InventoryRepository : IRepository<Inventory, InventoryPaginateRequest>
    {
        private readonly IDbConnection _dbConnection;

        public InventoryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private static string queryConsulta = @" 
                              SELECT T.Id,
                                     T.BaseInventarioId,
                                     T.TipoInventarioId,
                                     TI.Nome AS TipoInventario,                                     
                                     T.Codigo,
                                     T.CodigoAnterior,
                                     T.Incorporacao,
                                     T.IncorporacaoAnterior,
                                     T.Descricao,
                                     T.DescricaoComplemento,
                                     T.Marca,
                                     T.Modelo,
                                     T.Serie,
                                     T.TAG,
                                     T.Observacao,
                                     T.Auxiliar1,
                                     T.Auxiliar2,
                                     T.Auxiliar3,
                                     T.Auxiliar4,
                                     T.Auxiliar5,
                                     T.Auxiliar6,
                                     T.Auxiliar7,
                                     T.Auxiliar8,
                                     T.StatusRegistroId,
                                     T.CondicaoUso, 
                                     T.Documento,
                                     T.Quantidade,                                         
                                     T.DataCadastro,
                                     T.DataAtualizacao, 
                                     T.Propriedades,
                                     T.VidaUtilMeses,
                                     T.VidaUtilAnos,
                                     T.PercentualRemanescenteVidaUtil,
                                     T.MesesRemanecentes,
                                     T.AnosRemanescentes,
                                     T.ValorResidual,
                                     T.ValorDepreciavel,
                                     T.ValorEmUso,
                                     T.TesteRecuperabilidade,
                                     T.AnoFabricacao,
                                     T.ValorOriginal,
                                     T.DepreciacaoAcumulada,
                                     T.CustoReposicao,
                                     T.DataAquisicao,
                                     T.DataEntradaOperacao,
	                                 T.UsuarioId,
	                                 U.Email Usuario,
	                                 T.OrigemId,
	                                 O.Nome Origem,
                                     T.Anotacoes,
                                     T.Ativo,                                     
                                     T.FilialId,
                                     F.Descricao AS Filial,
                                     F.Codigo AS FilialCodigo,
                                     T.FilialIdAnterior,
                                     FA.Codigo AS FilialAnteriorCodigo,
                                     FA.Descricao AS FilialAnterior,
                                     T.CentroCustoId,
	                                 CC.Descricao AS CentroCusto,
                                     CC.Codigo AS CentroCustoCodigo,
                                     T.LocalId,
                                     L.Descricao AS Local,
                                     L.Codigo AS LocalCodigo,                    
                                     T.ResponsavelId,                    
                                     R.Codigo AS ResponsavelCodigo,
                                     R.Descricao AS Responsavel,
                                     T.ContaContabilId,
                                     COC.Codigo AS ContaContabilCodigo,
                                     COC.Descricao AS ContaContabil
                                FROM db_protecno.inventario T 
                          INNER JOIN db_protecno.tipoinventario TI ON T.TipoInventarioId = TI.Id
                          INNER JOIN db_protecno.filial F ON F.Id = T.FilialId
                           LEFT JOIN db_protecno.filial FA ON FA.Id = T.FilialIdAnterior
                           LEFT JOIN db_protecno.centrocusto CC ON T.CentroCustoId = CC.Id
                           LEFT JOIN db_protecno.local L ON T.LocalId = L.Id
                           LEFT JOIN db_protecno.responsavel R ON T.ResponsavelId = R.Id
                           LEFT JOIN db_protecno.contaContabil COC ON T.ContaContabilId = COC.Id
                          INNER JOIN db_protecno.usuario U on T.UsuarioId = U.Id 
                          INNER JOIN db_protecno.origem O on T.OrigemId = O.Id ";

        public async Task<List<Inventory>> GetPagintateAsync(InventoryPaginateRequest inventoryRQ, string sqlFilter, DynamicParameters dbArgs)
        {
            string pagination = PaginationExtensions.Paginate(
                pageSize: (int)inventoryRQ.PageSize,
                page: (int)inventoryRQ.Page,
                orderField: inventoryRQ.OrderField,
                order: inventoryRQ.Order);

            string query = queryConsulta +
                    $" {sqlFilter} {pagination}";

            List<Inventory> listItens = (List<Inventory>)await _dbConnection.QueryAsync<Inventory>(sql: query, param: dbArgs);

            return listItens;
        }

        public async Task<List<Inventory>> GetListAsync(InventoryPaginateRequest filterRQ)
        {
            List<Inventory> listItens = new();

            string sqlFilter = string.Empty;
            DynamicParameters dbArgs;
            FiltersExtentions<InventoryPaginateRequest>.FilterBuilder(filterRQ, CommonLists.BlackListFilterBuilderInventory.ToList(), out dbArgs, out sqlFilter);

            string query = queryConsulta;
            query += $" {sqlFilter}  ";

            listItens = (List<Inventory>)await _dbConnection.QueryAsync<Inventory>(sql: query, param: dbArgs);

            return listItens;
        }

        public List<Inventory> GetList(InventoryPaginateRequest filterRQ)
        {
            string sqlFilter = string.Empty;
            DynamicParameters dbArgs;
            FiltersExtentions<InventoryPaginateRequest>.FilterBuilder(filterRQ, CommonLists.BlackListFilterBuilderInventory.ToList(), out dbArgs, out sqlFilter);

            string query = queryConsulta;
            query += $" {sqlFilter}  ";

            List<Inventory> listItens = (List<Inventory>)_dbConnection.Query<Inventory>(sql: query, param: dbArgs);

            return listItens;
        }

        public void Update(Inventory inventory)
        {
            inventory.DataSync = DateTime.Now;

            _dbConnection.Update<Inventory>(inventory);
        }

        public int Insert(Inventory inventory)
        {
            inventory.DataSync = DateTime.Now;
            return (int)_dbConnection.Insert<Inventory>(inventory);
        }

        public int Delete(Inventory inventory)
        {
            return _dbConnection.Delete(inventory) ?1:0;
        }
    }
}