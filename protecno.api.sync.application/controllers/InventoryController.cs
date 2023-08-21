using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.helpers;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.models.register;
using protecno.api.sync.domain.services;
using protecno.api.sync.domain.validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace protecno.api.sync.application.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IRepository<Inventory, InventoryPaginateRequest> _inventoryRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IHistoryRepository _historyRepository;
        private readonly IMapper _mapper;
        private readonly ICsvHelperService _csvHelperService;
        private readonly IConfiguration _configuration;
        private readonly IReportGeneratorService _reportGeneratorService;
        private readonly ILogService _logService;
        private readonly IUnitOfWork _unitOfWork;

        // retirar isso quando implementar a segurança por JWT
        private readonly UserJwt userJwt = new UserJwt()
        {
            Email = "teste@teste.com",
            UserId = 1,
            TagCustomer = "Inventario Teste",           
            ExpiresPreSignedURL = 1,
            BaseInventoryId = 1,
            CustomerId = 1,
            StartedInventory = true
        };

        public InventoryController(
            IUnitOfWork unitOfWork,
            IRepository<Inventory, InventoryPaginateRequest> inventoryRepository,
            IHistoryRepository historyRepository,
            IInventoryService inventoryService,
            IMapper mapper,
            ICsvHelperService csvHelperService,
            IConfiguration configuration,
            IReportGeneratorService reportGeneratorService, 
            ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _inventoryRepository = inventoryRepository;
            _historyRepository = historyRepository;
            _mapper = mapper;
            _inventoryService = inventoryService;
            _csvHelperService = csvHelperService;
            _configuration = configuration;
            _reportGeneratorService = reportGeneratorService;
            _logService = logService;
        }

        [HttpGet("paginate")]
        public async Task<IActionResult> GetInventoryPagintateAsync([FromQuery] InventoryPaginateRequest register)
        {
            try
            {
                InventoryValidationGet validation = new InventoryValidationGet();
                ValidationResult resultValidation = validation.Validate(register);

                if (!resultValidation.IsValid)
                    return resultValidation.Errors.Select(x => x.ErrorMessage).CreateRestResponse(HttpStatusCode.BadRequest);

                ContainerResult<Inventory> itemRS = await _inventoryService.GetPagintateAsync(register, userJwt.UserId);

                return itemRS.ListItens.CreateRestResponse(pagination: new Pagination
                {
                    Page = (int)register.Page,
                    PageSize = (int)register.PageSize,
                    Total = itemRS.Count
                });
            }
            catch (Exception ex)
            {
                string message = "Erro ao executar a consulta de inventário";
                _logService.WhriteErro(ex, message, userJwt);
 
                return message.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetInventoryByIdAsync([FromQuery] InventoryByIdRequest inventoryByIdRQ)
        {
            try
            {
                InventoryValidationGetById validation = new InventoryValidationGetById();
                ValidationResult resultValidation = validation.Validate(inventoryByIdRQ);

                if (!resultValidation.IsValid)
                    return resultValidation.Errors.Select(x => x.ErrorMessage).CreateRestResponse((HttpStatusCode)resultValidation.Errors.FirstOrDefault().ErrorCode.To<int>());

                var list = await _inventoryRepository.GetListAsync(new InventoryPaginateRequest()
                {
                    Id = inventoryByIdRQ.InventoryId,
                    BaseInventarioId = inventoryByIdRQ.BaseInventarioId
                });

                Inventory Inventory = list.FirstOrDefault();

                if (Inventory == null)
                    return "Não localizado".CreateRestResponse(HttpStatusCode.OK);

                var inventoryRS = _mapper.Map<InventoryResult>(Inventory);

                inventoryRS.Annotations = JsonSerializer.Deserialize<List<Annotation>>(inventoryRS.Anotacoes);

                inventoryRS.HistoryList = await _historyRepository.GetHistoryListAsync(new History()
                {
                    RegistroId = inventoryByIdRQ.InventoryId,
                    TipoRegistro = Enum.GetName(typeof(EInventoryType), Inventory.TipoInventarioId)
                });

                return inventoryRS.CreateRestResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                string message = "Erro ao executar a consulta de inventário";
                _logService.WhriteErro(ex, message, userJwt);

                return message.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost()]
        public async Task<IActionResult> PostInventoryAsync(Inventory inventoryRQ)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var result = await Task.Run(() => _inventoryService.InsertInventory(inventoryRQ, userJwt, DateTime.Now, false,true));

                if (!result.IsValid)
                {
                    string messages = String.Join(", ", result.Errors.Select(x => x.ErrorMessage).ToList());
                    return new { DiscartedItemRQ = inventoryRQ, messages }.CreateRestResponse(HttpStatusCode.BadRequest);
                }

                _unitOfWork.Commit();
                return result.InventoryObject.CreateRestResponse(HttpStatusCode.OK);
            }

            catch (MySqlException ex)
            {
                _unitOfWork.Rollback();

                if (ex.Number == 1062)
                    return $"Item: '{inventoryRQ.FilialId}-{inventoryRQ.Codigo}-{inventoryRQ.Incorporacao}' já existe.".CreateRestResponse((HttpStatusCode.BadRequest));
                else
                {
                    string message = "Erro ao inserir item de inventário";
                    _logService.WhriteErro(ex, message, userJwt);                    
                    return message.CreateRestResponse(HttpStatusCode.InternalServerError);
                }
            }
            catch (System.Exception ex)
            {
                _unitOfWork.Rollback();
                string message = "Erro ao inserir item de inventário";
                _logService.WhriteErro(ex, message, userJwt);                
                return message.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventory(int id, Inventory updateInventoryRQ)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var list = await _inventoryRepository.GetListAsync(new InventoryPaginateRequest()
                {
                    Id = id,
                    BaseInventarioId = updateInventoryRQ.BaseInventarioId,
                    Ativo = null
                });

                Inventory originalInventory = list.FirstOrDefault();

                if (originalInventory == null)
                    return "Item não localizado".CreateRestResponse(HttpStatusCode.BadRequest);

                var result = await Task.Run(() => _inventoryService.UpdateInventory(originalInventory, updateInventoryRQ, userJwt, DateTime.Now, false,true));

                if (!result.IsValid)
                {
                    _unitOfWork.Rollback();
                    string messages = String.Join(", ", result.Errors.Select(x => x.ErrorMessage).ToList());
                    return new { DiscartedItemRQ = updateInventoryRQ, messages }.CreateRestResponse(HttpStatusCode.BadRequest);
                }
                else
                {
                    _unitOfWork.Commit();
                    return result.InventoryObject.CreateRestResponse(HttpStatusCode.OK);
                }
            }
            catch (System.Exception ex)
            {
                _unitOfWork.Rollback();
                string message = "Erro ao atualizar item de inventário";
                _logService.WhriteErro(ex, message, userJwt);

                return message.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("save-list")]
        public async Task<IActionResult> SaveListRegisterAsync(List<Inventory> listInventory)
        {
            try
            {
                var result = await Task.Run(() => _inventoryService.SaveInventoryList(listInventory, userJwt, DateTime.Now));

                return result.CreateRestResponse(HttpStatusCode.OK);
            }
            catch (System.Exception ex)
            {
                string message = "Erro ao gravar lista de itens de inventário";
                _logService.WhriteErro(ex, message, userJwt);

                return message.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(InventoryDeleteRequest inventoryDeleteRequest)
        {
            try
            {   
                var result = await Task.Run(() => _inventoryService.RemoveInventory(inventoryDeleteRequest, userJwt, DateTime.Now));

                if (!result.IsValid)
                {
                    string messages = String.Join(", ", result.Errors.Select(x => x.ErrorMessage).ToList());
                    return messages.CreateRestResponse(HttpStatusCode.BadRequest);
                }
                else
                    return result.Message.CreateRestResponse(HttpStatusCode.OK);
            }
            catch (System.Exception ex)
            {
                string message = $"Erro ao remover itens de inventário!";
                _logService.WhriteErro(ex, message, userJwt);
                return message.CreateRestResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}