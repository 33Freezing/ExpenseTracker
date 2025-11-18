using ExpenseTrackerWebApp.Database.Models;
using ExpenseTrackerWebApp.Dtos;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTrackerWebApp.Services
{
    public class IdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly CategoryService _categoryService;
        private readonly UserPreferencesService _userPreferencesService;
        private readonly AccountService _accountService;

        public IdentityService(
        UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
         CategoryService categoryService, UserPreferencesService userPreferencesService, AccountService accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _categoryService = categoryService;
            _userPreferencesService = userPreferencesService;
            _accountService = accountService;
        }

        public async Task<SignInResult> LoginAsync(LoginUserData loginData)
        {
            return await _signInManager.PasswordSignInAsync(
                loginData.Email,
                loginData.Password,
                loginData.RememberMe,
                lockoutOnFailure: false
            );
        }
        
        
        public async Task<IdentityResult> RegisterAsync(RegisterUserData registerData)
        {
            var user = new IdentityUser 
            { 
                UserName = registerData.Email, 
                Email = registerData.Email 
            };
            
            var result = await _userManager.CreateAsync(user, registerData.Password);
            if (!result.Succeeded)
            {
                return result;
            }

            await _categoryService.AssignUserDefaultCategories(user.Id);
            await _userPreferencesService.AssignUserDefaultPreferences(user.Id);
            await _accountService.AssignUserDefaultAccounts(user.Id);
            return result;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}