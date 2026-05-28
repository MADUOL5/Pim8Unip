using System.ComponentModel.DataAnnotations;

namespace StreamingAPI.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
    }

    public class CreateUsuarioDTO
    {
        [Required]
        public required string Nome { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Senha { get; set; }
    }

    public class UpdateUsuarioDTO
    {
        [Required]
        public required string Nome { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class LoginUsuarioDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Senha { get; set; }
    }

    public class AuthResponseDTO
    {
        public required UsuarioDTO Usuario { get; set; }
        public required string Token { get; set; }
        public DateTime ExpiraEm { get; set; }
    }
}
