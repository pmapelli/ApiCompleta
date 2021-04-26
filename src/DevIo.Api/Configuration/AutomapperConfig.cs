using AutoMapper;
using DevIo.Api.Dtos;
using DevIO.Business.Models;

namespace DevIo.Api.Configuration
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<Fornecedor, FornecedorDto>().ReverseMap();
            CreateMap<Endereco, EnderecoDto>().ReverseMap();
            CreateMap<ProdutoDto, Produto>();
            
            CreateMap<ProdutoImagemDto, Produto>().ReverseMap();
            
            CreateMap<Produto, ProdutoDto>().ForMember(dest => dest.NomeFornecedor, 
                opt => 
                    opt.MapFrom(src => src.Fornecedor.Nome));
        }
    }
}
