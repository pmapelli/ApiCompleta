using DevIO.Data.Context;
using DevIO.Data.Repository;
using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using DevIO.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevIo.Api.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            services.AddScoped<MeuDbContext>();
            services.AddScoped<IFornecedorRepository, FornecedorRepository>();
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();
            services.AddScoped<IFornecedorService, FornecedorService>();
            services.AddScoped<INotificador, Notificador>();

            return services;
        }
    }
}
