using System;
using System.IO;
using AutoMapper;
using DevIo.Api.Dtos;
using DevIO.Api.Extensions;
using DevIO.Business.Models;
using DevIo.Api.Controllers;
using System.Threading.Tasks;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace DevIo.Api.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/produtos")]
    public class ProdutosController : MainController
    {
        private readonly IMapper _mapper;
        private readonly IProdutoService _produtoService;
        private readonly IProdutoRepository _produtoRepository;

        public ProdutosController(INotificador notificador, IMapper mapper,
            IProdutoService produtoService, IProdutoRepository produtoRepository, IUser user) : base(notificador, user)
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

        [ClaimsAuthorize("Produto", "Adicionar")]
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

        [ClaimsAuthorize("Produto", "Adicionar")]
        [RequestSizeLimit(40000000)]
        [HttpPost("AdicionarArquivoGrande")]
        public async Task<ActionResult<ProdutoDto>> AdicionarArquivoGrande(ProdutoImagemDto produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";

            if (!await UploadArquivoGrande(produtoDto.ImagemUpload, imgPrefixo))
            {
                return CustomResponse(produtoDto);
            }

            produtoDto.Imagem = imgPrefixo + produtoDto.ImagemUpload.FileName;

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [ClaimsAuthorize("Produto", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> Atualizar(Guid id, ProdutoDto produtoDto)
        {
            if (id != produtoDto.Id)
            {
                NotificarErro("O id informado n??o ?? o mesmo que foi passado na query.");
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

        [ClaimsAuthorize("Produto", "Excluir")]
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
                // ModelState.AddModelError(string.Empty, "Forne??a uma imagem para este produto!");
                NotificarErro("Forne??a uma imagem para este produto!");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("J?? existe um arquivo com esse nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }

        private async Task<bool> UploadArquivoGrande(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forne??a uma imagem para este produto!");
                return false;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                NotificarErro("J?? existe um arquivo com esse nome!");
                return false;
            }

            await using var stream = new FileStream(path, FileMode.Create);
            await arquivo.CopyToAsync(stream);

            return true;
        }
    }
}