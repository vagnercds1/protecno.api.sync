using CsvHelper;
using CsvHelper.Configuration;
using protecno.api.sync.domain.enumerators;
using protecno.api.sync.domain.mapers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace protecno.api.sync.domain.services
{
    public interface ICsvHelperService
    {
        Task WriteRecordsAsync(string filePath, List<dynamic> listObject, bool hasHeaderRecord, EReportType reportType);

        List<dynamic> CsvReader(string filePath);
    }

    public class CsvHelperService : ICsvHelperService
    {
        public List<dynamic> CsvReader(string filePath)
        {
            List<dynamic> records;
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = (List<dynamic>)csv.GetRecords<dynamic>();
            }
            return records;
        }

        public async Task WriteRecordsAsync(string filePath, List<dynamic> listObject, bool hasHeaderRecord, EReportType reportType) 
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture);
            config.Delimiter = ";";  
            config.HasHeaderRecord = hasHeaderRecord;
            
            if (hasHeaderRecord)
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, config))
                {
                    switch (reportType)
                    {
                        case EReportType.InventarioFisicoCSV:
                                csv.Context.RegisterClassMap<CsvMapInventoryFisic>();
                            break;

                        case EReportType.InventarioContabilCSV:
                            csv.Context.RegisterClassMap<CsvMapInventoryAccounting>();
                            break;

                        case EReportType.CentroCustoCSV:
                        case EReportType.FilialCSV:
                        case EReportType.LocalCSV:
                        case EReportType.ResponsavelCSV:
                        case EReportType.ContaContabilCSV:
                            csv.Context.RegisterClassMap<CsvMapRegister>();
                            break;
                    }
                    
                    await csv.WriteRecordsAsync(listObject);
                }
            }
            else
            {
                using (var stream = File.Open(filePath, FileMode.Append))
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, config))
                {
                    switch (reportType)
                    {
                        case EReportType.InventarioFisicoCSV:
                            csv.Context.RegisterClassMap<CsvMapInventoryFisic>();
                            break;

                        case EReportType.InventarioContabilCSV:
                            csv.Context.RegisterClassMap<CsvMapInventoryAccounting>();
                            break;

                        case EReportType.CentroCustoCSV:
                        case EReportType.FilialCSV:
                        case EReportType.LocalCSV:
                        case EReportType.ResponsavelCSV:
                        case EReportType.ContaContabilCSV:
                            csv.Context.RegisterClassMap<CsvMapRegister>();
                            break;
                    }
                    await csv.WriteRecordsAsync(listObject);
                }
            } 
        }
    }
}
