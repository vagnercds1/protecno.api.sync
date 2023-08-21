using AutoMapper;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.helpers;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.inventory;
using protecno.api.sync.domain.models.register;
using protecno.api.sync.domain.models.report;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.services
{
    public interface IReportGeneratorService
    {
        ReportResult RequestReport(ReportRequest reportRequest, EReportType reportType, UserJwt userJwt);
    }

    public class ReportGeneratorService : IReportGeneratorService
    {
        private readonly IReportCashService _reportCashService;
        private readonly IInventoryService _inventoryService;
        private readonly IRegisterService _registerService;
        private readonly ICsvHelperService _csvHelperService;
        private readonly ICacheRepository _cacheRepository;
        private readonly IMapper _mapper;
        private readonly ILogService _logService;

        public ReportGeneratorService(
            IInventoryService inventoryService,
            IRegisterService registerService,
            ICsvHelperService csvHelperService,
            IReportCashService reportCashService,
            ICacheRepository cacheRepository,
            IMapper mapper,
            ILogService logService)
        {
            _reportCashService = reportCashService;
            _inventoryService = inventoryService;
            _registerService = registerService;
            _csvHelperService = csvHelperService;
            _cacheRepository = cacheRepository;
            _mapper = mapper;
            _logService = logService;
        }

        public ReportResult RequestReport(
            ReportRequest reportRequest,
            EReportType reportType,
            UserJwt userJwt)
        {
            string keyReportRequest = $"{reportType}-{userJwt.UserId}-{HashHelper.CreateHash<string>(reportRequest.ToJsonString())}";

            ReportResult requestReportResult = _reportCashService.CheckResquestReport(keyReportRequest);

            if (requestReportResult.StatusCode == HttpStatusCode.Processing || requestReportResult.StatusCode == HttpStatusCode.InternalServerError || requestReportResult.StatusCode == HttpStatusCode.NoContent)
                return requestReportResult;
            else if (requestReportResult.StatusCode == HttpStatusCode.OK)
            {
                _cacheRepository.RemoveKeysInMemoryCacheByPartKey(keyReportRequest);
                return requestReportResult;
            }

            Task.Run(() => { BuildReport(reportRequest, reportType, keyReportRequest, userJwt); });

            requestReportResult.StatusCode = HttpStatusCode.Processing;
            return requestReportResult;
        }

        private void BuildReport(ReportRequest reportRequest,
            EReportType reportType,
            string keyReportRequest,
            UserJwt userJwt)
        {
            try
            {
                var requestReportResult = _reportCashService.SaveCache(keyReportRequest,
                    reportType.ToString(),
                    reportType.GetGroupName(),
                    HttpStatusCode.Processing);

                if (!System.IO.Directory.Exists(requestReportResult.CacheReport.FilePath))
                    System.IO.Directory.CreateDirectory(requestReportResult.CacheReport.FilePath);

                switch (reportType)
                {
                    case EReportType.InventarioFisicoCSV:
                    case EReportType.InventarioFisicoPDF:
                    case EReportType.InventarioFotosPDF:
                    case EReportType.TermoDeResponsabilidadePDF:
                        BuildInventoriesReports(_mapper.Map<InventoryPaginateRequest>(reportRequest), reportType, userJwt.UserId, requestReportResult);
                        break;

                    case EReportType.FilialCSV:
                    case EReportType.FilialPDF:
                    case EReportType.LocalCSV:
                    case EReportType.LocalPDF:
                    case EReportType.CentroCustoCSV:
                        BuildRegisterReports(_mapper.Map<RegisterPaginateRequest>(reportRequest), reportType, userJwt.UserId, requestReportResult);
                        break;
                }

                if (System.IO.File.Exists(requestReportResult.CacheReport.FullPath))
                {
                    _reportCashService.SaveCache(keyReportRequest,
                    reportType.ToString(),
                    reportType.GetGroupName(),
                    HttpStatusCode.OK);
                }
                else
                {
                    _reportCashService.SaveCache(keyReportRequest,
                    reportType.ToString(),
                    reportType.GetGroupName(),
                    HttpStatusCode.NoContent);
                }
            }
            catch (Exception ex)
            {
                _reportCashService.SaveCache(keyReportRequest,
                  reportType.ToString(),
                  reportType.GetGroupName(),
                  HttpStatusCode.InternalServerError);

                _logService.WhriteErro(ex, "Houve um erro ao gerar o relatório solicitado", userJwt);
            }
        }

        private void BuildInventoriesReports(InventoryPaginateRequest inventoreRQ, EReportType reportType, int userId, ReportResult requestReportResult)
        {
            inventoreRQ.Page = 0;
            inventoreRQ.PageSize = 10;
            int exported = 0;
            bool printHeader = true;
            var itemRS = _inventoryService.GetPagintateAsync(inventoreRQ, userId).Result;

            while (itemRS.ListItens.Any())
            {
                switch (reportType)
                {
                    case EReportType.InventarioFisicoCSV:
                        _csvHelperService.WriteRecordsAsync(requestReportResult.CacheReport.FullPath, itemRS.ListItens.ToList<object>(), printHeader, reportType);
                        break;

                    case EReportType.InventarioFisicoPDF:
                        break;

                    case EReportType.InventarioFotosPDF:
                        break;

                    case EReportType.TermoDeResponsabilidadePDF:
                        break;
                }

                printHeader = false;
                exported += itemRS.ListItens.Count;
                inventoreRQ.Page++;

                itemRS = _inventoryService.GetPagintateAsync(inventoreRQ, userId).Result;
            }
        }

        private void BuildRegisterReports(RegisterPaginateRequest registerPaginateRequestRQ, EReportType reportType, int userId, ReportResult requestReportResult)
        {
            switch (reportType)
            {
                case EReportType.FilialCSV:
                case EReportType.FilialPDF:
                    registerPaginateRequestRQ.TipoRegistroId = ERegisterType.filial;
                    break;

                case EReportType.LocalCSV:
                case EReportType.LocalPDF:
                    registerPaginateRequestRQ.TipoRegistroId = ERegisterType.local;
                    break;

                case EReportType.CentroCustoCSV:
                case EReportType.CentroCustoPDF:
                    registerPaginateRequestRQ.TipoRegistroId = ERegisterType.centrocusto;
                    break;

                case EReportType.ResponsavelCSV:
                case EReportType.ResponsavelPDF:
                    registerPaginateRequestRQ.TipoRegistroId = ERegisterType.responsavel;
                    break;

                case EReportType.ContaContabilCSV:
                case EReportType.ContaContabilPDF:
                    registerPaginateRequestRQ.TipoRegistroId = ERegisterType.contacontabil;
                    break;
            }

            registerPaginateRequestRQ.Page = 0;
            registerPaginateRequestRQ.PageSize = 100;
            int exported = 0;
            bool printHeader = true;
            var itemRS = _registerService.GetPagintateAsync(registerPaginateRequestRQ, userId).Result;

            while (itemRS.ListItens.Any())
            {
                if (reportType.GetGroupName().ToString() == ".csv")
                    _csvHelperService.WriteRecordsAsync(requestReportResult.CacheReport.FullPath, itemRS.ListItens.ToList<object>(), printHeader, reportType);

                printHeader = false;
                exported += itemRS.ListItens.Count;
                registerPaginateRequestRQ.Page++;

                itemRS = _registerService.GetPagintateAsync(registerPaginateRequestRQ, userId).Result;
            }
        }
    }
}


