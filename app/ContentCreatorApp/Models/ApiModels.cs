namespace ContentCreatorApp.Models;

public sealed class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class AuthResponse
{
    public Usuario Usuario { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiraEm { get; set; }
}

public sealed class Criador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public sealed class Conteudo
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int CriadorId { get; set; }
    public string? ArquivoMidia { get; set; }
    public string? UrlMidia { get; set; }
    public string? UrlReproducao { get; set; }
    public bool DisponivelParaReproducao { get; set; }
    public Criador? Criador { get; set; }
}

public sealed class ReproducaoConteudo
{
    public int ConteudoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? UrlStream { get; set; }
    public string? ContentType { get; set; }
    public long TamanhoBytes { get; set; }
    public bool ArquivoExiste { get; set; }
    public bool EhVideo { get; set; }
    public bool DisponivelParaReproducao { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}

public sealed class ItemPlaylist
{
    public int Id { get; set; }
    public int PlaylistId { get; set; }
    public int ConteudoId { get; set; }
    public Conteudo? Conteudo { get; set; }
}

public sealed class Playlist
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public List<ItemPlaylist> Items { get; set; } = [];
}

public sealed class AuthCadastroRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public sealed class AuthLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public sealed class UsuarioCreateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public sealed class UsuarioUpdateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class CriadorRequest
{
    public string Nome { get; set; } = string.Empty;
}

public sealed class ConteudoCreateRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = "video";
    public int CriadorId { get; set; }
}

public sealed class ConteudoUpdateRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = "video";
    public int CriadorId { get; set; }
    public string? ArquivoMidia { get; set; }
}

public sealed class PlaylistRequest
{
    public string Nome { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
}

public sealed class AddItemPlaylistRequest
{
    public int ConteudoId { get; set; }
}
