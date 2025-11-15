using ExpenseTracker.Database.Models;
using ExpenseTracker.Dtos;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Services
{
    public class IdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly CategoryService _categoryService;

        public IdentityService(
        UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
         AuthenticationStateProvider authStateProvider, CategoryService categoryService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authStateProvider = authStateProvider;
            _categoryService = categoryService;
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
            return result;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}