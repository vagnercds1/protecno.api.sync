using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.helpers;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.import;
using protecno.api.sync.domain.models.report;
using protecno.api.sync.domain.services;
using protecno.api.sync.domain.validations;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace protecno.api.sync.application.controllers
{
    [Route("api/file")]
    [ApiController]
    public class FileController : Controller
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
            StartedInventory = true
        };

        private readonly ILogService _logService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImportFileService _importFileService;
        private readonly IImportCashService _importCashService;

        public FileController(
            ILogService logService,
            IUnitOfWork unitOfWork,
            IImportFileService importFileService,
             IImportCashService importCashService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
            _importFileService = importFileService;
            _importCashService = importCashService;
        }

        [HttpPost("import-csv")]
        public async Task<IActionResult> ImportCsvFileAsync([FromForm] IFormFile file, [FromForm] ImportFileRequest importFileRequest)
        {
            ImportFileResult importFileResult = new();
            try
            {
                ImportFileValidation validation = new ImportFileValidation(file);
                ValidationResult resultValidation = validation.Validate(importFileRequest);

                if (!resultValidation.IsValid)
                    return resultValidation.Errors.Select(x => x.ErrorMessage).CreateRestResponse(HttpStatusCode.BadRequest);

                string keyImportRequest = $"{importFileRequest.InformationType}-{userJwt.UserId}-{HashHelper.CreateHash<string>(importFileRequest.ToJsonString())}";
                importFileResult = _importCashService.CheckResquestImport(keyImportRequest);


                if (importFileResult.StatusCode == HttpStatusCode.Continue)
                {
                    ImportFileCache importFileCache = await _importFileService.SaveFile(file);

                    importFileResult = await Task.Run(() => _importFileService.ImportCsvFileAsync(file, importFileRequest, importFileCache, userJwt, importFileResult, keyImportRequest));

                    return "Processando".CreateRestResponse();
                }

                if (importFileResult.StatusCode == HttpStatusCode.InternalServerError)
                    return "Houve um erro ao importar o arquivo".CreateRestResponse((HttpStatusCode.InternalServerError));

                if (importFileResult.StatusCode == HttpStatusCode.NoContent)
                    return "O arquivo enviado está vazio".CreateRestResponse((HttpStatusCode.OK));

                if (importFileResult.StatusCode == HttpStatusCode.Processing)
                    return "Processando".CreateRestResponse();

                _importFileService.ClearCache(keyImportRequest);

                if (System.IO.File.Exists(importFileResult.CacheImport.ImportReport.FullFilePath))
                {
                    return File(new FileStreamDeleteHelper(importFileResult.CacheImport.ImportReport.FullFilePath, FileMode.Open),
                                Text.Plain,
                               importFileResult.CacheImport.ImportReport.PublicFileName);
                }
                else
                {
                    return "Importação finalizada com sucesso ".CreateRestResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                _logService.WhriteErro(ex, "Houve um erro ao importar o arquivo", userJwt);
                return "Houve um erro ao importar o arquivo".CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("import-file")]
        public async Task<IActionResult> ImportFileAsync([FromForm] IFormFile file, [FromForm] ImportFileRequest importFileRequest)
        {
            ImportFileResult importFileResult = new();
            try
            {
                ImportFileValidation validation = new ImportFileValidation(file);
                ValidationResult resultValidation = validation.Validate(importFileRequest);

                if (!resultValidation.IsValid)
                    return resultValidation.Errors.Select(x => x.ErrorMessage).CreateRestResponse(HttpStatusCode.BadRequest);

                string keyImportRequest = $"{importFileRequest.InformationType}-{userJwt.UserId}-{HashHelper.CreateHash<string>(importFileRequest.ToJsonString())}";
                importFileResult = _importCashService.CheckResquestImport(keyImportRequest);


                if (importFileResult.StatusCode == HttpStatusCode.Continue)                
                   await _importFileService.SaveFile(file);

                return "Arquivo recebido".CreateRestResponse(HttpStatusCode.OK);
               
            }
            catch (Exception ex)
            {
                _logService.WhriteErro(ex, "Houve um erro ao importar o arquivo", userJwt);
                return "Houve um erro ao importar o arquivo".CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("download-file")]
        public IActionResult DownLoadFileAsync([FromForm] ImportFileRequest importFileRequest)
        {
            ImportFileResult importFileResult = new();
            try
            {
                if (System.IO.File.Exists(importFileResult.CacheImport.ImportReport.FullFilePath))
                {
                    return File(new FileStreamDeleteHelper(importFileResult.CacheImport.ImportReport.FullFilePath, FileMode.Open),
                                Text.Plain,
                               importFileResult.CacheImport.ImportReport.PublicFileName);
                }
                else                
                    return "Arquivo não encontrado".CreateRestResponse(HttpStatusCode.NotFound);
                 
            }
            catch (Exception ex)
            {
                _logService.WhriteErro(ex, "Houve um erro ao baixar arquivo", userJwt);
                return "Houve um erro ao baixar arquivo".CreateRestResponse((HttpStatusCode.InternalServerError));
            }
        }
    }
}
