using Microsoft.AspNetCore.Mvc.RazorPages;
using protecno.api.sync.domain.entities;

namespace protecno.api.sync.domain.models.register
{
    public class RegisterPaginateRequest : Register
    {
        public RegisterPaginateRequest()
        {
            Page = Page == null ? 0 : Page;
            PageSize = PageSize == null ? 20 : PageSize;
            OrderField = string.IsNullOrEmpty(OrderField) ? "Codigo" : OrderField;
            Order = string.IsNullOrEmpty(Order) ? "Asc" : Order;
        }

        #region Pagination 

        public int? Page { get; set; }

        public int? PageSize { get; set; }

        public string OrderField { get; set; }

        public string Order { get; set; }
         
        #endregion 
    }
}
