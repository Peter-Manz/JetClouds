using JetCloud.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace JetCloud.Pages
{
    public class DownloadModel : PageModel
    {
        private AppDbContext _db;
        private IWebHostEnvironment _he;

        [BindProperty]
        public Files downloadFile { get; set; }

        public DownloadModel(AppDbContext db, IWebHostEnvironment he)
        {
            _db = db;
            _he = he;
        }

        public async Task<IActionResult> OnGetAstync(int fileID)
        {
            downloadFile = await _db.DepartmentFiles.FindAsync(fileID);
           

            if(!(downloadFile == null))
            {
                var bytes = downloadFile.fileData;
                return File(bytes, "application/octet-stream", downloadFile.fileName);              

            }

            return RedirectToPage("/Index");
        }

    }
}
