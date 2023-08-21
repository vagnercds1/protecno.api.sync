using FluentValidation.Results;
using protecno.api.sync.domain.entities;

namespace protecno.api.sync.domain.models.inventory
{
    public class InsertResult
    {
        public InsertResult()
        {
            Result = new ValidationResult();
        }

        public Inventory RequestInventory { get; set; }

        public ValidationResult Result { get; set; }
    }
}
