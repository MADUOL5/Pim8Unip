# Contrato da API para o Frontend

Documento curto para orientar a implementacao do frontend.

## Base URL

Usar a URL exibida pelo `dotnet run`.

Exemplo:

```txt
http://localhost:5065
```

## Rotas de documentacao

- Swagger UI: `/swagger`
- Guia web: `/docs/frontend`
- OpenAPI JSON: `/swagger/v1/swagger.json`

## Autenticacao

Rotas publicas:

```txt
POST /api/auth/cadastro
POST /api/auth/login
```

Todas as outras rotas `/api/*` precisam de:

```http
Authorization: Bearer <token>
```

## Cadastro

```http
POST /api/auth/cadastro
Content-Type: application/json
```

```json
{
  "nome": "Maria Silva",
  "email": "maria@email.com",
  "senha": "Senha123"
}
```

Resposta:

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

## Login

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "maria@email.com",
  "senha": "Senha123"
}
```

Resposta igual ao cadastro.

## Helper de requisicao

```ts
const API_URL = "http://localhost:5065";

export async function apiFetch(path: string, options: RequestInit = {}) {
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
    throw new Error("Sessao expirada.");
  }

  if (!response.ok) {
    throw new Error(await response.text());
  }

  if (response.status === 204) return null;
  return response.json();
}
```

## Endpoints principais

| Recurso | Metodo | Rota | Body |
| --- | --- | --- | --- |
| Auth | POST | `/api/auth/cadastro` | `{ nome, email, senha }` |
| Auth | POST | `/api/auth/login` | `{ email, senha }` |
| Usuarios | GET | `/api/usuarios` | nenhum |
| Usuarios | GET | `/api/usuarios/{id}` | nenhum |
| Usuarios | POST | `/api/usuarios` | `{ nome, email, senha }` |
| Usuarios | PUT | `/api/usuarios/{id}` | `{ nome, email }` |
| Usuarios | DELETE | `/api/usuarios/{id}` | nenhum |
| Criadores | GET | `/api/criadores` | nenhum |
| Criadores | GET | `/api/criadores/{id}` | nenhum |
| Criadores | POST | `/api/criadores` | `{ nome }` |
| Criadores | PUT | `/api/criadores/{id}` | `{ nome }` |
| Criadores | DELETE | `/api/criadores/{id}` | nenhum |
| Conteudos | GET | `/api/conteudos` | nenhum |
| Conteudos | GET | `/api/conteudos/{id}` | nenhum |
| Conteudos | GET | `/api/conteudos/{id}/reproducao` | nenhum |
| Conteudos | HEAD | `/api/conteudos/{id}/stream` | nenhum |
| Conteudos | GET | `/api/conteudos/{id}/stream` | nenhum |
| Conteudos | POST | `/api/conteudos` | `multipart/form-data` |
| Conteudos | PUT | `/api/conteudos/{id}` | `{ titulo, tipo, criadorId, arquivoMidia }` |
| Conteudos | DELETE | `/api/conteudos/{id}` | nenhum |
| Playlists | GET | `/api/playlists` | nenhum |
| Playlists | GET | `/api/playlists/{id}` | nenhum |
| Playlists | GET | `/api/playlists/usuario/{usuarioId}` | nenhum |
| Playlists | POST | `/api/playlists` | `{ nome, usuarioId }` |
| Playlists | PUT | `/api/playlists/{id}` | `{ nome, usuarioId }` |
| Playlists | DELETE | `/api/playlists/{id}` | nenhum |
| Playlists | POST | `/api/playlists/{playlistId}/itens` | `{ conteudoId }` |
| Playlists | DELETE | `/api/playlists/itens/{itemId}` | nenhum |

## Upload de conteudo

```ts
const form = new FormData();
form.append("titulo", "Aula 01");
form.append("tipo", "video");
form.append("criadorId", "1");
form.append("arquivo", file);

await apiFetch("/api/conteudos", {
  method: "POST",
  body: form,
});
```

Importante: com `FormData`, nao definir `Content-Type`.

## Player de video

Para abrir o player, primeiro validar se o conteudo possui video disponivel:

```ts
const reproducao = await apiFetch(`/api/conteudos/${conteudoId}/reproducao`);

if (!reproducao.disponivelParaReproducao) {
  throw new Error(reproducao.mensagem);
}

videoRef.current.src = reproducao.urlStream;
await videoRef.current.play();
```

Resposta de `/api/conteudos/{id}/reproducao`:

```json
{
  "conteudoId": 1,
  "titulo": "Aula 01",
  "tipo": "video",
  "arquivoMidia": "arquivo.mp4",
  "urlMidia": "http://localhost:5065/Media/arquivo.mp4",
  "urlStream": "http://localhost:5065/api/conteudos/1/stream",
  "contentType": "video/mp4",
  "tamanhoBytes": 12345678,
  "arquivoExiste": true,
  "ehVideo": true,
  "disponivelParaReproducao": true,
  "mensagem": "Video disponivel para reproducao."
}
```

O endpoint `GET /api/conteudos/{id}/stream` e publico para funcionar direto no atributo `src` da tag `<video>`. Ele valida se o arquivo existe, se e video e entrega o arquivo com suporte a `Range`, permitindo pausar, avancar e voltar.

Tambem existe `HEAD /api/conteudos/{id}/stream` para validar o stream sem baixar o video.

## Tipos TypeScript

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

## Estrutura frontend sugerida

```txt
src/
  api/
    client.ts
    auth.ts
    usuarios.ts
    criadores.ts
    conteudos.ts
    playlists.ts
  types/
    api.ts
  pages/
    Login.tsx
    Cadastro.tsx
    Conteudos.tsx
    Playlists.tsx
  components/
    ProtectedRoute.tsx
    MediaCard.tsx
```

## Tratamento de erros

- `401`: limpar token e voltar para login.
- `409`: e-mail ja cadastrado.
- `404`: mostrar mensagem de registro nao encontrado.
- `400`: validar dados do formulario.
