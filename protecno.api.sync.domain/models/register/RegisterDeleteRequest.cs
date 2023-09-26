using Newtonsoft.Json;
using protecno.api.sync.domain.enumerators;
using System.Collections.Generic;

namespace protecno.api.sync.domain.models.register
{
    public class RegisterDeleteRequest
    {
        public int? BaseInventoryId { get; set; }

        public EInformationType? InformationType { get; set; }

        public bool DeleteAll { get; set; }

        public List<string> RegisterCodeList { get; set; }
    }
}
