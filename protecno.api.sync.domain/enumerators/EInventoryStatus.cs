using System.ComponentModel.DataAnnotations;

namespace protecno.api.sync.domain.enumerators
{
    public enum EInventoryStatus
    { 
        [Display(Name = "inserido")]
        Inserted = 1,

        [Display(Name = "revisado")]
        Reviewed = 2,

        [Display(Name = "alterado")]
        Changed = 3,

        [Display(Name = "baixado")]
        WrittenOff = 4,

        [Display(Name = "desconhecido")]
        Unknown = 5
    }
}
