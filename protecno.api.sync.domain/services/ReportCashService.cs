using protecno.api.sync.domain.common;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.report;
using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace protecno.api.sync.domain.services
{
    public interface IReportCashService
    {
        ReportResult SaveCache(string keyReportRequest, string reportType, string extension, HttpStatusCode statusCode);

        ReportResult CheckResquestReport(string requetKey);
    }

    public class ReportCashService : IReportCashService
    {
        private readonly ICacheRepository _cacheRepository;

        public ReportCashService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }

        public ReportResult SaveCache(string keyReportRequest, string reportType, string extension, HttpStatusCode statusCode)
        {
            ReportResult requestReportResult = new()
            {
                StatusCode = statusCode,
                CacheReport = new ReportCache()
                { 
                    InformationType = reportType,
                    FilePath = Path.Combine(AppContext.BaseDirectory, "Reports"),
                    InternalFileName = $"{keyReportRequest}{extension}",
                    FullFilePath = Path.Combine(AppContext.BaseDirectory, "Reports", $"{keyReportRequest}{extension}"),
                    PublicFileName = $"{reportType}-{DateTime.Now.ToString("dd.MM.yyy hh.mm")}{extension}"
                }
            };

            _cacheRepository.SetKeyInMemory(keyReportRequest, requestReportResult.ToJsonString(), Constants.REPORT_CASH_MINUTES_TTL);

            return requestReportResult;
        }

        public ReportResult CheckResquestReport(string requetKey)
        {
            string result = _cacheRepository.GetKeyInMemory(requetKey);

            if (string.IsNullOrEmpty(result))
                return new ReportResult() { StatusCode = HttpStatusCode.Continue };

            return JsonSerializer.Deserialize<ReportResult>(result);
        }
    }
}
