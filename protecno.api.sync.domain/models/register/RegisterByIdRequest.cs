using protecno.api.sync.domain.enumerators;

namespace protecno.api.sync.domain.models.register
{
    public class RegisterByIdRequest
    {
        public int? BaseInventarioId { get; set; }
        
        public int? RegisterId { get; set; }

        public EInformationType? InformationType { get; set; }
    }
}
