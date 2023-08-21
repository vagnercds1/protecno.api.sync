using Newtonsoft.Json;
using protecno.api.sync.domain.enumerators;

namespace protecno.api.sync.domain.models.register
{
    public class RegisterDeleteRequest
    {
        public int? BaseInventarioId { get; set; }

        public ERegisterType? TipoRegistroId { get; set; }

        public bool DeleteAll { get; set; }

        public string Codigo { get; set; }
    }
}
