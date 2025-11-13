using ExpenseTracker.Dtos;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Services
{
    public class IdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public IdentityService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            
            return await _userManager.CreateAsync(user, registerData.Password);
        }
    }
}