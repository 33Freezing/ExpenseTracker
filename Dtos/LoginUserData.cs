namespace ExpenseTracker.Dtos
{
    public class LoginUserData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public LoginUserData()
        {
            Email = "";
            Password = "";
            RememberMe = false;
        }
    }
}