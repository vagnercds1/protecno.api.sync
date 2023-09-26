using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using protecno.api.sync.domain.common;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.helpers;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.models.report;
using protecno.api.sync.domain.services;
using protecno.api.sync.domain.validations;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace protecno.api.sync.application.controllers
{
    [Route("api/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        //Remove this when security be implemented
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

        private readonly IRepository<Inventory, InventoryPaginateRequest> _inventoryRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IMapper _mapper;
        private readonly ICsvHelperService _csvHelperService;
        private readonly IReportGeneratorService _reportGeneratorService;
        private readonly ILogService _logService;

        public ReportController(
            IRepository<Inventory, InventoryPaginateRequest> inventoryRepository,
            IInventoryService inventoryService,
            IMapper mapper,
            ICsvHelperService csvHelperService,
            IConfiguration configuration,
            IReportGeneratorService reportGeneratorService, ILogService logService)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
            _inventoryService = inventoryService;
            _csvHelperService = csvHelperService;
            _reportGeneratorService = reportGeneratorService;
            _logService = logService;
        } 

        [HttpGet("request")]
        public async Task<IActionResult> RequestReportAsync([FromQuery] ReportRequest request)
        {
            ReportResult requestReportResult = new();
            try
            { 
                ReportValidation validation = new ReportValidation();
                ValidationResult resultValidation = validation.Validate(request);

                if (!resultValidation.IsValid)
                    return resultValidation.Errors.Select(x => x.ErrorMessage).CreateRestResponse(HttpStatusCode.BadRequest);

                requestReportResult = await Task.Run(() => _reportGeneratorService.RequestReport(request, request.ReportType, userJwt));

                if (requestReportResult.StatusCode == HttpStatusCode.InternalServerError)
                    return Constants.Messages.REPORT_GENERATE_FAIL.CreateRestResponse((HttpStatusCode.InternalServerError));

                if (requestReportResult.StatusCode == HttpStatusCode.NoContent)
                    return Constants.Messages.REPORT_GENERATE_NO_CONTENT.CreateRestResponse((HttpStatusCode.OK));

                if (requestReportResult.StatusCode == HttpStatusCode.Processing)
                    return "Processando".CreateRestResponse();

                return File(new FileStreamDeleteHelper(Path.Combine(requestReportResult.CacheReport.FilePath, requestReportResult.CacheReport.InternalFileName),
                                                       FileMode.Open),
                            Text.Plain,
                            requestReportResult.CacheReport.PublicFileName);
            }
            catch (Exception ex)
            {
                _logService.WhriteErro(ex, Constants.Messages.REPORT_GENERATE_FAIL, userJwt);
                return Constants.Messages.REPORT_GENERATE_FAIL.CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }
    }
}
