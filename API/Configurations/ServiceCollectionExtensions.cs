using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence.Interfaces;
using Infrastructure.Persistence.Repositroies;
using Application.Interfaces;
using Application.Interfaces.Services;
using Infrastructure.Services;
using Infrastructure.Common.Interfaces;
using Infrastructure.Common;
using Skype.Infrastructure.Common;
using Infrastructure.Persistence.Repositories;
using Application.Interfaces.FileHandling;
using API.Helpers;
using API.Helpers.Infrastructure.FileHelpers;
using API.Hubs;

namespace API.Configurations
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
            services.AddScoped<IDbHelper, DbHelper>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add other services here if needed


            ///Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IAuthApplicationService, AuthApplicationService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IChatRoomService, ChatRoomService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IChatHubService, ChatHubService>();


            ///Helpers 
            services.AddScoped<IFileHelper, FileHelpers>();


            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();

            // Add other infrastructure-specific services here

            return services;
        }
    }
}

