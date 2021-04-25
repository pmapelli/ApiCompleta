using DevIO.Data.Context;
using DevIO.Data.Repository;
using DevIO.Business.Services;
using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.Extensions.DependencyInjection;

namespace DevIo.Api.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            services.AddScoped<MeuDbContext>();
            // services.AddScoped<IProdutoRepository, ProdutoRepository>();
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();
            services.AddScoped<IFornecedorRepository, FornecedorRepository>();
            
            services.AddScoped<INotificador, Notificador>();
            // services.AddScoped<IProdutoService, ProdutoService>();
            services.AddScoped<IFornecedorService, FornecedorService>();

            return services;
        }
    }
}
