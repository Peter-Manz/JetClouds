using System.ComponentModel.DataAnnotations;

namespace JetCloud.Data
{
    public class Department
    {
        [Key]
        public int departmentID { get; set; }

        [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} and at MAX {1} characters long.", MinimumLength = 2)]
        public string departmentName { get; set; }
    }
}
