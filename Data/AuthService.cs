using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography;
using TestMVC.Models;

namespace TestMVC.Data
{
    public class AuthService : IAuthService
    {

        private readonly DatabaseContext _context;
        private readonly IUtilityService _utility;

        private const string LDAP_PATH = "localhost";
        private const string LDAP_ADMIN = "cn=admin";
        private const string LDAP_ORG = "dc=testorg,dc=local";
        private const string LDAP_PASS = "admin";

        public AuthService(DatabaseContext context, IUtilityService utility)
        {
            _context = context;
            _utility = utility;
        }

        public SearchResponse LDAPLogin(string username, string password)
        {
            try
            {
                var identifier = new LdapDirectoryIdentifier(LDAP_PATH, 389);
                var creds = new NetworkCredential($"{LDAP_ADMIN},{LDAP_ORG}", LDAP_PASS);
                using var con = new LdapConnection(identifier);
                con.AuthType = AuthType.Basic;
                con.SessionOptions.ProtocolVersion = 3;
                con.Bind(creds);

                //search user based on username + password
                var request = new SearchRequest(LDAP_ORG, $"(uid={username})", SearchScope.Subtree, null);
                var response = (SearchResponse)con.SendRequest(request);

                if (response.Entries.Count == 0) return null;

                string winuser = response.Entries[0].DistinguishedName;

                using var usrcon = new LdapConnection(identifier);
                usrcon.AuthType = AuthType.Basic;
                usrcon.SessionOptions.ProtocolVersion = 3;
                var logincred = new NetworkCredential(winuser, password);
                usrcon.Bind(logincred);


                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            await Task.Delay(1000);

            //login db

            return true;
        }

        public async Task<string> GenerateUserTokenAsync(string username, string email)
        {
            var rng = RandomNumberGenerator.Create();
            var numbers = new byte[6];
            rng.GetBytes(numbers);

            string otp = "";

            foreach (var b in numbers)
            {
                otp += (b % 10).ToString();
            }

            UserToken? token = await _context.UserToken.FirstOrDefaultAsync(x => x.UserId == username && x.Active == true);

            if (token == null)
            {
                token = new UserToken { UserId = username, Email = email, Token = otp, Active = true };
                _context.Add(token);
            }
            else
            {
                token.Token = otp;
                token.Active = true;
            }


            await _context.SaveChangesAsync();

            return otp;
        }

        public async Task<bool> ValidateUserTokenAsync(string username, string token)
        {
            var otp = await _context.UserToken.FirstOrDefaultAsync(x => x.UserId == username && x.Active == true);

            if (otp == null) return false;
            if (otp.Token != token) return false;

            otp.Active = false;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
