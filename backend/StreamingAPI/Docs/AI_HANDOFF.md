# Handoff para outra IA

Este documento resume o estado atual do projeto para outra IA continuar o trabalho sem precisar inferir a arquitetura do zero.

## Objetivo do projeto

Criar uma API backend para um app de streaming. O frontend deve conseguir:

- cadastrar usuario;
- fazer login;
- guardar token;
- listar/criar/editar/remover usuarios;
- listar/criar/editar/remover criadores;
- listar/criar/editar/remover conteudos;
- fazer upload de arquivo de midia;
- listar/criar/editar/remover playlists;
- adicionar/remover conteudos em playlists.

## Estado atual

Implementado:

- CRUD de usuarios.
- Cadastro e login com JWT.
- Senha armazenada como hash.
- Middleware de validacao de token.
- CRUD de criadores.
- CRUD de conteudos com upload opcional.
- Validacao de reproducao em `GET /api/conteudos/{id}/reproducao`.
- Stream de video em `GET /api/conteudos/{id}/stream` com suporte a range.
- CRUD de playlists.
- Adicao/remocao de itens em playlist.
- SQLite com migrations.
- Swagger UI em `/swagger`.
- Guia frontend em `/docs/frontend`.
- OpenAPI JSON em `/swagger/v1/swagger.json`.

## Stack e convencoes

- Projeto .NET 10.
- Entity Framework Core.
- SQLite local em `Streaming.db`.
- Controllers REST em `Controllers/`.
- DTOs em `DTOs/`.
- Repositories em `Repositories/`.
- Models em `Models/`.
- Servicos em `Services/`.
- Middleware em `Middlewares/`.

## Arquivos relevantes

```txt
Program.cs
Controllers/AuthController.cs
Controllers/UsuariosController.cs
Controllers/CriadoresController.cs
Controllers/ConteudosController.cs
Controllers/PlaylistsController.cs
DTOs/UsuarioDTO.cs
DTOs/CriadorDTO.cs
DTOs/ConteudoDTO.cs
DTOs/PlaylistDTO.cs
DTOs/ItemPlaylistDTO.cs
Services/TokenService.cs
Middlewares/TokenValidationMiddleware.cs
Docs/ApiDocsHtml.cs
Docs/FRONTEND_CONTRACT.md
```

## Regras de seguranca

- Nunca retornar `SenhaHash`.
- Nunca salvar senha pura.
- Usar `Authorization: Bearer <token>` nas rotas protegidas.
- Rotas `/api/auth/*` sao publicas.
- Rotas `/api/*` fora de auth sao protegidas pelo middleware.
- `GET /api/conteudos/{id}/stream` e `HEAD /api/conteudos/{id}/stream` sao publicas de proposito para funcionar no `src` da tag `<video>`.
- Trocar `Jwt:Key` em producao.

## Como rodar e validar

```bash
dotnet build
dotnet run
```

Abrir:

```txt
http://localhost:<porta>/swagger
http://localhost:<porta>/docs/frontend
http://localhost:<porta>/swagger/v1/swagger.json
```

Validar fluxo:

1. `POST /api/auth/cadastro`.
2. Copiar `token`.
3. Chamar `GET /api/usuarios` com `Authorization: Bearer <token>`.
4. Chamar `GET /api/usuarios` sem token deve retornar `401`.

## Contrato para frontend

O contrato completo esta em:

```txt
Docs/FRONTEND_CONTRACT.md
```

Resumo:

- Login/cadastro retornam `{ usuario, token, expiraEm }`.
- O frontend deve guardar o token.
- `Content-Type: application/json` para JSON.
- `multipart/form-data` para upload de conteudo.
- Com `FormData`, o frontend nao deve setar `Content-Type` manualmente.
- Midias ficam acessiveis em `/Media/{arquivoMidia}`.
- Para player, frontend deve chamar `/api/conteudos/{id}/reproducao` e usar `urlStream` no `<video>`.

## Melhorias futuras sugeridas

- Adicionar refresh token.
- Restringir playlists por usuario logado.
- Criar roles/permissoes.
- Adicionar paginacao e filtros em conteudos.
- Adicionar testes automatizados.
- Remover providers de banco nao usados se o projeto ficar somente em SQLite.
- Mover `Jwt:Key` para variavel de ambiente em producao.

## Prompt sugerido para outra IA

```txt
Voce vai continuar um backend ASP.NET Core chamado StreamingAPI.
Leia README.md e Docs/FRONTEND_CONTRACT.md antes de alterar codigo.
Mantenha o contrato da API para o frontend.
Nao retorne SenhaHash em respostas.
Use JWT Bearer em rotas protegidas.
Se alterar modelos/DTOs, atualize migrations e documentacao.
O Swagger fica em /swagger, o OpenAPI em /swagger/v1/swagger.json e o guia frontend em /docs/frontend.
```
