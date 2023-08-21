using CsvHelper.Configuration;
using protecno.api.sync.domain.entities;
using System.Globalization;

namespace protecno.api.sync.domain.mapers
{
    public class CsvMapRegister : ClassMap<Register>
    {
        public CsvMapRegister()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.TipoRegistroId).Ignore();
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
