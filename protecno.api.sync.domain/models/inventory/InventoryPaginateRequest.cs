using Newtonsoft.Json;
using protecno.api.sync.domain.entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace protecno.api.sync.domain.models.inventory
{
    public class InventoryPaginateRequest : Inventory
    {
        public InventoryPaginateRequest()
        {
            Page = Page == null ? 0 : Page;
            PageSize = PageSize == null ? 20 : PageSize;
            OrderField = string.IsNullOrEmpty(OrderField) ? "Codigo" : OrderField;
            Order = string.IsNullOrEmpty(Order) ? "Asc" : Order;
        }

        public int? Page { get; set; }

        public int? PageSize { get; set; }

        public string OrderField { get; set; }

        public string Order { get; set; }
    }
}
