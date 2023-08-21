using CsvHelper.Configuration;
using protecno.api.sync.domain.entities;
using System.Globalization;

namespace protecno.api.sync.domain.mapers
{   
    public class CsvMapInventoryAccounting : ClassMap<Inventory>
    {
        public CsvMapInventoryAccounting()
        {
            AutoMap(CultureInfo.InvariantCulture);           
            Map(m => m.FilialId).Ignore();
            Map(m => m.FilialIdAnterior).Ignore();
            Map(m => m.LocalId).Ignore();
            Map(m => m.CentroCustoId).Ignore();
            Map(m => m.ResponsavelId).Ignore();
            Map(m => m.ContaContabilId).Ignore();
            Map(m => m.DataSync).Ignore();
            Map(m => m.OrigemId).Ignore();
            Map(m => m.Origem).Ignore();
            Map(m => m.DataCadastro).Ignore();
            Map(m => m.DataAtualizacao).Ignore();
            Map(m => m.DataSync).Ignore();
            Map(m => m.Usuario).Ignore();
            Map(m => m.UsuarioId).Ignore();
        }
    }
}
