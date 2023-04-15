using JetCloud.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Metadata;

namespace JetCloud.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public Registration Input { get; set; }

        IDataProtector _protector;

        private AppDbContext _db;

        public Users newUser = new Users();

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public RegisterModel(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm, AppDbContext db, IDataProtectionProvider provider)
        {
            _signInManager = sm;
            _userManager = um;
            _db = db;
            _protector = provider.CreateProtector("Contoso.Security.BearerToken");
            _roleManager = roleManager;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.UserName, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    //await CreateRoles();
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await _userManager.AddToRoleAsync(user, "Member");
                    var currentUsers = _db.Users.OrderByDescending(b => b.UserID).FirstOrDefault();

                    if (currentUsers == null)
                    {
                        newUser.UserID = 1;
                    }
                    else
                    {
                        newUser.UserID = currentUsers.UserID + 1;
                    }
                    newUser.Email = _protector.Protect(Input.Email);
                    newUser.GivenName = Convert.ToString(Request.Form["GivenName"]);
                    newUser.SurName = Convert.ToString(Request.Form["SurName"]);
                    newUser.Title = Convert.ToString(Request.Form["Title"]);
                    newUser.DepartmentID = Convert.ToInt32(Request.Form["DepartmentID"]);
                    newUser.SubscriptionStatus = true;
                    newUser.Dob = Convert.ToDateTime(Request.Form["Dob"]);
                    _db.Users.Add(newUser);
                    await _db.SaveChangesAsync();
                    return RedirectToPage("/Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
                return Page();
        }
        private async Task CreateRoles()
        {
            String[] roleNames = { "Admin", "Member" };
            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            var _user = await _userManager.FindByNameAsync("peter@chester.ac.uk");
            if (_user != null)
            {
                await _userManager.AddToRoleAsync(_user, "Admin");
            }
        }

    }
}
