using Microsoft.AspNetCore.Http;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.import;
using protecno.api.sync.domain.models.report;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.interfaces
{
    public interface IImportFileService
    {
        ImportFileResult ImportCsvFileAsync(IFormFile file, ImportFileRequest importFileRequest, ImportFileCache importFileCache, UserJwt userJwt, ImportFileResult requestReportResult, string keyImportRequest);

        Task<ImportFileCache> SaveFile(IFormFile file);

        void ClearCache(string keyImportRequest);
    }
}
