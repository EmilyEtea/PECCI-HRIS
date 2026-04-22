using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PECCI_HRIS.ViewModels
{
    public class EmployeeViewModel
    {
        public int EmployeeID { get; set; }

        [Required, Display(Name = "Employee No.")]
        public string EmployeeNo { get; set; } = string.Empty;

        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Suffix")]
        public string? Suffix { get; set; }

        [Required, Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required, Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Civil Status")]
        public string? CivilStatus { get; set; }

        [Display(Name = "Nationality")]
        public string? Nationality { get; set; } = "Filipino";

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Display(Name = "Personal Email")]
        [EmailAddress]
        public string? PersonalEmail { get; set; }

        [Display(Name = "Company Email")]
        [EmailAddress]
        public string? CompanyEmail { get; set; }

        // Government IDs
        [Display(Name = "SSS Number")]
        public string? SSSNumber { get; set; }

        [Display(Name = "PhilHealth Number")]
        public string? PhilHealthNumber { get; set; }

        [Display(Name = "Pag-IBIG Number")]
        public string? PagIbigNumber { get; set; }

        [Display(Name = "TIN Number")]
        public string? TINNumber { get; set; }

        // Employment
        [Required, Display(Name = "Department")]
        public int DepartmentID { get; set; }

        [Required, Display(Name = "Position")]
        public int PositionID { get; set; }

        [Required, Display(Name = "Date Hired")]
        [DataType(DataType.Date)]
        public DateTime DateHired { get; set; }

        [Display(Name = "Date Regularized")]
        [DataType(DataType.Date)]
        public DateTime? DateRegularized { get; set; }

        [Required, Display(Name = "Employment Status")]
        public string EmploymentStatus { get; set; } = "Regular";

        [Required, Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        // Dropdowns
        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Positions { get; set; } = new List<SelectListItem>();
    }

    public class EmployeeListViewModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateHired { get; set; }
    }
}
