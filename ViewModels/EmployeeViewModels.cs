using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PECCI_HRIS.ViewModels
{
    public class EmployeeViewModel
    {
        public int EmployeeID { get; set; }

        [Required, Display(Name = "Employee No.")]
        [StringLength(8, MinimumLength = 7, ErrorMessage = "Employee No. must be 7 to 8 characters.")]
        public string EmployeeNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(10)]
        [Display(Name = "Suffix")]
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Civil Status")]
        public string? CivilStatus { get; set; }

        [Display(Name = "Nationality")]
        public string? Nationality { get; set; } = "Filipino";

        [MaxLength(300)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        // Philippine mobile: 09XXXXXXXXX or +639XXXXXXXXX
        [Display(Name = "Contact Number")]
        [RegularExpression(@"^(09|\+639)\d{9}$",
            ErrorMessage = "Enter a valid Philippine mobile number (e.g. 09171234567 or +639171234567).")]
        public string? ContactNumber { get; set; }

        [Display(Name = "Personal Email")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? PersonalEmail { get; set; }

        [Display(Name = "Company Email")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? CompanyEmail { get; set; }

        // Government IDs — Philippine formats
        [Display(Name = "SSS Number")]
        [RegularExpression(@"^\d{2}-\d{7}-\d{1}$",
            ErrorMessage = "SSS format: XX-XXXXXXX-X (e.g. 33-1234567-8).")]
        public string? SSSNumber { get; set; }

        [Display(Name = "PhilHealth Number")]
        [RegularExpression(@"^\d{2}-\d{9}-\d{1}$",
            ErrorMessage = "PhilHealth format: XX-XXXXXXXXX-X (e.g. 12-345678901-2).")]
        public string? PhilHealthNumber { get; set; }

        [Display(Name = "Pag-IBIG Number")]
        [RegularExpression(@"^\d{4}-\d{4}-\d{4}$",
            ErrorMessage = "Pag-IBIG format: XXXX-XXXX-XXXX (e.g. 1234-5678-9012).")]
        public string? PagIbigNumber { get; set; }

        [Display(Name = "TIN Number")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{3}(-\d{3})?$",
            ErrorMessage = "TIN format: XXX-XXX-XXX or XXX-XXX-XXX-XXX (e.g. 123-456-789-000).")]
        public string? TINNumber { get; set; }

        // Employment
        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        [Display(Name = "Position")]
        public int PositionID { get; set; }

        [Required(ErrorMessage = "Date hired is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date Hired")]
        public DateTime DateHired { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Regularized")]
        public DateTime? DateRegularized { get; set; }

        [Required(ErrorMessage = "Employment status is required.")]
        [Display(Name = "Employment Status")]
        public string EmploymentStatus { get; set; } = "Regular";

        [Required(ErrorMessage = "Status is required.")]
        [Display(Name = "Status")]
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
