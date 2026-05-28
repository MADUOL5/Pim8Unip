using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using StreamingAPI.Services;

namespace StreamingAPI.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TokenService tokenService)
        {
            if (!ShouldValidateToken(context))
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await WriteUnauthorizedAsync(context, "Token nao informado.");
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                await WriteUnauthorizedAsync(context, "Token invalido.");
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, tokenService.GetValidationParameters(), out _);
                context.User = principal;

                await _next(context);
            }
            catch (SecurityTokenException)
            {
                await WriteUnauthorizedAsync(context, "Token invalido ou expirado.");
            }
            catch (ArgumentException)
            {
                await WriteUnauthorizedAsync(context, "Token invalido.");
            }
        }

        private static bool ShouldValidateToken(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                return false;
            }

            if (HttpMethods.IsOptions(context.Request.Method))
            {
                return false;
            }

            if (context.Request.Path.StartsWithSegments("/api/auth"))
            {
                return false;
            }

            var endpoint = context.GetEndpoint();
            return endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() == null;
        }

        private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message
            }));
        }
    }
}
