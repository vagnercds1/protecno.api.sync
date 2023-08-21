using protecno.api.sync.domain.enumerators;
using System.Collections.Generic;

namespace protecno.api.sync.domain.models.inventory
{
    public class InventoryDeleteRequest
    {
        public int? BaseInventarioId { get; set; }

        public EInventoryType? TipoInventarioId { get; set; }

        public bool DeleteAll { get; set; }

        public List<int>InventoryIdList { get; set; }
    }
}
