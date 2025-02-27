using System.Net.Http.Headers;
using System.Text;

namespace EmulatorWebApp
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private const string USERNAME = "root";  // Defina o usuário fixo
        private const string PASSWORD = "impinj";  // Defina a senha fixa

        public BasicAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.Headers["WWW-Authenticate"] = "Basic";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Autenticação necessária.");
                return;
            }

            var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

            if (credentials.Length != 2 || credentials[0] != USERNAME || credentials[1] != PASSWORD)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Credenciais inválidas.");
                return;
            }

            await _next(context); // Autenticado, passa para o próximo middleware
        }
    }
}