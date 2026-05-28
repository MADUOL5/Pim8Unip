using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamingAPI.Data;
using StreamingAPI.DTOs;
using StreamingAPI.Models;
using Microsoft.EntityFrameworkCore;
using StreamingAPI.Repositories;
using Microsoft.AspNetCore.StaticFiles;

namespace StreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConteudosController : ControllerBase
    {
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = CreateContentTypeProvider();
        private readonly ConteudoRepository _repository;
        private readonly CriadorRepository _criadorRepository;

        public ConteudosController(ConteudoRepository repository, CriadorRepository criadorRepository)
        {
            _repository = repository;
            _criadorRepository = criadorRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConteudoDTO>>> GetAll()
        {
            var conteudos = await _repository.GetAllAsync();
            var conteudoDTOs = conteudos.Select(MapToDTO).ToList();

            return Ok(conteudoDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConteudoDTO>> GetById(int id)
        {
            var conteudo = await _repository.GetByIdAsync(id);
            if (conteudo == null)
                return NotFound();

            return Ok(MapToDTO(conteudo));
        }

        [HttpGet("{id}/reproducao")]
        public async Task<ActionResult<ReproducaoConteudoDTO>> GetReproducao(int id)
        {
            var conteudo = await _repository.GetByIdAsync(id);
            if (conteudo == null)
                return NotFound();

            return Ok(BuildReproducaoDTO(conteudo));
        }

        [AllowAnonymous]
        [HttpHead("{id}/stream")]
        public async Task<IActionResult> ValidateStream(int id)
        {
            var conteudo = await _repository.GetByIdAsync(id);
            if (conteudo == null)
                return NotFound();

            var media = GetMediaInfo(conteudo);
            if (!media.ArquivoExiste)
                return NotFound(new { message = "Arquivo de midia nao encontrado." });

            if (!media.EhVideo)
                return BadRequest(new { message = "O conteudo possui arquivo, mas ele nao foi identificado como video." });

            Response.ContentType = media.ContentType!;
            Response.ContentLength = media.TamanhoBytes;
            Response.Headers.AcceptRanges = "bytes";

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("{id}/stream")]
        public async Task<IActionResult> Stream(int id)
        {
            var conteudo = await _repository.GetByIdAsync(id);
            if (conteudo == null)
                return NotFound();

            var media = GetMediaInfo(conteudo);
            if (!media.ArquivoExiste)
                return NotFound(new { message = "Arquivo de midia nao encontrado." });

            if (!media.EhVideo)
                return BadRequest(new { message = "O conteudo possui arquivo, mas ele nao foi identificado como video." });

            return PhysicalFile(media.FilePath!, media.ContentType!, enableRangeProcessing: true);
        }

        private ConteudoDTO MapToDTO(Conteudo c)
        {
            var media = GetMediaInfo(c);

            return new ConteudoDTO
            {
                Id = c.Id,
                Titulo = c.Titulo,
                Tipo = c.Tipo,
                CriadorId = c.CriadorId,
                ArquivoMidia = c.ArquivoMidia,
                UrlMidia = media.UrlMidia,
                UrlReproducao = media.UrlStream,
                DisponivelParaReproducao = media.DisponivelParaReproducao,
                Criador = c.Criador != null ? new CriadorDTO { Id = c.Criador.Id, Nome = c.Criador.Nome } : null
            };
        }

        [HttpPost]
        public async Task<ActionResult<ConteudoDTO>> Create([FromForm] CreateConteudoDTO createDTO, IFormFile? arquivo)
        {
            var criador = await _criadorRepository.GetByIdAsync(createDTO.CriadorId);
            if (criador == null)
                return BadRequest("Criador não encontrado");

            string? nomeArquivoFinal = createDTO.ArquivoMidia;

            // Se um arquivo real for enviado, salvamos na pasta Media
            if (arquivo != null && arquivo.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Gera um nome único para o arquivo
                nomeArquivoFinal = Guid.NewGuid().ToString() + Path.GetExtension(arquivo.FileName);
                var filePath = Path.Combine(folderPath, nomeArquivoFinal);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await arquivo.CopyToAsync(stream);
                }
            }

            var conteudo = new Conteudo
            {
                Titulo = createDTO.Titulo,
                Tipo = createDTO.Tipo,
                CriadorId = createDTO.CriadorId,
                ArquivoMidia = nomeArquivoFinal
            };

            await _repository.CreateAsync(conteudo);
            conteudo.Criador = criador; // Atribui para o DTO ter os dados do criador

            return CreatedAtAction(nameof(GetById), new { id = conteudo.Id }, MapToDTO(conteudo));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateConteudoDTO updateDTO)
        {
            var conteudo = await _repository.GetByIdAsync(id);
            if (conteudo == null)
                return NotFound();

            conteudo.Titulo = updateDTO.Titulo;
            conteudo.Tipo = updateDTO.Tipo;
            conteudo.CriadorId = updateDTO.CriadorId;
            conteudo.ArquivoMidia = updateDTO.ArquivoMidia;

            await _repository.UpdateAsync(conteudo);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        private ReproducaoConteudoDTO BuildReproducaoDTO(Conteudo conteudo)
        {
            var media = GetMediaInfo(conteudo);

            return new ReproducaoConteudoDTO
            {
                ConteudoId = conteudo.Id,
                Titulo = conteudo.Titulo,
                Tipo = conteudo.Tipo,
                ArquivoMidia = conteudo.ArquivoMidia,
                UrlMidia = media.UrlMidia,
                UrlStream = media.UrlStream,
                ContentType = media.ContentType,
                TamanhoBytes = media.TamanhoBytes,
                ArquivoExiste = media.ArquivoExiste,
                EhVideo = media.EhVideo,
                DisponivelParaReproducao = media.DisponivelParaReproducao,
                Mensagem = media.Mensagem
            };
        }

        private MediaInfo GetMediaInfo(Conteudo conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo.ArquivoMidia))
            {
                return new MediaInfo
                {
                    Mensagem = "Conteudo sem arquivo de midia vinculado."
                };
            }

            var nomeArquivo = Path.GetFileName(conteudo.ArquivoMidia);
            var mediaPath = Path.Combine(Directory.GetCurrentDirectory(), "Media");
            var filePath = Path.Combine(mediaPath, nomeArquivo);

            ContentTypeProvider.TryGetContentType(nomeArquivo, out var contentType);
            contentType ??= "application/octet-stream";

            var fileInfo = new FileInfo(filePath);
            var arquivoExiste = fileInfo.Exists;
            var ehVideo = contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
                || conteudo.Tipo.Equals("video", StringComparison.OrdinalIgnoreCase);
            var disponivel = arquivoExiste && ehVideo;

            return new MediaInfo
            {
                FilePath = arquivoExiste ? filePath : null,
                ContentType = contentType,
                TamanhoBytes = arquivoExiste ? fileInfo.Length : null,
                ArquivoExiste = arquivoExiste,
                EhVideo = ehVideo,
                DisponivelParaReproducao = disponivel,
                UrlMidia = arquivoExiste ? BuildAbsoluteUrl($"/Media/{Uri.EscapeDataString(nomeArquivo)}") : null,
                UrlStream = disponivel ? BuildAbsoluteUrl($"/api/conteudos/{conteudo.Id}/stream") : null,
                Mensagem = disponivel
                    ? "Video disponivel para reproducao."
                    : arquivoExiste
                        ? "Arquivo encontrado, mas nao foi identificado como video."
                        : "Arquivo de midia nao encontrado na pasta Media."
            };
        }

        private string BuildAbsoluteUrl(string path)
        {
            return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{path}";
        }

        private static FileExtensionContentTypeProvider CreateContentTypeProvider()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".mp4"] = "video/mp4";
            provider.Mappings[".webm"] = "video/webm";
            provider.Mappings[".ogv"] = "video/ogg";
            provider.Mappings[".mov"] = "video/quicktime";
            provider.Mappings[".m4v"] = "video/x-m4v";
            provider.Mappings[".avi"] = "video/x-msvideo";
            provider.Mappings[".mkv"] = "video/x-matroska";
            provider.Mappings[".wmv"] = "video/x-ms-wmv";

            return provider;
        }

        private class MediaInfo
        {
            public string? FilePath { get; set; }
            public string? ContentType { get; set; }
            public long? TamanhoBytes { get; set; }
            public bool ArquivoExiste { get; set; }
            public bool EhVideo { get; set; }
            public bool DisponivelParaReproducao { get; set; }
            public string? UrlMidia { get; set; }
            public string? UrlStream { get; set; }
            public required string Mensagem { get; set; }
        }
    }
}
