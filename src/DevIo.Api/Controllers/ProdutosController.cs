using System;
using System.IO;
using AutoMapper;
using DevIo.Api.Dtos;
using DevIO.Business.Models;
using System.Threading.Tasks;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DevIo.Api.Controllers
{
    [Route("api/produtos")]
    public class ProdutosController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IProdutoService _produtoService;
        private readonly IProdutoRepository _produtoRepository;

        public ProdutosController(INotificador notificador, IMapper mapper,
            IProdutoService produtoService, IProdutoRepository produtoRepository) : base(notificador)
        {
            _mapper = mapper;
            _produtoService = produtoService;
            _produtoRepository = produtoRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoDto>> ObterTodos()
        {
            var produto = _mapper.Map<IEnumerable<ProdutoDto>>(await _produtoRepository.ObterTodos());

            return produto;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> ObterPorId(Guid id)
        {
            var produtoDto = _mapper.Map<ProdutoDto>(await _produtoRepository.ObterPorId(id));

            return produtoDto == null ? NotFound() : produtoDto;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> Adicionar(ProdutoDto produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + produtoDto.Imagem;

            if (!UploadArquivo(produtoDto.ImagemUpload, imagemNome))
            {
                return CustomResponse(produtoDto);
            }

            produtoDto.Imagem = imagemNome;
            
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }
        
        [RequestSizeLimit(40000000)]
        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoDto>> AdicionarArquivoGrande(ProdutoImagemDto produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";

            if (! await UploadArquivoGrande(produtoDto.ImagemUpload, imgPrefixo))
            {
                return CustomResponse(produtoDto);
            }

            produtoDto.Imagem = imgPrefixo + produtoDto.ImagemUpload.FileName;
            
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> Atualizar(Guid id, ProdutoDto produtoDto)
        {
            if (id != produtoDto.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query.");
                return CustomResponse(produtoDto);
            }

            var produtoAtualizacao = _mapper.Map<Produto>(_produtoRepository.ObterPorId(id));

            produtoDto.Imagem = produtoAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoDto.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + produtoDto.Imagem;
                if (!UploadArquivo(produtoDto.ImagemUpload, imagemNome))
                {
                    return CustomResponse(produtoDto);
                }

                produtoAtualizacao.Imagem = imagemNome;
            }

            produtoAtualizacao.Nome = produtoDto.Nome;
            produtoAtualizacao.Descricao = produtoDto.Descricao;
            produtoAtualizacao.Valor = produtoDto.Valor;
            produtoAtualizacao.Ativo = produtoDto.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> Excluir(Guid id)
        {
            var produtoDto = _mapper.Map<ProdutoDto>(await _produtoRepository.ObterPorId(id));

            if (produtoDto == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse();
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            var imageDataByteArray = Convert.FromBase64String(arquivo);

            if (string.IsNullOrEmpty(arquivo))
            {
                // ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto!");
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com esse nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);
            
            return true;
        }

        private async Task<bool> UploadArquivoGrande(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }
            
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                NotificarErro("Já existe um arquivo com esse nome!");
                return false;
            }

            await using var stream = new FileStream(path, FileMode.Create);
            await arquivo.CopyToAsync(stream);

            return true;
        }
    }
}