using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Elo_fotbalek.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Elo_fotbalek.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IBlobClient blobClient;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IBlobClient blobClient)
            : base(options, logger, encoder)
        {
            this.blobClient = blobClient;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);

                if (authHeader.Scheme != "Basic")
                {
                    return AuthenticateResult.NoResult();
                }

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                if (credentials.Length != 2)
                {
                    return AuthenticateResult.Fail("Invalid Basic Auth header format");
                }

                var username = credentials[0];
                var password = credentials[1];

                var users = await this.blobClient.GetUsers();
                var user = users.FirstOrDefault(u =>
                    u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (user == null || user.Password != password)
                {
                    return AuthenticateResult.Fail("Invalid username or password");
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("FullName", user.Name),
                    new Claim(ClaimTypes.Role, "Administrator")
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = "application/json";
            return Response.WriteAsync("{\"success\":false,\"error\":\"Authentication required\"}");
        }
    }
}
