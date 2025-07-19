using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Configurations
{
    public static class AuthenticationConfigurations
    {
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, ConfigurationManager Configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
            {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = Configuration["Jwt:Issuer"],
                                ValidAudience = Configuration["Jwt:Audience"],
                                IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!)),
                                ClockSkew = TimeSpan.Zero
                    };

                    // For debugging
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                                Console.WriteLine($"Auth failed: {context.Exception.Message}");
                                return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("Auth succeeded");
                            return Task.CompletedTask;
                        },

                        // For SignalR
                        OnMessageReceived = context =>
                         {
                             // Only allow tokens for SignalR endpoints
                             var accessToken = context.Request.Query["access_token"];
                             var path = context.HttpContext.Request.Path;
                             if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                             {
                                 context.Token = accessToken;
                             }
                             return Task.CompletedTask;
                         }
                    };

                
            });
        
        
            return services;
        }
    }
}
