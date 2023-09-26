using System.ComponentModel.DataAnnotations;

namespace protecno.api.sync.domain.enumerators
{
    public enum EImportFileType
    {
        Undefined = 0,

        Filial = 1,

        Local = 2,

        CentroCusto = 3,

        Responsavel = 4,

        ContaContabil = 5,

        InventarioFisico = 6,

        InventarioContabil = 7,

        InventarioEstoque = 8
    }
}
