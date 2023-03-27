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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.StaticFiles;

namespace JetCloud.Pages
{
    public class IndexModel : PageModel
    {
        private AppDbContext _db;
        private IWebHostEnvironment _he;

        public Files uploadedFile = new Files();

        [BindProperty]
        public IFormFile Upload { get; set; }

        public IList<Files> departmentFiles { get; private set; }
        public IList<Department> departments { get;  private set; }


        //[BindProperty]
       // public Files downloadFile { get; set; }

        // will use this to encrpyt all the data eventually
        // IDataProtector _protector;


        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(AppDbContext db, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment he)//, IDataProtectionProvider provider)
        {
            _signInManager = signInManager;
            _db = db;
            _he = he;
            //_protector = provider.CreateProtector("Contoso.Security.BearerToken");

        }
        public void OnGet()
        {
            departmentFiles = _db.DepartmentFiles.OrderByDescending(b => b.fileID).ToList();
            departments = _db.Departments.OrderByDescending(b => b.departmentID).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {

                return Page();
            }

            var currentFile = _db.DepartmentFiles.OrderByDescending(b => b.fileID).FirstOrDefault();

            if (currentFile == null)
            {
                uploadedFile.fileID = 1;
                uploadedFile.fileVersion = 1;
            }
            else
            {
                int fileID = currentFile.fileID + 1;
                uploadedFile.fileID = fileID;

            }
            
            uploadedFile.fileName = Convert.ToString(Upload.FileName);
            uploadedFile.departmentID = Convert.ToInt32(Request.Form["departmentID"]);            
            //uploadedFile.fileType = Convert.ToString(Upload.GetType());
            uploadedFile.fileDate = Convert.ToDateTime(Request.Form["fileDate"]);
            uploadedFile.fileVersion = 1;

            //This was just testing other methods, will switch to appropriate type from download function
            var filePath = Path.Combine(_he.ContentRootPath, "uploads", Upload.FileName);
            uploadedFile.fileType = Convert.ToString(filePath);
            //end of method testing

            using (MemoryStream ms = new MemoryStream(100))
            {
                await Upload.CopyToAsync(ms);
                uploadedFile.fileData = ms.ToArray();
            }
            _db.DepartmentFiles.Add(uploadedFile);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDownloadAsync(int fileID)
        {
            //cannont figure out why fileID not getting sent to Controller function, test id is 5
            var downloadFile = await _db.DepartmentFiles.FindAsync(5);               

            if (!(downloadFile == null))
            {
                //start of adapted code from https://www.aspsnippets.com/Articles/FileContentResult-Net-Core-Example-Using-FileContentResult-in-ASPNet-Core-MVC.aspx
                byte[] bytes = downloadFile.fileData;
                string contentType = "";
                new FileExtensionContentTypeProvider().TryGetContentType(downloadFile.fileName, out contentType);
                //end of adapted code

                //start of adatped code https://dev.to/zoltanhalasz/upload-and-download-pdf-files-to-from-ms-sql-database-using-razor-pages-7jh
                return new FileContentResult(bytes, contentType) { FileDownloadName = downloadFile.fileName };
                //end of adapted code
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }
    }
}