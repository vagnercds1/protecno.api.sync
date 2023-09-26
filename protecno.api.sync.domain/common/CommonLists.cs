namespace protecno.api.sync.domain.common
{
    public static class CommonLists
    {
        public static readonly string[] RegisterFildsToCompare = new string[]
        {
            "Codigo",
            "Descricao",
            "Ativo"
        };

        public static readonly string[] InventoryFildsToCompare = new string[]
        {
            "AnoFabricacao",
            "AnosRemanescentes",
            "Ativo",
            "Auxiliar1",
            "Auxiliar2",
            "Auxiliar3",
            "Auxiliar4",
            "Auxiliar5",
            "Auxiliar6",
            "Auxiliar7",
            "Auxiliar8",
            "CentroCustoId",
            "Codigo",
            "CodigoAnterior",
            "CondicaoUso",
            "ContaContabilId",
            "CustoReposicao",
            "DataAquisicao",
            "DataEntradaOperacao",
            "DepreciacaoAcumulada",
            "Descricao",
            "DescricaoComplemento",
            "Documento",
            "FilialId",
            "FilialIdAnterior",
            "Incorporacao",
            "IncorporacaoAnterior",
            "LocalId",
            "Marca",
            "MesesRemanecentes",
            "Modelo",
            "Observacao",
            "PercentualRemanescenteVidaUtil",
            "Propriedades",
            "Quantidade",
            "ResponsavelId",
            "Serie",
            "TAG",
            "TesteRecuperabilidade",
            "ValorDepreciavel",
            "ValorEmUso",
            "ValorOriginal",
            "ValorResidual",
            "VidaUtilAnos",
            "VidaUtilMeses"
        };

        public static readonly string[] BlackListFilterBuilderRegister = new string[]
        {
            "InformationType",
            "Page",
            "PageSize",
            "OrderField",
            "Order"
        };

        public static readonly string[] BlackListFilterBuilderInventory = new string[]
        {
            "InformationType",
            "Page",
            "PageSize",
            "OrderField",
            "Order"
        };
    }
}
