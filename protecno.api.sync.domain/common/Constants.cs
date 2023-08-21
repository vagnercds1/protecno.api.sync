namespace protecno.api.sync.domain.common
{
    public static class Constants
    {
        public const int REPORT_CASH_MINUTES_TTL = 2;

        public const int TTL_COUNT_MINUTES = 10;

        public const string EXTENSION_REPORT_CSV = ".csv";

        public const string EXTENSION_REPORT_PDF = ".pdf";

        public static class Messages
        {
            public const string REPORT_GENERATE_NO_CONTENT = "Não existem registros que antendam os parametros da busca.";

            public const string REPORT_GENERATE_FAIL = "Não possível gerar o relatório solicitado"; 
        }
    }
}
