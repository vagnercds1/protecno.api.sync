using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.register;
using protecno.api.sync.domain.services;
using protecno.api.sync.domain.validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace protecno.api.sync.application.controllers
{
    [Route("api/register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        // Remove this when security be implemented
        private readonly UserJwt userJwt = new UserJwt()
        {
            Email = "teste@teste.com",
            UserId = 1,
            TagCustomer = "Inventario Teste",
            ExpiresPreSignedURL = 1,
            BaseInventoryId = 1,
            CustomerId = 1,
            StartedInventory = false
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Register, RegisterPaginateRequest> _registerRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly IRegisterService _registerService;
        private readonly IMapper _mapper;
        private readonly ILogService _logService;

        public RegisterController(IRepository<Register, RegisterPaginateRequest> registerRepo,
            IHistoryRepository historyRepo,
            IRegisterService registerService,
            IMapper mapper,
            ILogService logService,
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _registerRepository = registerRepo;
            _historyRepository = historyRepo;
            _registerService = registerService;
            _mapper = mapper;
            _logService = logService;
        }

        [HttpGet("paginate")]
        public async Task<IActionResult> GetRegistersPagintateAsync([FromQuery] RegisterPaginateRequest registerRequest)
        {
            try
            {
                int pag = (int)registerRequest.Page;
                RegisterValidationGet validation = new RegisterValidationGet();
                ValidationResult resultValidation = validation.Validate(registerRequest);

                if (!resultValidation.IsValid)
                    return resultValidation.Errors.Select(x => x.ErrorMessage).CreateRestResponse(HttpStatusCode.BadRequest);

                ContainerResult<Register> registerRS = await _registerService.GetPagintateAsync(registerRequest, userJwt.UserId);

                return registerRS.ListItens.CreateRestResponse(pagination: new Pagination
                {
                    Page = pag,
                    PageSize = (int)registerRequest.PageSize,
                    Total = registerRS.Count
                });
            }
            catch (Exception ex)
            {
                string message = $"Erro ao executar a consulta de {Enum.GetName(typeof(EInformationType), registerRequest.InformationType)}";
                _logService.WhriteErro(ex, message, userJwt);

                return message.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetRegisterByIdAsync([FromQuery] RegisterByIdRequest registerByIdRQ)
        {
            try
            {
                RegisterValidationGetById validation = new RegisterValidationGetById();
                ValidationResult resultValidation = validation.Validate(registerByIdRQ);

                if (!resultValidation.IsValid)
                    return resultValidation.Errors.Select(x => x.ErrorMessage).CreateRestResponse(HttpStatusCode.BadRequest);

                var list = await _registerRepository.GetListAsync(new RegisterPaginateRequest()
                {
                    InformationType = registerByIdRQ.InformationType,
                    Id = registerByIdRQ.RegisterId,
                    BaseInventarioId = registerByIdRQ.BaseInventarioId
                });

                Register register = list.FirstOrDefault();

                if (register == null)
                    return "Não localizado".CreateRestResponse(HttpStatusCode.OK);

                RegisterResult registerRS = _mapper.Map<RegisterResult>(register);

                registerRS.HistoryList = await _historyRepository.GetHistoryListAsync(new History() { TipoRegistro = Enum.GetName(typeof(EInformationType), registerByIdRQ.InformationType), RegistroId = (int)register.Id });

                return registerRS.CreateRestResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                string message = $"Erro ao executar a consulta de {Enum.GetName(typeof(EInformationType), registerByIdRQ.InformationType)}";
                _logService.WhriteErro(ex, message, userJwt);

                return message.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost()]
        public async Task<IActionResult> PostRegisterAsync(Register register)
        {
            try
            {
                var result = await Task.Run(() => _registerService.InsertRegister(register, userJwt, DateTime.Now));

                if (!result.IsValid)
                {
                    string messages = String.Join(", ", result.Errors.Select(x => x.ErrorMessage).ToList());
                    return new { DiscartedItemRQ = register, messages }.CreateRestResponse(HttpStatusCode.BadRequest);
                }

                return result.RegisterObject.CreateRestResponse(HttpStatusCode.OK);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    return $"{Enum.GetName(typeof(EInformationType), register.InformationType)} código: '{register.Codigo}' já existe.".CreateRestResponse((HttpStatusCode.BadRequest));
                else
                {
                    string message = $"Erro ao inserir {Enum.GetName(typeof(EInformationType), register.InformationType)}";

                    _logService.WhriteErro(ex, message, userJwt);

                    return message.CreateRestResponse(HttpStatusCode.InternalServerError);
                }
            }
            catch (System.Exception ex)
            {
                string message = $"Erro ao inserir {Enum.GetName(typeof(EInformationType), register.InformationType)}";

                _logService.WhriteErro(ex, message, userJwt);

                return message.CreateRestResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut()]
        public async Task<IActionResult> PutRegisterAsync(Register registerPutRQ)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                if (registerPutRQ.Id == 0)
                    return "Informe o 'Id'".CreateRestResponse(HttpStatusCode.BadRequest);

                var list = await _registerRepository.GetListAsync(new RegisterPaginateRequest()
                {
                    InformationType = registerPutRQ.InformationType,
                    Id = registerPutRQ.Id,
                    BaseInventarioId = registerPutRQ.BaseInventarioId,
                    Ativo = null
                });

                Register originalRegister = list.FirstOrDefault();
                RegisterValidationResult result = await Task.Run(() => _registerService.UpdateRegister(originalRegister, registerPutRQ, userJwt, DateTime.Now));

                if (!result.IsValid)
                {
                    string messages = String.Join(", ", result.Errors.Select(x => x.ErrorMessage).ToList());
                    return new { DiscartedItemRQ = registerPutRQ, messages }.CreateRestResponse(HttpStatusCode.BadRequest);
                }
                else
                {
                    _unitOfWork.Commit();
                    return result.RegisterObject.CreateRestResponse(HttpStatusCode.OK);
                }
            }
            catch (MySqlException ex)
            {
                _unitOfWork.Rollback();

                if (ex.Number == 1062)
                    return $"O código: '{registerPutRQ.Codigo}' já existe.".CreateRestResponse((HttpStatusCode.BadRequest));
                else
                {
                    string message = $"Erro ao atualizar {Enum.GetName(typeof(EInformationType), registerPutRQ.InformationType)}.";
                    _logService.WhriteErro(ex, message, userJwt);
                    return message.CreateRestResponse(HttpStatusCode.InternalServerError);
                }
            }
            catch (System.Exception ex)
            {
                _unitOfWork.Rollback();
                string message = $"Erro ao atualizar {Enum.GetName(typeof(EInformationType), registerPutRQ.InformationType)}";
                _logService.WhriteErro(ex, message, userJwt);
                return message.CreateRestResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("save-list")]
        public async Task<IActionResult> SaveListRegisterAsync(List<Register> listRegister)
        {
            try
            {
                var result = await Task.Run(() => _registerService.SaveRegisterList(listRegister, userJwt, DateTime.Now));

                return result.CreateRestResponse(HttpStatusCode.OK);
            }
            catch (System.Exception ex)
            {
                string message = $"Erro ao salvar lista de {Enum.GetName(typeof(EInformationType), listRegister.First().InformationType)}";
                _logService.WhriteErro(ex, message, userJwt);

                return message.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAsync(RegisterDeleteRequest registerDeleteRequest)
        {
            try
            {
                var result = await Task.Run(() => _registerService.RemoveRegister(registerDeleteRequest, userJwt, DateTime.Now));

                if (!result.IsValid && string.IsNullOrEmpty(result.Message))
                {
                    string messages = String.Join(", ", result.Errors.Select(x => x.ErrorMessage).ToList());
                    return new { DiscartedItemRQ = registerDeleteRequest, messages }.CreateRestResponse(HttpStatusCode.BadRequest);
                }
                else
                {
                    return new
                    {
                        Message = result.Message,
                        ListFails = result.Errors.Select(x => x.ErrorMessage)
                    }.CreateRestResponse(HttpStatusCode.OK);
                }
            }
            catch (System.Exception ex)
            {
                string message = $"Erro ao remover {Enum.GetName(typeof(EInformationType), registerDeleteRequest.InformationType)}";
                _logService.WhriteErro(ex, message, userJwt);
                return message.CreateRestResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
