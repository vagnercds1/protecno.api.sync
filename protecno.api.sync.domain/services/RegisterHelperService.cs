using FluentValidation.Results;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.models.register;
using System;
using System.Linq;

namespace protecno.api.sync.domain.services
{
    public class RegisterHelperService : IRegisterHelperService
    {
        private readonly IRepository<Register, RegisterPaginateRequest> _registerRepository;
        private readonly IRegisterService _registerService;
        private readonly ICacheRepository _cacheRepository;

        public RegisterHelperService(IRepository<Register, RegisterPaginateRequest> registerRepository, IRegisterService registerService, ICacheRepository cacheRepository)
        {
            _registerRepository = registerRepository;
            _registerService = registerService;
            _cacheRepository = cacheRepository;
        }

        public Inventory FillAllRegisters(Inventory inventoryRQ)
        {
            if (inventoryRQ.FilialId == 0 || inventoryRQ.FilialId == null)
                inventoryRQ.FilialId = string.IsNullOrEmpty(inventoryRQ.FilialCodigo) ? 0 : GetRegisterId(inventoryRQ.BaseInventarioId, EInformationType.filial, inventoryRQ.FilialCodigo);

            if (inventoryRQ.FilialIdAnterior == 0 || inventoryRQ.FilialIdAnterior == null)
                inventoryRQ.FilialIdAnterior = string.IsNullOrEmpty(inventoryRQ.FilialCodigoAnterior) ? 0 : GetRegisterId(inventoryRQ.BaseInventarioId, EInformationType.filial, inventoryRQ.FilialCodigoAnterior);

            if (inventoryRQ.LocalId == 0 || inventoryRQ.LocalId == null) 
                inventoryRQ.LocalId = string.IsNullOrEmpty(inventoryRQ.LocalCodigo) ? 0 : GetRegisterId(inventoryRQ.BaseInventarioId, EInformationType.local, inventoryRQ.LocalCodigo);

            if (inventoryRQ.ResponsavelId == 0 || inventoryRQ.ResponsavelId == null)
                inventoryRQ.ResponsavelId = string.IsNullOrEmpty(inventoryRQ.ResponsavelCodigo) ? 0 : GetRegisterId(inventoryRQ.BaseInventarioId, EInformationType.responsavel, inventoryRQ.ResponsavelCodigo);

            if (inventoryRQ.CentroCustoId == 0 || inventoryRQ.CentroCustoId == null)
                inventoryRQ.CentroCustoId = string.IsNullOrEmpty(inventoryRQ.CentroCustoCodigo) ? 0 : GetRegisterId(inventoryRQ.BaseInventarioId, EInformationType.centrocusto, inventoryRQ.CentroCustoCodigo);

            if (inventoryRQ.ContaContabilId == 0 || inventoryRQ.ContaContabilId == null)
                inventoryRQ.ContaContabilId = string.IsNullOrEmpty(inventoryRQ.ContaContabilCodigo) ? 0 : GetRegisterId(inventoryRQ.BaseInventarioId, EInformationType.contacontabil, inventoryRQ.ContaContabilCodigo);

            return inventoryRQ;
        }

        public int GetRegisterId(int baseInventarioId, EInformationType registerType, string code)
        {
            string registerId = _cacheRepository.GetKeyInMemory($"{(int)registerType}{code}{baseInventarioId}");
            if (!string.IsNullOrEmpty(registerId))
                return Convert.ToInt32(registerId);

            var resultQuery = _registerRepository.GetList(new RegisterPaginateRequest()
            {
                InformationType = registerType,
                Codigo = code,
                BaseInventarioId = baseInventarioId
            });

            Register register = resultQuery.FirstOrDefault();

            if (register == null)
                return 0;

            _cacheRepository.SetKeyInMemory($"{(int)registerType}{code}{baseInventarioId}", register.Id.ToString(), 10);
            return (int)register.Id;
        }

        public InsertResult InsertRegistersWhenNotExists(Inventory inventoryRQ, UserJwt userJwt, DateTime date, bool fillAllRegisters)
        { 
            InsertResult insertResult = new();

            ValidationResult insertResults = new();

            //Need refactor to make as generic form 

            #region FilialAnterior
            if (((inventoryRQ.FilialIdAnterior == 0 || inventoryRQ.FilialIdAnterior == null) || !fillAllRegisters) && (!string.IsNullOrEmpty(inventoryRQ.FilialCodigoAnterior) || !string.IsNullOrEmpty(inventoryRQ.FilialAnterior)))
            {
                RegisterValidationResult result = _registerService.InsertRegister(
                    new Register()
                    {
                        BaseInventarioId = inventoryRQ.BaseInventarioId,
                        Ativo = true,
                        DataCadastro = date,
                        OrigemId = inventoryRQ.OrigemId,
                        InformationType = EInformationType.filial,
                        Codigo = inventoryRQ.FilialCodigoAnterior,
                        Descricao = inventoryRQ.FilialAnterior
                    },
                    userJwt,
                    DateTime.Now);
                if (!result.IsValid)
                    insertResults.Errors.AddRange(result.Errors);
                else
                    inventoryRQ.FilialIdAnterior = result.RegisterObject.Id;
            }
            #endregion

            #region Filial
            if ((inventoryRQ.FilialId == 0 || inventoryRQ.FilialId == null) && (!string.IsNullOrEmpty(inventoryRQ.FilialCodigo) || !string.IsNullOrEmpty(inventoryRQ.Filial)))
            {
                RegisterValidationResult result = _registerService.InsertRegister(
                    new Register()
                    {
                        BaseInventarioId = inventoryRQ.BaseInventarioId,
                        Ativo = true,
                        DataCadastro = date,
                        OrigemId = inventoryRQ.OrigemId,
                        InformationType =EInformationType.filial,
                        Codigo = inventoryRQ.FilialCodigo,
                        Descricao = inventoryRQ.Filial
                    },
                    userJwt,
                    DateTime.Now);
                if (!result.IsValid)
                    insertResults.Errors.AddRange(result.Errors);
                else
                    inventoryRQ.FilialId = result.RegisterObject.Id;
            }
            #endregion

            #region Local
            if ((inventoryRQ.LocalId == 0 || inventoryRQ.LocalId == null) && (!string.IsNullOrEmpty(inventoryRQ.LocalCodigo) || !string.IsNullOrEmpty(inventoryRQ.Local)))
            {
                RegisterValidationResult result = _registerService.InsertRegister(
                    new Register()
                    {
                        BaseInventarioId = inventoryRQ.BaseInventarioId,
                        Ativo = true,
                        DataCadastro = date,
                        OrigemId = inventoryRQ.OrigemId,
                        InformationType = EInformationType.local,
                        Codigo = inventoryRQ.LocalCodigo,
                        Descricao = inventoryRQ.Local
                    },
                    userJwt,
                    DateTime.Now);
                if (!result.IsValid)
                    insertResults.Errors.AddRange(result.Errors);
                else
                    inventoryRQ.LocalId = result.RegisterObject.Id;
            }
            #endregion

            #region Responsavel
            if ((inventoryRQ.ResponsavelId == 0 || inventoryRQ.ResponsavelId == null) && (!string.IsNullOrEmpty(inventoryRQ.ResponsavelCodigo) || !string.IsNullOrEmpty(inventoryRQ.Responsavel)))
            {
                RegisterValidationResult result = _registerService.InsertRegister(
                    new Register()
                    {
                        BaseInventarioId = inventoryRQ.BaseInventarioId,
                        Ativo = true,
                        DataCadastro = date,
                        OrigemId = inventoryRQ.OrigemId,
                        InformationType = EInformationType.responsavel,
                        Codigo = inventoryRQ.ResponsavelCodigo,
                        Descricao = inventoryRQ.Responsavel
                    },
                    userJwt,
                    DateTime.Now);
                if (!result.IsValid)
                    insertResults.Errors.AddRange(result.Errors);
                else
                    inventoryRQ.ResponsavelId = result.RegisterObject.Id;
            }
            #endregion

            #region CentroCusto
            if ((inventoryRQ.CentroCustoId == 0 || inventoryRQ.CentroCustoId == null) && (!string.IsNullOrEmpty(inventoryRQ.CentroCustoCodigo) || !string.IsNullOrEmpty(inventoryRQ.CentroCusto)))
            {
                RegisterValidationResult result = _registerService.InsertRegister(
                    new Register()
                    {
                        BaseInventarioId = inventoryRQ.BaseInventarioId,
                        Ativo = true,
                        DataCadastro = date,
                        OrigemId = inventoryRQ.OrigemId,
                        InformationType = EInformationType.centrocusto,
                        Codigo = inventoryRQ.CentroCustoCodigo,
                        Descricao = inventoryRQ.CentroCusto
                    },
                    userJwt,
                    DateTime.Now);
                if (!result.IsValid)
                    insertResults.Errors.AddRange(result.Errors);
                else
                    inventoryRQ.CentroCustoId = result.RegisterObject.Id;
            }
            #endregion

            #region ContaContabil
            if ((inventoryRQ.ContaContabilId == 0 || inventoryRQ.ContaContabilId == null) && (!string.IsNullOrEmpty(inventoryRQ.ContaContabilCodigo) || !string.IsNullOrEmpty(inventoryRQ.ContaContabil)))
            {
                RegisterValidationResult result = _registerService.InsertRegister(
                    new Register()
                    {
                        BaseInventarioId = inventoryRQ.BaseInventarioId,
                        Ativo = true,
                        DataCadastro = date,
                        OrigemId = inventoryRQ.OrigemId,
                        InformationType = EInformationType.contacontabil,
                        Codigo = inventoryRQ.ContaContabilCodigo,
                        Descricao = inventoryRQ.ContaContabil
                    },
                    userJwt,
                    DateTime.Now);
                if (!result.IsValid)
                    insertResults.Errors.AddRange(result.Errors);
                else
                    inventoryRQ.ContaContabilId = result.RegisterObject.Id;
            }
            #endregion

            insertResult.Result = insertResults;
            insertResult.RequestInventory = inventoryRQ;

            return insertResult;
        }
    }
}
