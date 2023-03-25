using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace JetCloud.Data
{
    public class Users
    {
        public int UserID { get; set; }
        [StringLength(50)]
        public string? GivenName { get; set; }
        [StringLength(50)]
        public string? SurName { get; set; }
        public string? Title { get; set; }
        public string? Department { get; set; }
        public DateTime Dob { get; set; }
        public Boolean SubscriptionStatus { get; set; }
        [Key, StringLength(75)]
        public string? Email { get; set; }
    }
}
