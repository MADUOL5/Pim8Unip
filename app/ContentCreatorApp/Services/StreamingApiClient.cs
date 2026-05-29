using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ContentCreatorApp.Models;

namespace ContentCreatorApp.Services;

public sealed class StreamingApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<AuthResponse> CadastrarAsync(string baseUrl, AuthCadastroRequest request) =>
        SendAsync<AuthResponse>(baseUrl, "/api/auth/cadastro", HttpMethod.Post, null, request);

    public Task<AuthResponse> LoginAsync(string baseUrl, AuthLoginRequest request) =>
        SendAsync<AuthResponse>(baseUrl, "/api/auth/login", HttpMethod.Post, null, request);

    public Task<List<Usuario>> ListarUsuariosAsync(string baseUrl, string token) =>
        SendAsync<List<Usuario>>(baseUrl, "/api/usuarios", HttpMethod.Get, token);

    public Task<Usuario> CriarUsuarioAsync(string baseUrl, string token, UsuarioCreateRequest request) =>
        SendAsync<Usuario>(baseUrl, "/api/usuarios", HttpMethod.Post, token, request);

    public Task AtualizarUsuarioAsync(string baseUrl, string token, int id, UsuarioUpdateRequest request) =>
        SendNoContentAsync(baseUrl, $"/api/usuarios/{id}", HttpMethod.Put, token, request);

    public Task RemoverUsuarioAsync(string baseUrl, string token, int id) =>
        SendNoContentAsync(baseUrl, $"/api/usuarios/{id}", HttpMethod.Delete, token);

    public Task<List<Criador>> ListarCriadoresAsync(string baseUrl, string token) =>
        SendAsync<List<Criador>>(baseUrl, "/api/criadores", HttpMethod.Get, token);

    public Task<Criador> CriarCriadorAsync(string baseUrl, string token, CriadorRequest request) =>
        SendAsync<Criador>(baseUrl, "/api/criadores", HttpMethod.Post, token, request);

    public Task AtualizarCriadorAsync(string baseUrl, string token, int id, CriadorRequest request) =>
        SendNoContentAsync(baseUrl, $"/api/criadores/{id}", HttpMethod.Put, token, request);

    public Task RemoverCriadorAsync(string baseUrl, string token, int id) =>
        SendNoContentAsync(baseUrl, $"/api/criadores/{id}", HttpMethod.Delete, token);

    public Task<List<Conteudo>> ListarConteudosAsync(string baseUrl, string token) =>
        SendAsync<List<Conteudo>>(baseUrl, "/api/conteudos", HttpMethod.Get, token);

    public Task<ReproducaoConteudo> ObterReproducaoAsync(string baseUrl, string token, int id) =>
        SendAsync<ReproducaoConteudo>(baseUrl, $"/api/conteudos/{id}/reproducao", HttpMethod.Get, token);

    public async Task<Conteudo> CriarConteudoAsync(
        string baseUrl,
        string token,
        ConteudoCreateRequest request,
        MediaUpload? arquivo)
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent(request.Titulo), "titulo" },
            { new StringContent(request.Tipo), "tipo" },
            { new StringContent(request.CriadorId.ToString()), "criadorId" }
        };

        if (arquivo is not null)
        {
            var fileContent = new StreamContent(await arquivo.OpenReadAsync());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(arquivo.ContentType) ? "application/octet-stream" : arquivo.ContentType);
            content.Add(fileContent, "arquivo", arquivo.FileName);
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, BuildUri(baseUrl, "/api/conteudos"))
        {
            Content = content
        };
        AddBearerToken(message, token);

        using var response = await httpClient.SendAsync(message);
        return await ReadResponseAsync<Conteudo>(response);
    }

    public Task AtualizarConteudoAsync(string baseUrl, string token, int id, ConteudoUpdateRequest request) =>
        SendNoContentAsync(baseUrl, $"/api/conteudos/{id}", HttpMethod.Put, token, request);

    public Task RemoverConteudoAsync(string baseUrl, string token, int id) =>
        SendNoContentAsync(baseUrl, $"/api/conteudos/{id}", HttpMethod.Delete, token);

    public Task<List<Playlist>> ListarPlaylistsAsync(string baseUrl, string token) =>
        SendAsync<List<Playlist>>(baseUrl, "/api/playlists", HttpMethod.Get, token);

    public Task<Playlist> CriarPlaylistAsync(string baseUrl, string token, PlaylistRequest request) =>
        SendAsync<Playlist>(baseUrl, "/api/playlists", HttpMethod.Post, token, request);

    public Task AtualizarPlaylistAsync(string baseUrl, string token, int id, PlaylistRequest request) =>
        SendNoContentAsync(baseUrl, $"/api/playlists/{id}", HttpMethod.Put, token, request);

    public Task RemoverPlaylistAsync(string baseUrl, string token, int id) =>
        SendNoContentAsync(baseUrl, $"/api/playlists/{id}", HttpMethod.Delete, token);

    public Task<ItemPlaylist> AdicionarItemPlaylistAsync(
        string baseUrl,
        string token,
        int playlistId,
        AddItemPlaylistRequest request) =>
        SendAsync<ItemPlaylist>(baseUrl, $"/api/playlists/{playlistId}/itens", HttpMethod.Post, token, request);

    public Task RemoverItemPlaylistAsync(string baseUrl, string token, int itemId) =>
        SendNoContentAsync(baseUrl, $"/api/playlists/itens/{itemId}", HttpMethod.Delete, token);

    private async Task<T> SendAsync<T>(
        string baseUrl,
        string path,
        HttpMethod method,
        string? token,
        object? body = null)
    {
        using var message = new HttpRequestMessage(method, BuildUri(baseUrl, path));
        if (body is not null)
        {
            message.Content = JsonContent.Create(body, options: JsonOptions);
        }

        AddBearerToken(message, token);
        using var response = await httpClient.SendAsync(message);
        return await ReadResponseAsync<T>(response);
    }

    private async Task SendNoContentAsync(
        string baseUrl,
        string path,
        HttpMethod method,
        string? token,
        object? body = null)
    {
        using var message = new HttpRequestMessage(method, BuildUri(baseUrl, path));
        if (body is not null)
        {
            message.Content = JsonContent.Create(body, options: JsonOptions);
        }

        AddBearerToken(message, token);
        using var response = await httpClient.SendAsync(message);
        await EnsureSuccessAsync(response);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return result ?? throw new StreamingApiException("A API retornou uma resposta vazia.", 0);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        var message = string.IsNullOrWhiteSpace(body)
            ? $"Erro HTTP {(int)response.StatusCode}."
            : body;

        throw new StreamingApiException(message, (int)response.StatusCode);
    }

    private static void AddBearerToken(HttpRequestMessage message, string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static Uri BuildUri(string baseUrl, string path)
    {
        var cleanBase = string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:5065" : baseUrl.Trim().TrimEnd('/');
        return new Uri($"{cleanBase}{path}");
    }
}

public sealed class StreamingApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class MediaUpload(string fileName, string contentType, Func<Task<Stream>> openReadAsync)
{
    public string FileName { get; } = fileName;

    public string ContentType { get; } = contentType;

    public Task<Stream> OpenReadAsync() => openReadAsync();
}
