using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StreamingAPI.DTOs;
using StreamingAPI.Models;
using StreamingAPI.Repositories;
using StreamingAPI.Services;

namespace StreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioRepository _repository;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly TokenService _tokenService;

        public AuthController(
            UsuarioRepository repository,
            IPasswordHasher<Usuario> passwordHasher,
            TokenService tokenService)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        [HttpPost("cadastro")]
        public async Task<ActionResult<AuthResponseDTO>> Cadastro(CreateUsuarioDTO createDTO)
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

            return StatusCode(StatusCodes.Status201Created, BuildAuthResponse(usuario));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginUsuarioDTO loginDTO)
        {
            var usuario = await _repository.GetByEmailAsync(loginDTO.Email);
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.SenhaHash))
            {
                return Unauthorized(new { message = "E-mail ou senha invalidos." });
            }

            var resultadoSenha = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.SenhaHash,
                loginDTO.Senha);

            if (resultadoSenha == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "E-mail ou senha invalidos." });
            }

            if (resultadoSenha == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.SenhaHash = _passwordHasher.HashPassword(usuario, loginDTO.Senha);
                await _repository.UpdateAsync(usuario);
            }

            return Ok(BuildAuthResponse(usuario));
        }

        private AuthResponseDTO BuildAuthResponse(Usuario usuario)
        {
            var expiraEm = _tokenService.GetExpiration();
            return new AuthResponseDTO
            {
                Usuario = MapToDTO(usuario),
                Token = _tokenService.GenerateToken(usuario, expiraEm),
                ExpiraEm = expiraEm
            };
        }

        private static UsuarioDTO MapToDTO(Usuario usuario)
        {
            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
    }
}
