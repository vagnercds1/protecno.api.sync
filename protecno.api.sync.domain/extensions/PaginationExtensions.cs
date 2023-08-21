namespace protecno.api.sync.domain.extensions
{
    public static class PaginationExtensions
    {
        public static string Paginate(int pageSize, int page, string orderField, string order)
        { 
            string pagination = $"order by {orderField} {order} LIMIT {(page) * pageSize},{pageSize} ";

            return pagination;
        }
    }
}
