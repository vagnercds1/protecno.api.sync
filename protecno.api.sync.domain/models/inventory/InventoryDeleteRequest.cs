using protecno.api.sync.domain.enumerators;
using System.Collections.Generic;

namespace protecno.api.sync.domain.models.inventory
{
    public class InventoryDeleteRequest
    {
        public int? BaseInventoryId { get; set; }

        public EInformationType? InformationType { get; set; }

        public bool DeleteAll { get; set; }

        public List<int>InventoryIdList { get; set; }
    }
}
