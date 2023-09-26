using protecno.api.sync.domain.common;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.models.report;
using System.Net;
using System.Text.Json;

namespace protecno.api.sync.domain.services
{
    public interface IImportCashService
    {
        ImportFileResult SaveCache(string keyImportRequest, ImportFileCache importFileCache, HttpStatusCode statusCode);

        ImportFileResult CheckResquestImport(string requetKey);
    }

    public class ImportCashService : IImportCashService
    {
        private readonly ICacheRepository _cacheRepository;

        public ImportCashService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }

        public ImportFileResult SaveCache(string keyImportRequest, ImportFileCache importFileCache, HttpStatusCode statusCode)
        { 
            ImportFileResult requestReportResult = new()
            {
                StatusCode = statusCode,
                CacheImport = importFileCache
            };

            _cacheRepository.SetKeyInMemory(keyImportRequest, requestReportResult.ToJsonString(), Constants.REPORT_CASH_MINUTES_TTL);

            return requestReportResult;
        }

        public ImportFileResult CheckResquestImport(string requetKey)
        {
            string result = _cacheRepository.GetKeyInMemory(requetKey);

            if (string.IsNullOrEmpty(result))
                return new ImportFileResult() { StatusCode = HttpStatusCode.Continue };

            return JsonSerializer.Deserialize<ImportFileResult>(result);
        }
    }
}
