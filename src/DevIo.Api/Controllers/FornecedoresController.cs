using System;
using AutoMapper;
using DevIo.Api.Dtos;
using DevIO.Business.Models;
using System.Threading.Tasks;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DevIo.Api.Controllers
{
    [Route("api/[controller]")]
    public class FornecedoresController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IFornecedorService _fornecedorService;
        private readonly IFornecedorRepository _fornecedorRepository;    
        
        public FornecedoresController(IFornecedorRepository fornecedorRepository, IMapper mapper, IFornecedorService fornecedorService)
        {
            _mapper = mapper;
            _fornecedorService = fornecedorService;
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

        [HttpPost]
        public async Task<ActionResult<FornecedorDto>> Adicionar(FornecedorDto fornecedorDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorDto);

            var result = await _fornecedorService.Adicionar(fornecedor);

            if (!result) return BadRequest();

            var retorno = _mapper.Map<FornecedorDto>(fornecedor);
            
            return Ok(retorno);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Atualizar(Guid id, FornecedorDto fornecedorDto)
        {
            if (id != fornecedorDto.Id) return BadRequest();                                    

            if (!ModelState.IsValid) return BadRequest();

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorDto);

            var result = await _fornecedorService.Atualizar(fornecedor);

            if (!result) return BadRequest();

            var retorno = _mapper.Map<FornecedorDto>(fornecedor);
            
            return Ok(retorno);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Excluir(Guid id)
        {
            var fornecedor = await ObterFornecedorEndereco(id);

            if (fornecedor == null) return NotFound();

            var result = await _fornecedorService.Remover(id);

            if (!result) return BadRequest();

            return Ok(fornecedor);
        }

        public async Task<FornecedorDto> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
        }

        public async Task<FornecedorDto> ObterFornecedorEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterFornecedorEndereco(id));
        }
    }
}
