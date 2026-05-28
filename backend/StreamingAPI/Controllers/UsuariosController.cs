using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StreamingAPI.DTOs;
using StreamingAPI.Models;
using StreamingAPI.Repositories;

namespace StreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioRepository _repository;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuariosController(UsuarioRepository repository, IPasswordHasher<Usuario> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetAll()
        {
            var usuarios = await _repository.GetAllAsync();
            var usuarioDTOs = usuarios.Select(MapToDTO).ToList();

            return Ok(usuarioDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDTO>> GetById(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null)
                return NotFound();

            return Ok(MapToDTO(usuario));
        }

        private static UsuarioDTO MapToDTO(Usuario u)
        {
            return new UsuarioDTO
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email
            };
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioDTO>> Create(CreateUsuarioDTO createDTO)
        {
            var email = NormalizeEmail(createDTO.Email);
            if (await _repository.EmailExistsAsync(email))
            {
                return Conflict(new { message = "E-mail ja cadastrado." });
            }

            var usuario = new Usuario
            {
                Nome = createDTO.Nome.Trim(),
                Email = email,
                SenhaHash = string.Empty
            };

            usuario.SenhaHash = _passwordHasher.HashPassword(usuario, createDTO.Senha);
            await _repository.CreateAsync(usuario);

            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, MapToDTO(usuario));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUsuarioDTO updateDTO)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null)
                return NotFound();

            var email = NormalizeEmail(updateDTO.Email);
            if (await _repository.EmailExistsAsync(email, id))
            {
                return Conflict(new { message = "E-mail ja cadastrado." });
            }

            usuario.Nome = updateDTO.Nome.Trim();
            usuario.Email = email;

            await _repository.UpdateAsync(usuario);

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

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
    }
}
