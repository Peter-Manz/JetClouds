using JetCloud.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace JetCloud.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IList<Files> departmentFiles { get; private set; }
        public IList<Department> departments { get; private set; }
       
 

        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(AppDbContext db, SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
            _db = db;
           
        }
        public void OnGet()
        {
            departmentFiles = _db.DepartmentFiles.OrderByDescending(b => b.fileID).ToList();
            departments = _db.Departments.OrderByDescending(b => b.departmentID).ToList();
        }
    }
}