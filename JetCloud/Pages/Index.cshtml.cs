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
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Authorization;
using static System.Net.WebRequestMethods;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Claims;

namespace JetCloud.Pages
{
    [Authorize]
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
        public string CurrentFilter { get; set; }
        public string DepartmentSort { get; set; }
        public string CurrentDepartment { get; set; }
        public string CurrentUser { get; set; }

        IDataProtector _protector;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(AppDbContext db, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> um, IWebHostEnvironment he, IDataProtectionProvider provider)
        {
            _signInManager = signInManager;
            _userManager = um;
            _db = db;
            _he = he;
            _protector = provider.CreateProtector("Contoso.Security.BearerToken");

        }
        //Start of Adapted Code (Tdykstra, n.d.)

        public async Task OnGetAsync(string sortOrder, string search, int? depID)
        {

            var currentUser = User.FindFirstValue(ClaimTypes.Email);
            var currentAspUser = await _userManager.FindByEmailAsync(currentUser);
            var currentRoles = await _userManager.GetRolesAsync(currentAspUser);
            var assingedRole = currentRoles[0];


            foreach (var user in _db.Users)
            {
                string decryptedString = _protector.Unprotect(user.Email);
                if (decryptedString == currentUser){
                    CurrentUser = user.Title + " " + user.GivenName  + " " + user.SurName;
                    if (assingedRole != "Admin")
                    {
                        depID = user.DepartmentID;
                    }
                }
            }
              
            DepartmentSort = String.IsNullOrEmpty(sortOrder) ? "dep_sort" : "";

            if (depID == null){
                depID = 0;
            }
           
            CurrentFilter = search;

            foreach (var dep in _db.Departments)
            {
                if (dep.departmentID == depID)
                {
                    var currentDepName = dep.departmentName;
                    CurrentDepartment = Convert.ToString(currentDepName);
                }
            }
            IQueryable <Files> depFiles = from f in _db.DepartmentFiles select f;

            if (!string.IsNullOrEmpty(search))
            {
                depFiles = depFiles.Where(b => b.fileName.Contains(search));
            }
            if (assingedRole != "Admin")
            {
                depFiles = depFiles.Where(b => b.departmentID == depID);
            }
            else
            {
                switch (sortOrder)
                {
                    case "dep_sort":
                        depFiles = depFiles.Where(b => b.departmentID == depID);
                        break;
                    default:
                        depFiles = depFiles.OrderBy(b => b.departmentID);
                        break;
                }
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
            var currentUser = User.FindFirstValue(ClaimTypes.Email);
            var currentAspUser = await _userManager.FindByEmailAsync(currentUser);
            var currentRoles = await _userManager.GetRolesAsync(currentAspUser);
            var assingedRole = currentRoles[0];

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
            
            if (assingedRole == "Admin")
            {
                uploadedFile.departmentID = Convert.ToInt32(Request.Form["departmentID"]);
            }
            else {
                foreach (var user in _db.Users)
                {
                    string decryptedString = _protector.Unprotect(user.Email);
                    if (decryptedString == currentUser)
                    {
                        uploadedFile.departmentID = user.DepartmentID;
                    }
                }
               
            }
            uploadedFile.fileDate = DateTime.Now;
            uploadedFile.fileVersion = 1;

            //This was just testing other methods, will switch to appropriate type from download function
            var filePath = Path.Combine(_he.ContentRootPath, "uploads", Upload.FileName);
            //uploadedFile.fileType = _protector.Protect(Convert.ToString(filePath));
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
           
            var downloadFile = await _db.DepartmentFiles.FindAsync(fileID);               

            if (!(downloadFile == null))
            {
                //start of adapted code from (Khan, n.d.)
                byte[] bytes = downloadFile.fileData;
                string contentType = "";
                new FileExtensionContentTypeProvider().TryGetContentType(downloadFile.fileName, out contentType);
                //end of adapted code

                //start of adatped code (Halasz, 2020)
                return new FileContentResult(bytes, contentType) { FileDownloadName = downloadFile.fileName };
                //end of adapted code
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }
        public async Task<IActionResult> OnPostDownloadAllAsync()
        {
            //Start of adapted Code (Zhi, 2022)
           
            var fileCapacity = await _db.DepartmentFiles.ToListAsync();
            var zipName = $"JetClouds-{DateTime.Now.ToString("yyyy_MM_dd")}.zip";
            if (fileCapacity != null) {
                using (var ms = new MemoryStream())
                {
                    using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in _db.DepartmentFiles)
                        {
                            var entry = zip.CreateEntry(file.fileName);
                            using (var fs = new MemoryStream(file.fileData))
                            using (var es = entry.Open())
                            {
                                fs.CopyTo(es);
                            }

                        }
                    }
                    return File(ms.ToArray(), "JetClouds/Zip", zipName);
                }

            }
            //end of Adapted Code

            return RedirectToPage("/Index");
        }
        public async Task<IActionResult> OnPostExcelAsync()
        {
            //Start of adapted code from (Ardal, 2020)
            var fileCapacity = await _db.DepartmentFiles.ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("fileName,fileDate,fileType,fileData,fileVersion,departmentID,fileID");

            if (fileCapacity != null){
                foreach (var file in _db.DepartmentFiles)
                {
                    builder.AppendLine($"{file.fileName},{file.fileDate},{file.fileType},{file.fileData},{file.fileVersion},{file.departmentID},{file.fileID}");
                }
                return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "JetCloudFiles.csv");
            }      
            return RedirectToPage("/Index");
            //end of adapted code
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
        private async Task<string> getRoleAsync()
        {
            var currentUser = User.FindFirstValue(ClaimTypes.Email);
            var currentAspUser = await _userManager.FindByEmailAsync(currentUser);
            var currentRoles = await _userManager.GetRolesAsync(currentAspUser);
            return currentRoles[0];
        }
    }
}