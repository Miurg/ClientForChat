using Windows.Security.Credentials;

namespace ClientForChat.Services
{
    public class TokenService
    {
        private const string TokenName = "LoginToken";

        public void SaveToken(string token)
        {
            var vault = new PasswordVault();
            var passwordCredential = new PasswordCredential(TokenName, "user", token);
            vault.Add(passwordCredential);
        }

        public string GetToken()
        {
            var vault = new PasswordVault();
            var credentials = vault.Retrieve(TokenName, "user");
            return credentials.Password;
        }

        public void DeleteToken()
        {
            var vault = new PasswordVault();
            var credentials = vault.Retrieve(TokenName, "user");
            vault.Remove(credentials);
        }
    }
}