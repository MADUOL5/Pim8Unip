using Microsoft.AspNetCore.Mvc;
using StreamingAPI.Data;
using StreamingAPI.DTOs;
using StreamingAPI.Models;
using Microsoft.EntityFrameworkCore;
using StreamingAPI.Repositories;

namespace StreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistsController : ControllerBase
    {
        private readonly PlaylistRepository _repository;

        public PlaylistsController(PlaylistRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlaylistDTO>>> GetAll()
        {
            var playlists = await _repository.GetAllAsync();

            var playlistDTOs = playlists.Select(MapToDTO).ToList();
            return Ok(playlistDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlaylistDTO>> GetById(int id)
        {
            var playlist = await _repository.GetByIdAsync(id);

            if (playlist == null)
                return NotFound();

            return Ok(MapToDTO(playlist));
        }

        private static PlaylistDTO MapToDTO(Playlist p)
        {
            return new PlaylistDTO
            {
                Id = p.Id,
                Nome = p.Nome ?? string.Empty,
                UsuarioId = p.UsuarioId,
                Items = p.Items?.Select(ip => new ItemPlaylistDTO
                {
                    Id = ip.Id,
                    PlaylistId = ip.PlaylistId,
                    ConteudoId = ip.ConteudoId,
                    Conteudo = ip.Conteudo != null ? new ConteudoDTO
                    {
                        Id = ip.Conteudo.Id,
                        Titulo = ip.Conteudo.Titulo ?? string.Empty,
                        Tipo = ip.Conteudo.Tipo ?? string.Empty,
                        CriadorId = ip.Conteudo.CriadorId
                    } : null
                }).ToList() ?? new List<ItemPlaylistDTO>()
            };
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<PlaylistDTO>>> GetByUsuarioId(int usuarioId)
        {
            var playlists = await _repository.GetByUsuarioIdAsync(usuarioId);

            var playlistDTOs = playlists.Select(MapToDTO).ToList();
            return Ok(playlistDTOs);
        }

        [HttpPost]
        public async Task<ActionResult<PlaylistDTO>> Create(CreatePlaylistDTO createDTO)
        {
            var playlist = new Playlist
            {
                Nome = createDTO.Nome,
                UsuarioId = createDTO.UsuarioId
            };

            await _repository.CreateAsync(playlist);

            return CreatedAtAction(nameof(GetById), new { id = playlist.Id }, MapToDTO(playlist));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreatePlaylistDTO updateDTO)
        {
            var playlist = await _repository.GetByIdAsync(id);
            if (playlist == null)
                return NotFound();

            playlist.Nome = updateDTO.Nome;

            await _repository.UpdateAsync(playlist);

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

        [HttpPost("{playlistId}/itens")]
        public async Task<ActionResult> AddItem(int playlistId, AddItemPlaylistDTO addItemDTO)
        { 
            var added = await _repository.AddItemAsync(playlistId, addItemDTO.ConteudoId);
            if (!added)
                return BadRequest("Playlist ou Conteúdo não encontrado.");
            return Ok(new { message = "Item adicionado à playlist" });
        }

        [HttpDelete("itens/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            await _repository.RemoveItemAsync(itemId);
            return NoContent();
        }
    }
}
