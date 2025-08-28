using System.DirectoryServices.Protocols;

namespace TestMVC.Data
{
    public interface IAuthService
    {
        public Task<bool> LoginAsync(string username, string password);
        public SearchResponse LDAPLogin(string username, string password);
        public Task<string> GenerateUserTokenAsync(string username, string email);
        public Task<bool> ValidateUserTokenAsync(string username, string token);
    }
}
