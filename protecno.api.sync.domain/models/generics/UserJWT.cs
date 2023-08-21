using System;

namespace protecno.api.sync.domain.models.generics
{
    public class UserJwt
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string TagCustomer { get; set; }
        public double ExpiresPreSignedURL { get; set; }
        public int CustomerId { get; set; }
        public int BaseInventoryId { get; set; }
        public bool StartedInventory { get; set; }
    }
}
