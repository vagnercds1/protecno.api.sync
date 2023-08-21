using AutoMapper;
using Dapper;
using FluentValidation.Results;
using MySql.Data.MySqlClient;
using protecno.api.sync.domain.common;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.validations;
using ServiceStack.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Inventory, InventoryPaginateRequest> _inventoryRepository;
        private readonly IRegisterHelperService _registerHelper;
        private readonly IMapper _mapper;
        private readonly ICacheRepository _cacheRepository;
        private readonly ICountRepository _countRepository;

        public InventoryService(IUnitOfWork unitOfWork,
            IRepository<Inventory, InventoryPaginateRequest> inventoryRepository,
            IMapper mapper,
            IRegisterHelperService registerHelper,
            ICacheRepository cacheRepository,
            ICountRepository countRepository)
        {
            _unitOfWork = unitOfWork;
            _inventoryRepository = inventoryRepository;
            _cacheRepository = cacheRepository;
            _registerHelper = registerHelper;
            _mapper = mapper;
            _countRepository = countRepository;
        }

        public InventoryValidationResult InsertInventory(Inventory inventoryRQ, UserJwt userJWT, System.DateTime saveDate, bool insertRegistersWhenNotExists, bool fillAllRegisters)
        {
            InventoryValidationResult inventoryValidationResult = new();

            if (fillAllRegisters)
                inventoryRQ = _registerHelper.FillAllRegisters(inventoryRQ);

            if (insertRegistersWhenNotExists)
            {
                InsertResult insertResult = _registerHelper.InsertRegistersWhenNotExists(inventoryRQ, userJWT, saveDate, fillAllRegisters);

                if (!insertResult.Result.IsValid)
                {
                    inventoryValidationResult.Errors.AddRange(insertResult.Result.Errors);
                    return inventoryValidationResult;
                }
                else
                    inventoryRQ = _registerHelper.FillAllRegisters(inventoryRQ);
            }

            ValidationResult validationResult = new InventoryValidation().Validate(inventoryRQ);
            inventoryValidationResult = _mapper.Map<InventoryValidationResult>(validationResult);
            if (!inventoryValidationResult.IsValid)
                return inventoryValidationResult;

            inventoryRQ.Anotacoes = FillAnnotations(null, inventoryRQ.Mensagem, userJWT.Email, saveDate);

            inventoryRQ.StatusRegistroId = inventoryRQ.StatusRegistroId == null ? (int)EInventoryStatus.Unknown : inventoryRQ.StatusRegistroId;
            inventoryRQ.UsuarioId = userJWT.UserId;
            inventoryRQ.DataCadastro = saveDate;
            inventoryRQ.Incorporacao = inventoryRQ.Incorporacao == null ? 0 : inventoryRQ.Incorporacao;
            inventoryRQ.Id = _unitOfWork.InventoryRepository.Insert(inventoryRQ);

            inventoryValidationResult.InventoryObject = inventoryRQ;

            _cacheRepository.RemoveKeysInMemoryCacheByPartKey("count");
            return inventoryValidationResult;
        }

        public InventoryValidationResult UpdateInventory(Inventory originalInventory, Inventory updateInventory, UserJwt userJWT, DateTime saveDate, bool insertRegistersWhenNotExists, bool fillAllRegisters)
        {
            InventoryValidationResult inventoryValidationResult = new();

            if (fillAllRegisters)
                updateInventory = _registerHelper.FillAllRegisters(updateInventory);

            if (insertRegistersWhenNotExists)
            {
                InsertResult insertResult = _registerHelper.InsertRegistersWhenNotExists(updateInventory, userJWT, saveDate, fillAllRegisters);

                if (!insertResult.Result.IsValid)
                {
                    inventoryValidationResult.Errors.AddRange(insertResult.Result.Errors);
                    return inventoryValidationResult;
                }
                else
                    updateInventory = _registerHelper.FillAllRegisters(updateInventory);
            }

            ValidationResult validationResult = new InventoryValidation().Validate(updateInventory);
            inventoryValidationResult = _mapper.Map<InventoryValidationResult>(validationResult);
            if (!inventoryValidationResult.IsValid)
                return inventoryValidationResult;

            List<History> listHistoric = new();
            if (userJWT.StartedInventory)
                listHistoric = updateInventory.BuildHistoric<Inventory>(originalInventory,
                                                                        updateInventory,
                                                                        originalInventory.Id,
                                                                        (int)originalInventory.OrigemId,
                                                                        userJWT.UserId,
                                                                        Enum.GetName(typeof(EInventoryType), updateInventory.TipoInventarioId),
                                                                        CommonLists.InventoryFildsToCompare.ToList());

            updateInventory.Anotacoes = FillAnnotations(originalInventory.Anotacoes, updateInventory.Mensagem, userJWT.Email, saveDate);
            updateInventory.StatusRegistroId = listHistoric.Any() ? (int)EInventoryStatus.Changed : (int)EInventoryStatus.Reviewed;
            updateInventory.DataAtualizacao = saveDate;
            updateInventory.Id = originalInventory.Id;
            updateInventory.UsuarioId = originalInventory.UsuarioId;
            updateInventory.DataCadastro = originalInventory.DataCadastro;
            updateInventory.Incorporacao = updateInventory.Incorporacao == null ? 0 : updateInventory.Incorporacao;

            try
            {
                _unitOfWork.InventoryRepository.Update(updateInventory);

                if (listHistoric.Any())
                    listHistoric.ForEach(x => _unitOfWork.HistoryRepository.Insert(x));
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    inventoryValidationResult.Errors.Add(new ValidationFailure() { ErrorMessage = $"{$"Item: '{updateInventory.FilialCodigo}-{updateInventory.Codigo}-{updateInventory.Incorporacao}' já existe"}" });
                else
                    inventoryValidationResult.Errors.Add(new ValidationFailure() { ErrorMessage = $"{$"Erro ao atualizar item: '{updateInventory.FilialCodigo}-{updateInventory.Codigo}-{updateInventory.Incorporacao}'{Environment.NewLine}{ex.Message}"} " });
            }
            catch (Exception ex)
            {
                inventoryValidationResult.Errors.Add(new ValidationFailure() { ErrorMessage = $"{ex.Message} | {updateInventory.ToJsonString()}" });
            }

            inventoryValidationResult.InventoryObject = updateInventory;

            _cacheRepository.RemoveKeysInMemoryCacheByPartKey("count");
            return inventoryValidationResult;
        }

        private string FillAnnotations(string contentAnnotations, string message, string email, DateTime saveDate)
        {
            List<Annotation> listAnotacoes = contentAnnotations != null ? JsonSerializer.Deserialize<List<Annotation>>(contentAnnotations) : new List<Annotation>();

            if (!string.IsNullOrEmpty(message))
            {
                listAnotacoes.Add(new Annotation()
                {
                    EmailUsuario = email,
                    Data = saveDate,
                    Mensagem = message
                });
            }
            return listAnotacoes.ToJsonString();
        }

        public InventoryValidationResult RemoveInventory(InventoryDeleteRequest inventoryDeleteRequest, UserJwt userJwt, DateTime saveDate)
        {
            InventoryDeleteValidation validation = new InventoryDeleteValidation();
            ValidationResult result = validation.Validate(inventoryDeleteRequest);
            InventoryValidationResult resultValidation = _mapper.Map<InventoryValidationResult>(result);

            if (!resultValidation.IsValid)
                return resultValidation;

            int count = 0;
            if (inventoryDeleteRequest.DeleteAll)
                count = RemoveAll(inventoryDeleteRequest, userJwt, saveDate);
            else
                count = RemoveList(inventoryDeleteRequest, userJwt, saveDate);

            _cacheRepository.RemoveKeysInMemoryCacheByPartKey("count");

            resultValidation.Message = $"Foram removido {count} itens de inventário";
            return new InventoryValidationResult();
        }

        private int RemoveAll(InventoryDeleteRequest inventoryDeleteRequest, UserJwt userJwt, DateTime saveDate)
        {



            return 1;
        }

        private int RemoveList(InventoryDeleteRequest inventoryDeleteRequest, UserJwt userJwt, DateTime saveDate)
        { 
            int count = 0;
            foreach (int inventoryId in inventoryDeleteRequest.InventoryIdList)
            {
                var list = _inventoryRepository.GetList(new InventoryPaginateRequest()
                {
                    Id = inventoryId,
                    TipoInventarioId = inventoryDeleteRequest.TipoInventarioId,
                    BaseInventarioId = (Int32)inventoryDeleteRequest.BaseInventarioId
                });

                if (list != null)
                {
                    if (userJwt.StartedInventory)
                    {
                        Inventory updateInventory = list[0];
                        updateInventory.Ativo = false;

                        InventoryValidationResult updateResult = UpdateInventory(list.FirstOrDefault(), updateInventory, userJwt, saveDate, false, true);

                        if (updateResult.IsValid)
                            count++;
                    }
                    else
                    {
                        if (_inventoryRepository.Delete(list[0]) > 0)
                            count++;
                    }
                }
            }
            return count;
        }

        #region Save List Methods

        public SaveListResult SaveInventoryList(List<Inventory> listInventory, UserJwt userJWT, DateTime saveDate)
        {
            try
            {
                ProcessSave<Inventory> processSave = PrepareProcessingList(listInventory);

                _unitOfWork.BeginTransaction();

                processSave = UpdateList(listInventory, processSave, userJWT, saveDate);

                processSave = InsertList(processSave, userJWT, saveDate);

                _unitOfWork.Commit();

                _cacheRepository.RemoveKeysInMemoryCacheByPartKey($"count");

                return processSave.SaveListRS;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        private ProcessSave<Inventory> PrepareProcessingList(List<Inventory> listInventory)
        {
            ProcessSave<Inventory> processSave = new();
            foreach (Inventory inventory in listInventory)
            {
                inventory.Incorporacao = inventory.Incorporacao == null ? 0 : (int)inventory.Incorporacao;
                var filledInventory = _registerHelper.FillAllRegisters(inventory);

                ValidationResult validationResult = new InventoryValidation().Validate(inventory);
                InventoryValidationResult inventoryValidationResult = _mapper.Map<InventoryValidationResult>(validationResult);

                if (inventoryValidationResult.IsValid)
                {
                    var foudItem = _inventoryRepository.GetList(new InventoryPaginateRequest()
                    {
                        FilialId = (int)filledInventory.FilialId,
                        Codigo = filledInventory.Codigo,
                        Incorporacao = filledInventory.Incorporacao == null ? 0 : (int)filledInventory.Incorporacao,
                        BaseInventarioId = filledInventory.BaseInventarioId
                    }).FirstOrDefault();

                    if (foudItem != null)
                        processSave.ListFoundItens.Add(foudItem);
                    else
                        processSave.ListNewItens.Add(inventory);
                }
                else
                {
                    string messages = String.Join(", ", inventoryValidationResult.Errors.Select(x => x.ErrorMessage).ToList());

                    processSave.SaveListRS.DiscartedItens.Add(new { DiscartedItemRQ = inventory, messages });
                    processSave.SaveListRS.QuantityDiscarded++;
                }
            }
            return processSave;
        }

        private ProcessSave<Inventory> InsertList(ProcessSave<Inventory> processSave, UserJwt userJWT, DateTime saveDate)
        {
            foreach (var inventory in processSave.ListNewItens)
            {
                ValidationResult result = new();
                var resultInsert = InsertInventory(inventory, userJWT, saveDate, true, false);
                result.Errors.AddRange(resultInsert.Errors);

                if (result.IsValid)
                    processSave.SaveListRS.QuantityInserted++;
            }
            return processSave;
        }

        private ProcessSave<Inventory> UpdateList(List<Inventory> listInventory, ProcessSave<Inventory> processSave, UserJwt userJWT, DateTime saveDate)
        {
            foreach (var updateInventory in processSave.ListFoundItens)
            {
                ValidationResult result = new();
                var newValues = listInventory.Find(x => x.Codigo == updateInventory.Codigo && x.FilialCodigo == updateInventory.FilialCodigo && x.Incorporacao == updateInventory.Incorporacao);

                var resultInsert = UpdateInventory(updateInventory, newValues, userJWT, saveDate, true, false);
                result.Errors.AddRange(resultInsert.Errors);

                if (result.IsValid)
                    processSave.SaveListRS.QuantityUpdated++;
            }
            return processSave;
        }

        #endregion

        #region Paginate Methods
       
        public async Task<ContainerResult<Inventory>> GetPagintateAsync(InventoryPaginateRequest inventoryRQ, int userId)
        {
            string sqlFilter = string.Empty;
            DynamicParameters dbArgs;
            FiltersExtentions<InventoryPaginateRequest>.FilterBuilder(inventoryRQ, CommonLists.BlackListFilterBuilderRegister.ToList(), out dbArgs, out sqlFilter);

            List<Inventory> listItens = await _inventoryRepository.GetPagintateAsync(inventoryRQ, sqlFilter, dbArgs);

            int countItens = await GetCountAsync(inventoryRQ, sqlFilter, dbArgs, userId);

            var result = new ContainerResult<Inventory>()
            {
                Count = countItens,
                ListItens = listItens
            };

            return result;
        }

        private async Task<int> GetCountAsync(InventoryPaginateRequest inventoryRQ, string sqlFilter, DynamicParameters dbArgs, int userId)
        {
            //This query will leave here and go to the Repository Service
            string queryCount = $"SELECT COUNT(*) FROM db_protecno.inventario T {sqlFilter}";
            var inventoryRqClone = inventoryRQ.Clone<InventoryPaginateRequest>();
            inventoryRqClone.Page = 0;
            inventoryRqClone.PageSize = 0;

            return await _countRepository.GetCountItensAsync(queryCount, dbArgs, inventoryRqClone.ToJsonString(), userId, "countInventory");
        }

        #endregion
    }
}
