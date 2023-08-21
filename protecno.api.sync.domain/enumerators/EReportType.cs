using System.ComponentModel.DataAnnotations;

namespace protecno.api.sync.domain.enumerators
{
    public enum EReportType
    {
        [Display(GroupName = ".csv")]
        InventarioFisicoCSV = 1,

        [Display(GroupName = ".pdf")]
        TermoDeResponsabilidadePDF = 2,

        [Display(GroupName = ".pdf")]
        InventarioFisicoPDF = 3,

        [Display(GroupName = ".pdf")]
        InventarioFotosPDF = 4,

        [Display(GroupName = ".csv")]
        InventarioContabilCSV = 5,

        [Display(GroupName = ".pdf")]
        InventarioContabilPDF = 6,

        [Display(GroupName = ".csv")]
        FilialCSV = 7,

        [Display(GroupName = ".pdf")]
        FilialPDF = 8,

        [Display(GroupName = ".csv")]
        LocalCSV = 9,

        [Display(GroupName = ".pdf")]
        LocalPDF = 10,

        [Display(GroupName = ".csv")]
        CentroCustoCSV = 11,

        [Display(GroupName = ".pdf")]
        CentroCustoPDF = 12,

        [Display(GroupName = ".csv")]
        ResponsavelCSV = 13,

        [Display(GroupName = ".pdf")]
        ResponsavelPDF = 14,

        [Display(GroupName = ".csv")]
        ContaContabilCSV = 15,

        [Display(GroupName = ".pdf")]
        ContaContabilPDF = 16,

        [Display(GroupName = ".csv")]
        ConciliadosCSV = 17,

        [Display(GroupName = ".csv")]
        SobresFisicasCSV = 18,

        [Display(GroupName = ".csv")]
        SobrasContabeisCSV = 19,

        [Display(GroupName = ".pdf")]
        ConciliadosPDF = 20,

        [Display(GroupName = ".pdf")]
        SobresFisicasPDF = 21,

        [Display(GroupName = ".pdf")]
        SobrasContabeisPDF = 22,
    }
}
