using System.ComponentModel.DataAnnotations;

namespace protecno.api.sync.domain.enumerators
{
    public enum EOrigin
	{
        [Display(Name = "Invalid")]
        Invalid = 0,

        [Display(Name = "Web")]
		Web = 1,

		[Display(Name = "APP")]
		APP = 2,

        [Display(Name = "API")]
		API = 3,

	    [Display(Name = "IOT")]
		IOT = 4,

		[Display(Name = "CSV")]
		CSV = 5
	}
}
