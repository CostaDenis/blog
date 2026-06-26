namespace Blog;

public static class Configuration
{
    //Token - JWT - Jason Web Token
    public static string JwtKey = "XHd7Bv82nPa94sd1kZqFv9RxpTy3LsDQ";
    public static string ApiKeyName = "api_key";
    public static string ApiKey = "curso_api_6R0t4E3";
    public static SmtpConfiguration Smtp = new();

    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}