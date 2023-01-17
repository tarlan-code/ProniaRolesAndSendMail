using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pronia.Models;
using Pronia.Utilies.Enums;
using Pronia.ViewModels;

namespace Pronia.Controllers
{
    public class AccountController : Controller
    {
        
      
        UserManager<AppUser> _userManager { get; }
        SignInManager<AppUser> _signInManager { get; }
        RoleManager<IdentityRole> _roleManager { get; }


        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterVM regVM)
        {
            if (!ModelState.IsValid) return View();
            AppUser user = new AppUser()
            {
                Name = regVM.Name,
                Surname = regVM.Surname,
                UserName = regVM.Username,
                Email = regVM.Email
            };
            IdentityResult result = await _userManager.CreateAsync(user, regVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }

            await _userManager.AddToRoleAsync(user,Roles.Names.Member.ToString());

            await _signInManager.SignInAsync(user, true);
            return RedirectToAction("Index","Home");
        }


        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginVM loginVM,string? ReturnUrl)
        {
            if (!ModelState.IsValid) return View();

            AppUser appUser = await _userManager.FindByNameAsync(loginVM.UsernameOrEmail);

            if(appUser is null)
            {
                appUser = await _userManager.FindByNameAsync(loginVM.UsernameOrEmail);
                if(appUser is null)
                {
                    ModelState.AddModelError("", "Username or Password is wrong");
                    return View();
                }
            }

            var result = await _signInManager.PasswordSignInAsync(appUser, loginVM.Password, loginVM.IsPresistance,true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Username or Password is wrong");
                return View();
            }

            if(ReturnUrl is not null)
            {
                return Redirect(ReturnUrl);
            }
            return RedirectToAction("Index","Home");
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        //public async Task<IActionResult> Test()
        //{
        //    var user = await _userManager.FindByNameAsync("sabir");
        //    await _userManager.AddToRoleAsync(user,Roles.Names.Superadmin.ToString());
        //    user = await _userManager.FindByNameAsync("tarlan");
        //    await _userManager.AddToRoleAsync(user, Roles.Names.Admin.ToString());
        //    return View();
        //}

        //public async Task<IActionResult> AddRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole { Name="Superadmin"});
        //    await _roleManager.CreateAsync(new IdentityRole { Name = "Admin"});
        //    await _roleManager.CreateAsync(new IdentityRole { Name = "Member"});
        //    return View();
        //}
    }
}
