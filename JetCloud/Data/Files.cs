using System;
using System.ComponentModel.DataAnnotations;
namespace JetCloud.Data
{
    public class Files
    {
        [Key]
        public int fileID { get; set; }
        [Required ,StringLength(75, ErrorMessage = "The {0} must be at least {2} and at MAX {1} characters long.", MinimumLength = 3)]
        public string fileName { get; set; }
        public DateTime fileDate { get; set;}
        public String fileType { get; set; }
        public byte[] fileData { get; set; }
        public int fileVersion { get; set; }
        public int departmentID { get; set; }
    }
}
