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
using System.Diagnostics.Tracing;
using System;
using System.Security.Cryptography.Pkcs;
using Microsoft.Data.SqlClient;

namespace JetCloud.Pages
{
    public class IndexModel : PageModel
    {
        private AppDbContext _db;
        private IWebHostEnvironment _he;

        public Files uploadedFile = new Files();

        [BindProperty]
        public IFormFile Upload { get; set; }

        public IList<Files> DepartmentFiles { get; set; }
        public IList<Department> Departments { get; set; }
        public IList<Users> Users { get; set; }

        public string NameSort { get; set; }
        public string CurrentFilter { get; set; }


        //[BindProperty]
        // public Files downloadFile { get; set; }

         IDataProtector _protector;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(AppDbContext db, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment he, IDataProtectionProvider provider)
        {
            _signInManager = signInManager;
            _db = db;
            _he = he;
            _protector = provider.CreateProtector("Contoso.Security.BearerToken");

        }
        //Start of Adapted Code https://learn.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-6.0
        public async Task OnGetAsync(string sortOrder, string search)
        {
           
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            CurrentFilter = search;

            IQueryable<Files> depFiles = from f in _db.DepartmentFiles select f;
            //IQueryable<Files> depzFiles;
   
            //foreach (var file in _db.DepartmentFiles)
            //{
            //    var fileName = _protector.Unprotect(file.fileName);
            //    var fileType = _protector.Unprotect(file.fileType);
            //    var fileData = _protector.Unprotect(file.fileData);
            //    var readableFile = (file.fileID, fileName, file.fileDate, fileType, fileData, file.fileVersion, file.departmentID);
            //}


            if (!string.IsNullOrEmpty(search))
            {
                depFiles =  depFiles.Where(b => b.fileName.Contains(search));
            }

            switch (sortOrder) 
            {
                case "name_desc":
                    depFiles = depFiles.OrderByDescending(b => b.fileName);
                    break;

                default:
                    depFiles = depFiles.OrderBy(b => b.departmentID);
                    break;
            }
            DepartmentFiles = await depFiles.ToListAsync();
            
            Departments = await _db.Departments.ToListAsync();
            Users = await _db.Users.OrderByDescending(b => b.UserID).ToListAsync();

        }
        //end of adapted Code

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

            uploadedFile.fileName = _protector.Protect(Convert.ToString(Upload.FileName));

            //uploadedFile.fileName = Convert.ToString(Upload.FileName);
            uploadedFile.departmentID = Convert.ToInt32(Request.Form["departmentID"]);            
            //uploadedFile.fileType = Convert.ToString(Upload.GetType());
            uploadedFile.fileDate = Convert.ToDateTime(Request.Form["fileDate"]);
            uploadedFile.fileVersion = 1;

            //This was just testing other methods, will switch to appropriate type from download function
            var filePath = Path.Combine(_he.ContentRootPath, "uploads", Upload.FileName);
            uploadedFile.fileType = _protector.Protect(Convert.ToString(filePath));
            //uploadedFile.fileType = Convert.ToString(filePath);
            //end of method testing

            using (MemoryStream ms = new MemoryStream(100))
            {
                await Upload.CopyToAsync(ms);
                uploadedFile.fileData = _protector.Protect(ms.ToArray());
                //uploadedFile.fileData = ms.ToArray();
            }
            _db.DepartmentFiles.Add(uploadedFile);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDownloadAsync(int fileID)
        {
           
            var downloadFile = await _db.DepartmentFiles.FindAsync(fileID);               

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
        public async Task<IActionResult> OnPostDeleteAsync(int? fileID)
        {
            if(fileID == null)
            {
                return NotFound();
            }
            var file = await _db.DepartmentFiles.FindAsync(fileID);
           
            if (file == null)
            {
                return NotFound();
            }
            try
            {
                _db.DepartmentFiles.Remove(file);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new Exception($"Item {file.fileID} not found!", e);
            }
            return RedirectToPage("./Index");
        }
    }
}