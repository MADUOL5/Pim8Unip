# StreamingAPI

Backend ASP.NET Core para uma aplicacao de streaming com usuarios, autenticacao JWT, criadores, conteudos, playlists e itens de playlist.

Este README foi escrito para ser entregue a outro desenvolvedor, ao frontend ou a outra IA. Ele resume o que existe, como rodar, como autenticar e como consumir a API.

## Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite local
- JWT com `System.IdentityModel.Tokens.Jwt`
- Senha com hash usando `PasswordHasher<Usuario>`
- OpenAPI nativo do ASP.NET Core
- Swagger UI customizado em HTML

## Como rodar

```bash
dotnet restore
dotnet run
```

Ao subir, o terminal mostra a porta, por exemplo:

```txt
Now listening on: http://localhost:5065
```

Use essa URL como base no frontend.

## URLs importantes

Se a API subir em `http://localhost:5065`:

- Swagger UI: `http://localhost:5065/swagger`
- Guia web para frontend: `http://localhost:5065/docs/frontend`
- OpenAPI JSON: `http://localhost:5065/swagger/v1/swagger.json`
- Midias salvas: `http://localhost:5065/Media/{arquivoMidia}`
- Player/stream de video: `http://localhost:5065/api/conteudos/{id}/stream`

## Banco de dados

O banco local e o arquivo:

```txt
Streaming.db
```

As migrations sao aplicadas automaticamente no startup:

```csharp
context.Database.Migrate();
```

## Autenticacao

As rotas publicas sao:

- `POST /api/auth/cadastro`
- `POST /api/auth/login`
- `/swagger`
- `/docs/frontend`
- `/swagger/v1/swagger.json`
- `/Media/*`
- `GET /api/conteudos/{id}/stream`
- `HEAD /api/conteudos/{id}/stream`

As demais rotas `api/*` precisam de token JWT.

Header obrigatorio nas rotas protegidas:

```http
Authorization: Bearer <token>
Content-Type: application/json
```

O token e retornado no cadastro e no login.

## Fluxo recomendado para o frontend

1. Usuario faz cadastro ou login.
2. Frontend salva `token`.
3. Toda chamada para `/api/*`, exceto auth, envia `Authorization: Bearer <token>`.
4. Se a API responder `401`, limpar sessao local e redirecionar para login.

Exemplo:

```ts
const API_URL = "http://localhost:5065";

async function apiFetch(path: string, options: RequestInit = {}) {
  const token = localStorage.getItem("token");
  const isFormData = options.body instanceof FormData;

  const headers = {
    ...(isFormData ? {} : { "Content-Type": "application/json" }),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    localStorage.removeItem("token");
    throw new Error("Sessao expirada ou token invalido.");
  }

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || `Erro HTTP ${response.status}`);
  }

  if (response.status === 204) return null;
  return response.json();
}
```

## Endpoints

### Auth

#### Cadastro

```http
POST /api/auth/cadastro
```

Body:

```json
{
  "nome": "Maria Silva",
  "email": "maria@email.com",
  "senha": "Senha123"
}
```

Resposta `201`:

```json
{
  "usuario": {
    "id": 1,
    "nome": "Maria Silva",
    "email": "maria@email.com"
  },
  "token": "eyJhbGciOi...",
  "expiraEm": "2026-05-28T02:30:00Z"
}
```

#### Login

```http
POST /api/auth/login
```

Body:

```json
{
  "email": "maria@email.com",
  "senha": "Senha123"
}
```

Resposta `200`:

```json
{
  "usuario": {
    "id": 1,
    "nome": "Maria Silva",
    "email": "maria@email.com"
  },
  "token": "eyJhbGciOi...",
  "expiraEm": "2026-05-28T02:30:00Z"
}
```

### Usuarios

Todas protegidas por token.

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | `/api/usuarios` | Lista usuarios |
| GET | `/api/usuarios/{id}` | Busca usuario por id |
| POST | `/api/usuarios` | Cria usuario |
| PUT | `/api/usuarios/{id}` | Atualiza nome/e-mail |
| DELETE | `/api/usuarios/{id}` | Remove usuario |

Criar usuario:

```json
{
  "nome": "Maria Silva",
  "email": "maria@email.com",
  "senha": "Senha123"
}
```

Atualizar usuario:

```json
{
  "nome": "Maria Souza",
  "email": "maria@email.com"
}
```

Resposta de usuario:

```json
{
  "id": 1,
  "nome": "Maria Souza",
  "email": "maria@email.com"
}
```

### Criadores

Todas protegidas por token.

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | `/api/criadores` | Lista criadores |
| GET | `/api/criadores/{id}` | Busca criador por id |
| POST | `/api/criadores` | Cria criador |
| PUT | `/api/criadores/{id}` | Atualiza criador |
| DELETE | `/api/criadores/{id}` | Remove criador |

Body para criar/atualizar:

```json
{
  "nome": "Studio Exemplo"
}
```

Resposta:

```json
{
  "id": 1,
  "nome": "Studio Exemplo"
}
```

### Conteudos

Todas protegidas por token.

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | `/api/conteudos` | Lista conteudos |
| GET | `/api/conteudos/{id}` | Busca conteudo por id |
| GET | `/api/conteudos/{id}/reproducao` | Valida se o conteudo tem video pronto para player |
| HEAD | `/api/conteudos/{id}/stream` | Valida o stream sem baixar o arquivo |
| GET | `/api/conteudos/{id}/stream` | Entrega o video com suporte a range para `<video>` |
| POST | `/api/conteudos` | Cria conteudo, aceita upload |
| PUT | `/api/conteudos/{id}` | Atualiza conteudo |
| DELETE | `/api/conteudos/{id}` | Remove conteudo |

Criar conteudo usa `multipart/form-data`.

Campos:

- `titulo`: string
- `tipo`: string, exemplo `video`, `musica`, `podcast`
- `criadorId`: number
- `arquivoMidia`: string opcional
- `arquivo`: arquivo opcional

Exemplo frontend:

```ts
const form = new FormData();
form.append("titulo", "Aula 01");
form.append("tipo", "video");
form.append("criadorId", "1");
form.append("arquivo", fileInput.files[0]);

await apiFetch("/api/conteudos", {
  method: "POST",
  body: form,
});
```

Atualizar conteudo usa JSON:

```json
{
  "titulo": "Aula 01 atualizada",
  "tipo": "video",
  "criadorId": 1,
  "arquivoMidia": "arquivo.mp4"
}
```

Resposta:

```json
{
  "id": 1,
  "titulo": "Aula 01",
  "tipo": "video",
  "criadorId": 1,
  "arquivoMidia": "nome-do-arquivo.mp4",
  "urlMidia": "http://localhost:5065/Media/nome-do-arquivo.mp4",
  "urlReproducao": "http://localhost:5065/api/conteudos/1/stream",
  "disponivelParaReproducao": true,
  "criador": {
    "id": 1,
    "nome": "Studio Exemplo"
  }
}
```

Para exibir a midia:

```txt
http://localhost:5065/Media/nome-do-arquivo.mp4
```

#### Player de video

Antes de abrir o player, o frontend deve chamar:

```http
GET /api/conteudos/{id}/reproducao
Authorization: Bearer <token>
```

Resposta quando o video esta pronto:

```json
{
  "conteudoId": 1,
  "titulo": "Aula 01",
  "tipo": "video",
  "arquivoMidia": "nome-do-arquivo.mp4",
  "urlMidia": "http://localhost:5065/Media/nome-do-arquivo.mp4",
  "urlStream": "http://localhost:5065/api/conteudos/1/stream",
  "contentType": "video/mp4",
  "tamanhoBytes": 12345678,
  "arquivoExiste": true,
  "ehVideo": true,
  "disponivelParaReproducao": true,
  "mensagem": "Video disponivel para reproducao."
}
```

Exemplo no frontend:

```ts
const reproducao = await apiFetch(`/api/conteudos/${conteudoId}/reproducao`);

if (reproducao.disponivelParaReproducao) {
  videoElement.src = reproducao.urlStream;
  await videoElement.play();
} else {
  console.warn(reproducao.mensagem);
}
```

O endpoint `GET /api/conteudos/{id}/stream` e publico de proposito, porque a tag `<video src="...">` nao envia `Authorization`. Ele usa o id do conteudo, valida se o arquivo existe e se e video, e responde com suporte a `Range`, que e necessario para o navegador avançar/voltar no player.

### Playlists

Todas protegidas por token.

| Metodo | Rota | Descricao |
| --- | --- | --- |
| GET | `/api/playlists` | Lista playlists com itens |
| GET | `/api/playlists/{id}` | Busca playlist por id |
| GET | `/api/playlists/usuario/{usuarioId}` | Lista playlists de um usuario |
| POST | `/api/playlists` | Cria playlist |
| PUT | `/api/playlists/{id}` | Atualiza nome da playlist |
| DELETE | `/api/playlists/{id}` | Remove playlist |
| POST | `/api/playlists/{playlistId}/itens` | Adiciona conteudo na playlist |
| DELETE | `/api/playlists/itens/{itemId}` | Remove item da playlist |

Criar playlist:

```json
{
  "nome": "Favoritos",
  "usuarioId": 1
}
```

Atualizar playlist:

```json
{
  "nome": "Favoritos atualizados",
  "usuarioId": 1
}
```

Adicionar item:

```json
{
  "conteudoId": 3
}
```

Resposta de playlist:

```json
{
  "id": 1,
  "nome": "Favoritos",
  "usuarioId": 1,
  "items": [
    {
      "id": 1,
      "playlistId": 1,
      "conteudoId": 3,
      "conteudo": {
        "id": 3,
        "titulo": "Aula 01",
        "tipo": "video",
        "criadorId": 1,
        "arquivoMidia": "arquivo.mp4",
        "criador": null
      }
    }
  ]
}
```

## Tipos TypeScript sugeridos

```ts
export type Usuario = {
  id: number;
  nome: string;
  email: string;
};

export type AuthResponse = {
  usuario: Usuario;
  token: string;
  expiraEm: string;
};

export type Criador = {
  id: number;
  nome: string;
};

export type Conteudo = {
  id: number;
  titulo: string;
  tipo: string;
  criadorId: number;
  arquivoMidia?: string | null;
  urlMidia?: string | null;
  urlReproducao?: string | null;
  disponivelParaReproducao: boolean;
  criador?: Criador | null;
};

export type ReproducaoConteudo = {
  conteudoId: number;
  titulo: string;
  tipo: string;
  arquivoMidia?: string | null;
  urlMidia?: string | null;
  urlStream?: string | null;
  contentType?: string | null;
  tamanhoBytes?: number | null;
  arquivoExiste: boolean;
  ehVideo: boolean;
  disponivelParaReproducao: boolean;
  mensagem: string;
};

export type ItemPlaylist = {
  id: number;
  playlistId: number;
  conteudoId: number;
  conteudo?: Conteudo | null;
};

export type Playlist = {
  id: number;
  nome: string;
  usuarioId: number;
  items: ItemPlaylist[];
};
```

## Status HTTP esperados

| Status | Significado |
| --- | --- |
| 200 | Consulta, login ou operacao com resposta feita com sucesso |
| 201 | Cadastro/criacao feita com sucesso |
| 204 | Atualizacao ou exclusao feita sem corpo de resposta |
| 400 | Dados invalidos ou relacionamento inexistente |
| 401 | Token ausente, invalido ou expirado |
| 404 | Registro nao encontrado |
| 409 | E-mail ja cadastrado |

## Estrutura atual do backend

```txt
StreamingAPI/
  Controllers/
    AuthController.cs
    UsuariosController.cs
    CriadoresController.cs
    ConteudosController.cs
    PlaylistsController.cs
  Data/
    StreamingContext.cs
  DTOs/
    UsuarioDTO.cs
    CriadorDTO.cs
    ConteudoDTO.cs
    PlaylistDTO.cs
    ItemPlaylistDTO.cs
  Docs/
    ApiDocsHtml.cs
    AI_HANDOFF.md
    FRONTEND_CONTRACT.md
  Middlewares/
    TokenValidationMiddleware.cs
  Migrations/
  Models/
  Repositories/
  Services/
    TokenService.cs
  Program.cs
  Streaming.db
```

## Observacoes importantes para outra IA

- Nao retornar `SenhaHash` em nenhum endpoint.
- Nao salvar senha pura no banco.
- `POST /api/conteudos` recebe `multipart/form-data`, nao JSON.
- Antes de abrir o player, chamar `GET /api/conteudos/{id}/reproducao`.
- Para o `<video>`, usar `urlStream` retornada pela API.
- Quando usar `FormData`, nao setar manualmente `Content-Type`; o browser adiciona o boundary.
- O middleware protege rotas `/api/*`, exceto `/api/auth/*`.
- A chave JWT atual esta em `appsettings.json` e e somente para desenvolvimento.
- Se mudar DTO/modelo, criar migration EF.
- O Swagger interativo esta em `/swagger`.
- O contrato para frontend esta tambem em `Docs/FRONTEND_CONTRACT.md`.
