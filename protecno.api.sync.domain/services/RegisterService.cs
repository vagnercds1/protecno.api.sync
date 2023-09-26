using AutoMapper;
using Dapper;
using FluentValidation.Results;
using MySql.Data.MySqlClient;
using protecno.api.sync.domain.common;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.helpers;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.models.register;
using protecno.api.sync.domain.validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace protecno.api.sync.domain.services
{
    public class RegisterService : IRegisterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Register, RegisterPaginateRequest> _registerRepository;
        private readonly IMapper _mapper;
        private readonly ICountRepository _countRepository;
        private readonly ICacheRepository _cacheRepository;

        public RegisterService(IUnitOfWork unitOfWork, IRepository<Register, RegisterPaginateRequest> registerRepository, IMapper mapper, ICountRepository countRepository, ICacheRepository cacheRepository)
        {
            _registerRepository = registerRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _countRepository = countRepository;
            _cacheRepository = cacheRepository;
        }

        public RegisterValidationResult InsertRegister(Register register, UserJwt userJwt, DateTime saveDate)
        {
            ValidationResult validationResult = new RegisterValidation().Validate(register);
            RegisterValidationResult registerValidationResult = _mapper.Map<RegisterValidationResult>(validationResult);

            if (!registerValidationResult.IsValid)
                return registerValidationResult;

            register.UsuarioId = userJwt.UserId;
            register.DataCadastro = saveDate;
            register.Ativo = true;
            register.Id = _unitOfWork.RegisterRepository.Insert(register);

            registerValidationResult.RegisterObject = register;
            string entityName = EnumHelper.GetEnumTitleCase<EInformationType>((EInformationType)register.InformationType);
            
            _cacheRepository.RemoveKeysInMemoryCacheByPartKey($"count{entityName}"); 
            return registerValidationResult;
        }

        public RegisterValidationResult UpdateRegister(Register originalRegister, Register updateRegister, UserJwt userJwt, DateTime saveDate)
        {
            string registerName = EnumHelper.GetEnumTitleCase<EInformationType>((EInformationType)updateRegister.InformationType);

            RegisterValidationResult resultValidation = new();
            originalRegister ??= new Register();
            RegisterValidation validation = new();

            ValidationResult validationResult = validation.Validate(updateRegister);
            resultValidation = _mapper.Map<RegisterValidationResult>(validationResult);
            if (!resultValidation.IsValid)
                return resultValidation;

            List<History> listHistoric = new();
            if (userJwt.StartedInventory)
                listHistoric = updateRegister.BuildHistoric<Register>(originalRegister,
                                                                      updateRegister,
                                                                      (int)originalRegister.Id,
                                                                      (int)updateRegister.OrigemId,
                                                                      userJwt.UserId,
                                                                      Enum.GetName(typeof(EInformationType), updateRegister.InformationType),
                                                                      CommonLists.RegisterFildsToCompare.ToList());
            try
            {
                updateRegister.DataAtualizacao = saveDate;

                _unitOfWork.RegisterRepository.Update(updateRegister);

                if (listHistoric.Any())
                    listHistoric.ForEach(x => _unitOfWork.HistoryRepository.Insert(x));

                resultValidation.RegisterObject = updateRegister;
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    resultValidation.Errors.Add(new ValidationFailure() { ErrorMessage = $"{registerName} código: '{updateRegister.Codigo}' já existe." });
                else
                {
                    resultValidation.Errors.Add(new ValidationFailure() { ErrorMessage = $"{ex.Message} | {updateRegister.ToJsonString()}" });
                    throw;
                }
            }
            catch (Exception ex)
            {
                resultValidation.Errors.Add(new ValidationFailure() { ErrorMessage = $"{ex.Message} | {updateRegister.ToJsonString()}" });
                throw;
            }

            _cacheRepository.RemoveKeysInMemoryCacheByPartKey("count");

            return resultValidation;
        }

        #region Delete Methods

        public RegisterValidationResult RemoveRegister(RegisterDeleteRequest registerDeleteRequest, UserJwt userJwt, DateTime saveDate)
        {
            string registerName = EnumHelper.GetEnumTitleCase<EInformationType>((EInformationType)registerDeleteRequest.InformationType);

            RegisterDeleteValidation validation = new RegisterDeleteValidation();
            ValidationResult result = validation.Validate(registerDeleteRequest);
            RegisterValidationResult resultValidation = _mapper.Map<RegisterValidationResult>(result);

            if (!resultValidation.IsValid)
                return resultValidation;

            (RegisterValidationResult resultValidationDependency, List<Register> foundRegisters) = ValidadateRegisterToDelete(registerDeleteRequest, resultValidation, userJwt);

            if (!resultValidationDependency.IsValid && foundRegisters.Count == 0)
                return resultValidationDependency;

            if (userJwt.StartedInventory)
            {
                foundRegisters.ForEach(register =>
                {
                    register.Ativo = false;
                    register.InformationType = registerDeleteRequest.InformationType;
                });

                SaveListResult saveListResult = SaveRegisterList(foundRegisters, userJwt, saveDate);
                resultValidationDependency.Message = $"Foram desativados {saveListResult.QuantityUpdated} registros de {registerName}";
            }
            else
            {
                try
                {
                    if (foundRegisters.Any())
                    {
                        _unitOfWork.BeginTransaction();

                        foundRegisters.ForEach(register =>
                        {
                            register.Ativo = false;
                            register.InformationType = registerDeleteRequest.InformationType;
                            _unitOfWork.RegisterRepository.Delete(register);
                        });
                    }

                    resultValidation.Message = $"Foram excluídos {foundRegisters.Count} registros de {registerName}.";

                    _unitOfWork.Commit();

                }
                catch (Exception)
                {
                    _unitOfWork.Rollback();
                    throw;
                }
            }

            _cacheRepository.RemoveKeysInMemoryCacheByPartKey($"count{registerName}");

            return resultValidation;
        }

        private (RegisterValidationResult, List<Register>) ValidadateRegisterToDelete(RegisterDeleteRequest registerDeleteRequest,RegisterValidationResult resultValidation,UserJwt userJwt)
        { 
            if (registerDeleteRequest.DeleteAll)            
                return ValidateDeleteAll(registerDeleteRequest, resultValidation, userJwt); 
            else 
                return ValidateDeleteList(registerDeleteRequest, resultValidation, userJwt);
        }

        private (RegisterValidationResult, List<Register>) ValidateDeleteList(RegisterDeleteRequest registerDeleteRequest,RegisterValidationResult resultValidation,UserJwt userJwt)
        {
            string registerName = EnumHelper.GetEnumTitleCase<EInformationType>((EInformationType)registerDeleteRequest.InformationType);

            List<Register> listReturn = new List<Register>();

            var filter = new RegisterPaginateRequest()
            {
                InformationType = (EInformationType)registerDeleteRequest.InformationType,
                BaseInventarioId = (int)registerDeleteRequest.BaseInventoryId
            };

            foreach (string code in registerDeleteRequest.RegisterCodeList.Where(x => !string.IsNullOrEmpty(x)).ToList())
            {
                filter.Codigo = code;
                List<Register> listFound = _registerRepository.GetList(filter);

                if (!listFound.Any())
                    resultValidation.Errors.Add(new ValidationFailure() { ErrorMessage = $"{registerName} código: {code} não encontrado." });

                listFound.ForEach(register =>
                {
                    int countInventory = GetInventoryDependencesCount(register, (EInformationType)registerDeleteRequest.InformationType, (int)registerDeleteRequest.BaseInventoryId, userJwt.UserId);
                    if (countInventory > 0)
                        resultValidation.Errors.Add(new ValidationFailure() { ErrorMessage = $"Há {countInventory} itens de inventário que dependem de {registerName} código: '{register.Codigo}' " });
                    else
                        listReturn.Add(register);
                });
            }

            return (resultValidation, listReturn);
        }

        private (RegisterValidationResult, List<Register>) ValidateDeleteAll(RegisterDeleteRequest registerDeleteRequest, RegisterValidationResult resultValidation, UserJwt userJwt)
        {
            string registerName = EnumHelper.GetEnumTitleCase<EInformationType>((EInformationType)registerDeleteRequest.InformationType);
            List<Register> listReturn = new List<Register>();

            var filter = new RegisterPaginateRequest()
            {
                InformationType = (EInformationType)registerDeleteRequest.InformationType,
                BaseInventarioId = (int)registerDeleteRequest.BaseInventoryId
            };

            List<Register> listFound = _registerRepository.GetList(filter);

            listFound.ForEach(register =>
            {
                int countInventory = GetInventoryDependencesCount(register, (EInformationType)registerDeleteRequest.InformationType, (int)registerDeleteRequest.BaseInventoryId, userJwt.UserId);
                if (countInventory > 0)
                    resultValidation.Errors.Add(new ValidationFailure() { ErrorMessage = $"Existe(m) {countInventory} dependendência(s) de {registerName} '{register.Codigo}' " });
                else
                    listReturn.Add(register);
            });
            return (resultValidation, listReturn);
        }

        private int GetInventoryDependencesCount(Register founRegister, EInformationType tipoRegistroId, int baseInventarioId, int userId)
        {
            string registerName = EnumHelper.GetEnumTitleCase<EInformationType>(tipoRegistroId);

            InventoryPaginateRequest filterObject = new();

            switch (tipoRegistroId)
            {
                case EInformationType.filial:
                    filterObject.FilialId = founRegister.Id;
                    break;

                case EInformationType.local:
                    filterObject.LocalId = founRegister.Id;
                    break;

                case EInformationType.responsavel:
                    filterObject.ResponsavelId = founRegister.Id;
                    break;

                case EInformationType.centrocusto:
                    filterObject.CentroCustoId = founRegister.Id;
                    break;

                case EInformationType.contacontabil:
                    filterObject.ContaContabilId = founRegister.Id;
                    break;
            }

            filterObject.BaseInventarioId = baseInventarioId;

            string sqlFilter = string.Empty;
            DynamicParameters dbArgs;
            FiltersExtentions<InventoryPaginateRequest>.FilterBuilder(filterObject, CommonLists.BlackListFilterBuilderRegister.ToList(), out dbArgs, out sqlFilter);
             
            var count = _countRepository.GetCountItensAsync("inventario", sqlFilter, dbArgs, filterObject.ToJsonString(), userId).Result;

            return count;
        }

        #endregion

        #region Save List Methods

        public SaveListResult SaveRegisterList(List<Register> listRegisters, UserJwt userJwt, DateTime saveDate)
        {
            try
            {
                ProcessSave<Register> processSave = PrepareProcessingList(listRegisters);

                _unitOfWork.BeginTransaction();

                processSave = UpdateList(listRegisters, processSave, userJwt, saveDate);

                processSave = InsertList(processSave, userJwt, saveDate);

                _unitOfWork.Commit();

                return processSave.SaveListRS;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        private ProcessSave<Register> PrepareProcessingList(List<Register> listRegisters)
        {
            ProcessSave<Register> processSave = new();
            foreach (Register register in listRegisters)
            {
                RegisterValidation validation = new();
                var result = validation.Validate(register);
                RegisterValidationResult registerValidationResult = _mapper.Map<RegisterValidationResult>(result);

                registerValidationResult.RegisterObject = register;
                if (registerValidationResult.IsValid)
                {
                    var registerList = _registerRepository.GetList(new RegisterPaginateRequest()
                    {
                        Ativo = null,
                        InformationType = register.InformationType,
                        Codigo = register.Codigo,
                        BaseInventarioId = register.BaseInventarioId
                    });

                    if (registerList.Any())
                        processSave.ListFoundItens.Add(registerList.FirstOrDefault());
                    else
                        processSave.ListNewItens.Add(register);
                }
                else
                {
                    string messages = String.Join(", ", registerValidationResult.Errors.Select(x => x.ErrorMessage).ToList());

                    processSave.SaveListRS.DiscartedItens.Add(new { DiscartedItemRQ = register, messages });

                    processSave.SaveListRS.QuantityDiscarded++;
                }
            }
            return processSave;
        }

        private ProcessSave<Register> UpdateList(List<Register> listRegisters, ProcessSave<Register> processSave, UserJwt userJwt, DateTime saveDate)
        {
            foreach (Register updateRegister in processSave.ListFoundItens)
            {
                ValidationResult result = new();

                var newValues = listRegisters.Find(x => x.Codigo == updateRegister.Codigo);
                var resultUpdate = UpdateRegister(updateRegister, newValues, userJwt, saveDate);
                result.Errors.AddRange(resultUpdate.Errors);

                if (result.IsValid)
                    processSave.SaveListRS.QuantityUpdated++;
            }
            return processSave;
        }

        private ProcessSave<Register> InsertList(ProcessSave<Register> processSave, UserJwt userJwt, DateTime saveDate)
        {
            foreach (Register register in processSave.ListNewItens)
            {
                ValidationResult result = new();

                var resultInsert = InsertRegister(register, userJwt, saveDate);
                result.Errors.AddRange(resultInsert.Errors);

                if (result.IsValid)
                    processSave.SaveListRS.QuantityInserted++;
            }
            return processSave;
        }

        #endregion

        #region Paginate Methods

        public async Task<ContainerResult<Register>> GetPagintateAsync(RegisterPaginateRequest registerRequest, int userId)
        {
            string sqlFilter = string.Empty;
            DynamicParameters dbArgs;
            FiltersExtentions<RegisterPaginateRequest>.FilterBuilder(registerRequest, CommonLists.BlackListFilterBuilderRegister.ToList(), out dbArgs, out sqlFilter);

            List<Register> listItens = await _registerRepository.GetPagintateAsync(registerRequest, sqlFilter, dbArgs);

            int countRegisters = await GetCountAsync(registerRequest, sqlFilter, dbArgs, userId);

            return new ContainerResult<Register>()
            {
                Count = countRegisters,
                ListItens = listItens
            };
        }

        private async Task<int> GetCountAsync(RegisterPaginateRequest registerRequest, string sqlFilter, DynamicParameters dbArgs, int userId)
        { 
            string entity = EnumHelper.GetEnumTitleCase<EInformationType>((EInformationType)registerRequest.InformationType);
              
            var registerRequestClone = registerRequest.Clone<RegisterPaginateRequest>();
            registerRequestClone.Page = 0;
            registerRequestClone.PageSize = 0;

            return await _countRepository.GetCountItensAsync(entity, sqlFilter, dbArgs, registerRequestClone.ToJsonString(), userId);
        }

        #endregion
    }
}
