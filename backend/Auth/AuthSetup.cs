using Microsoft.AspNetCore.Authentication.Cookies;

namespace backend.Auth;

// Вход через cookie (не JWT). Сервер после входа ставит cookie fitness_auth,
// браузер сам присылает её с каждым запросом.
public static class AuthSetup
{
    public const string CookieName = "fitness_auth";

    public static IServiceCollection AddCookieLogin(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = CookieName;
                // JavaScript страницы cookie не видит - её нельзя украсть через XSS
                options.Cookie.HttpOnly = true;
                // Vue на :5173 и API на :5000 - один сайт localhost, поэтому Lax достаточно
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                // У API нет страницы входа, поэтому вместо перенаправления - коды:
                // не вошёл - 401, вошёл, но нельзя - 403.
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization();
        return services;
    }
}
