using FluentValidation.Results;
using protecno.api.sync.domain.entities;

namespace protecno.api.sync.domain.models.inventory
{
    public class InventoryValidationResult: ValidationResult
    {
        public string Message { get; set; }

        public Inventory InventoryObject { get; set; }
    }
}
