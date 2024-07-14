using Microsoft.IdentityModel.Tokens;
using webapp.DAL.Tools;

namespace david_api.Middlewares
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenMiddleware(RequestDelegate next, string key, string issuer, string audience)
        {
            _next = next;
            _key = key;
            _issuer = issuer;
            _audience = audience;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                AttachUserToContext(context, token);

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var principal = TokenService.ValidateToken(token, _key, _issuer, _audience);
                context.User = principal;
            }
            catch (SecurityTokenValidationException)
            {
                // Token validation failed
            }
        }
    }
}
