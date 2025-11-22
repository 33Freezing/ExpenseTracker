using ExpenseTrackerWebApp.Dtos;
using ExpenseTrackerWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ExpenseTrackerWebApp.Controllers
{
    
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IdentityService _identityService; 
        private readonly TransactionService _transactionService;

        public AuthController(IdentityService identityService, TransactionService transactionService)
        {
            _identityService = identityService;
            _transactionService = transactionService;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var userData = new LoginUserData(){Email = email, Password = password};
            var result = await _identityService.LoginAsync(userData);
            
            if (result.Succeeded)
            {
                await _transactionService.CheckForReoccuringTransactions();
                return Redirect("/");
            }
            
            return Redirect($"/login?error=true&email={email}");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _identityService.LogoutAsync();
            return Redirect("/login");
        }
    }
}
