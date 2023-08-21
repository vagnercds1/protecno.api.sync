using CsvHelper.Configuration;
using protecno.api.sync.domain.entities;
using System.Globalization;

namespace protecno.api.sync.domain.mapers
{
    public class CsvMapInventoryFisic : ClassMap<Inventory>
    {
        public CsvMapInventoryFisic()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.VidaUtilMeses).Ignore();
            Map(m => m.VidaUtilAnos).Ignore();
            Map(m => m.PercentualRemanescenteVidaUtil).Ignore();
            Map(m => m.MesesRemanecentes).Ignore();
            Map(m => m.AnosRemanescentes).Ignore();
            Map(m => m.ValorResidual).Ignore();
            Map(m => m.ValorDepreciavel).Ignore();
            Map(m => m.ValorEmUso).Ignore();
            Map(m => m.TesteRecuperabilidade).Ignore();
            Map(m => m.AnoFabricacao).Ignore();
            Map(m => m.ValorOriginal).Ignore();
            Map(m => m.DepreciacaoAcumulada).Ignore();
            Map(m => m.CustoReposicao).Ignore();
            Map(m => m.DataAquisicao).Ignore();
            Map(m => m.DataEntradaOperacao).Ignore();
            Map(m => m.FilialId).Ignore();
            Map(m => m.FilialIdAnterior).Ignore();
            Map(m => m.LocalId).Ignore();
            Map(m => m.CentroCustoId).Ignore();
            Map(m => m.ResponsavelId).Ignore();
            Map(m => m.ContaContabilId).Ignore();
            Map(m => m.ContaContabilCodigo).Ignore();
            Map(m => m.ContaContabil).Ignore();
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
