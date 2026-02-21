namespace OrderItApp.Models
{
    public class AuthViewModel
    {
        public LoginViewModel Login { get; set; } = new LoginViewModel();
        public SignupViewModel Signup { get; set; } = new SignupViewModel();
    }
}