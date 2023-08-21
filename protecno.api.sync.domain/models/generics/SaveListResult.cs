using System.Collections.Generic;

namespace protecno.api.sync.domain.models.generics
{
    public class SaveListResult
    {
        public int QuantityInserted { get; set; }

        public int QuantityUpdated { get; set; }

        public int QuantityDiscarded { get; set; }

        public List<object> DiscartedItens { get; set; } = new List<object>();
    }
}
