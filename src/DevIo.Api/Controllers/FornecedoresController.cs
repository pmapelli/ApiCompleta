using System;
using AutoMapper;
using DevIo.Api.Dtos;
using DevIO.Api.Extensions;
using DevIO.Business.Models;
using System.Threading.Tasks;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace DevIo.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class FornecedoresController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IFornecedorService _fornecedorService;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IFornecedorRepository _fornecedorRepository;

        public FornecedoresController(IFornecedorRepository fornecedorRepository, IMapper mapper, 
            IFornecedorService fornecedorService, INotificador notificador, 
            IEnderecoRepository enderecoRepository, IUser user) : base(notificador, user)
        {
            _mapper = mapper;
            _fornecedorService = fornecedorService;
            _enderecoRepository = enderecoRepository;
            _fornecedorRepository = fornecedorRepository;            
        }

        [HttpGet]
        public async Task<IEnumerable<FornecedorDto>> ObterTodos()
        {
            var fornecedor = _mapper.Map<IEnumerable<FornecedorDto>>(await _fornecedorRepository.ObterTodos());

            return fornecedor;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> ObterPorId(Guid id)
        {
            var fornecedor = await ObterFornecedorProdutosEndereco(id);

            if (fornecedor == null) return NotFound();

            return fornecedor;
        }

        [HttpGet("obter-endereco/{id:guid}")]
        public async Task<ActionResult<EnderecoDto>> ObterEnderecoPorId(Guid id)
        {
            var enderecoDto = _mapper.Map<EnderecoDto>(await _enderecoRepository.ObterPorId(id));

            if (enderecoDto == null) return NotFound();

            return enderecoDto;
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("atualizar-endereco/{id:guid}")]
        public async Task<ActionResult<EnderecoDto>> AtualizarEndereco(Guid id, EnderecoDto enderecoDto)
        {
            if (id != enderecoDto.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query.");
                return CustomResponse(enderecoDto);
            }                                    

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _enderecoRepository.Atualizar(_mapper.Map<Endereco>(enderecoDto));

            return CustomResponse(enderecoDto);
        }
        
        [ClaimsAuthorize("Fornecedor", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<FornecedorDto>> Adicionar(FornecedorDto fornecedorDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.Adicionar(_mapper.Map<Fornecedor>(fornecedorDto));

            return CustomResponse(fornecedorDto);
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Atualizar(Guid id, FornecedorDto fornecedorDto)
        {
            if (id != fornecedorDto.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query.");
                return CustomResponse(fornecedorDto);
            }                                    

            if (!ModelState.IsValid) return CustomResponse(ModelState);

           await _fornecedorService.Atualizar(_mapper.Map<Fornecedor>(fornecedorDto));

           return CustomResponse(fornecedorDto);
        }

        [ClaimsAuthorize("Fornecedor", "Excluir")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Excluir(Guid id)
        {
            var fornecedorDto = await ObterFornecedorEndereco(id);

            if (fornecedorDto == null) return NotFound();

            await _fornecedorService.Remover(id);

            return CustomResponse();
        }

        private async Task<FornecedorDto> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
        }

        private async Task<FornecedorDto> ObterFornecedorEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterFornecedorEndereco(id));
        }
    }
}
