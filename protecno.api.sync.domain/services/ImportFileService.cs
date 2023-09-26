using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using protecno.api.sync.domain.entities;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.extensions;
using protecno.api.sync.domain.helpers;
using protecno.api.sync.domain.interfaces;
using protecno.api.sync.domain.interfaces.repositories;
using protecno.api.sync.domain.mapers;
using protecno.api.sync.domain.models.generics;
using protecno.api.sync.domain.models.import;
using protecno.api.sync.domain.models.report;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.services
{
    public class ImportFileService : IImportFileService
    {
        private readonly IImportCashService _importCashService;
        private readonly ICacheRepository _cacheRepository;
        private readonly IRegisterService _registerService;
        private readonly ICsvHelperService _csvHelperService;

        public ImportFileService(IImportCashService importCashService, ICacheRepository cacheRepository, IRegisterService registerService, ICsvHelperService csvHelperService)
        {
            _importCashService = importCashService;
            _cacheRepository = cacheRepository;
            _registerService = registerService;
            _csvHelperService = csvHelperService;
        }

        public ImportFileResult ImportCsvFileAsync(IFormFile file, ImportFileRequest importFileRequest, ImportFileCache importFileCache, UserJwt userJwt, ImportFileResult requestReportResult, string keyImportRequest)
        { 
            requestReportResult.StatusCode = HttpStatusCode.Processing;
            _importCashService.SaveCache(keyImportRequest, importFileCache, HttpStatusCode.Processing);

            string informationType = EnumHelper.GetEnumTitleCase<EInformationType>((EInformationType)importFileRequest.InformationType);
                      
            Task.Run(() => { ProcessImport(keyImportRequest, informationType, importFileCache, userJwt); });

            return requestReportResult;
        }

        public void ClearCache(string keyImportRequest)
        {
            _cacheRepository.RemoveKeysInMemoryCacheByPartKey(keyImportRequest);
        }

        public async Task<ImportFileCache> SaveFile(IFormFile file)
        {
            var importFileCache = new ImportFileCache();

            string uploadFilePath = Path.Combine(AppContext.BaseDirectory, "Uploads");

            if (!System.IO.Directory.Exists(uploadFilePath))
                System.IO.Directory.CreateDirectory(uploadFilePath);

            importFileCache.ImportFileFullPath = Path.Combine(uploadFilePath, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

            using (var stream = new FileStream(importFileCache.ImportFileFullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return importFileCache;
        }

        private void ProcessImport(string keyImportRequest,string informationType, ImportFileCache importFileCache, UserJwt userJwt)
        {
            DateTime importDate = DateTime.Now;

            importFileCache.ImportReport = new ReportCache()
            {
                InformationType = informationType,
                FilePath = Path.Combine(AppContext.BaseDirectory, "Reports"),
                InternalFileName = $"{keyImportRequest}.csv",
                FullFilePath = Path.Combine(AppContext.BaseDirectory, "Reports", $"{keyImportRequest}.csv"),
                PublicFileName = $"report-{informationType}-{DateTime.Now.ToString("dd.MM.yyy hh.mm")}.csv"
            };

            string aa = importFileCache.ToJsonString();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                Delimiter = ";"
            };

            using (var reader = new StreamReader(importFileCache.ImportFileFullPath))
            using (var csv = new CsvReader(reader, config))
            {
                int count=0;
                csv.Context.RegisterClassMap<CsvMapRegister>();

                var batchSize = 100; 
                var records = new List<Register>();

                while (csv.Read())
                {
                    var item = csv.GetRecord<Register>();              
                    item.OrigemId = (int)EOrigin.CSV;
                    item.InformationType = EInformationType.filial;
                    item.Id = null;

                    records.Add(item);
                    count++;

                    if (records.Count >= batchSize)                     
                        SaveList(records, importFileCache, userJwt, importDate);
                }

                if(records.Any())
                    SaveList(records, importFileCache, userJwt, importDate);

                _importCashService.SaveCache(keyImportRequest, importFileCache, HttpStatusCode.OK);
            } 
        }

        private void SaveList(List<Register> records,ImportFileCache importFileCache, UserJwt userJwt, DateTime importDate)
        {
            SaveListResult saveListResult = _registerService.SaveRegisterList(  records, userJwt, importDate);

            if (saveListResult.DiscartedItens.Any())
                _csvHelperService.WriteRecordsAsync(importFileCache.ImportReport.FullFilePath, saveListResult.DiscartedItens, true, EReportType.FilialCSV);

            records.Clear();
        }
    }
}

