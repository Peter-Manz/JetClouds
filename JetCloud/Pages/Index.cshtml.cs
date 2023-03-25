using JetCloud.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.IO;

namespace JetCloud.Pages
{
    public class IndexModel : PageModel
    {
        private AppDbContext _db;
        private IWebHostEnvironment _he;

        [BindProperty]
        public Files newFile { get; set; }

        //[BindProperty]
        [Display(Name ="File")]
        public IFormFile FormFile { get; set; }

        public IList<Files> departmentFiles { get; private set; }
        public IList<Department> departments { get; private set; }

        IDataProtector _protector;


        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(AppDbContext db, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment he, IDataProtectionProvider provider)
        {
            _signInManager = signInManager;
            _db = db;
            _he = he;
            _protector = provider.CreateProtector("Contoso.Security.BearerToken");

        }
        public void OnGet()
        {
            departmentFiles = _db.DepartmentFiles.OrderByDescending(b => b.fileID).ToList();
            departments = _db.Departments.OrderByDescending(b => b.departmentID).ToList();
        }
        public async Task<IActionResult> OnPostAsync(IFormFile file)
        {
            if (!ModelState.IsValid) { return Page(); }

            //var fileDic = "Files";
            //string filePath = Path.Combine(_he.WebRootPath, fileDic);


            //var filePath = Path.GetTempFileName();

            //newFile.fileName = file.FileName;

            var currentFile = _db.DepartmentFiles.OrderByDescending(b => b.fileID).FirstOrDefault();
            if (currentFile == null)
            {
                newFile.fileID = 1;
            }
            else
            {
                newFile.fileID = currentFile.fileID + 1;
                newFile.fileVersion = currentFile.fileVersion + 1;
                using (var ms = new MemoryStream())
                {
                    await FormFile.CopyToAsync(ms);

                    if (ms.Length < 2097152)
                    {
                        newFile.fileData = ms.ToArray();
                       
                        _db.DepartmentFiles.Add(newFile);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        ModelState.AddModelError("File", "The file is too large.");
                    }
                }
             
            }
            return RedirectToPage();
        }

    }
}