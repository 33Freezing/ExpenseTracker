namespace ExpenseTracker.Dtos
{
    public class RegisterUserData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }

        public RegisterUserData()
        {
            Email = "";
            Password = "";
            PasswordConfirm = "";
        }
    }
}