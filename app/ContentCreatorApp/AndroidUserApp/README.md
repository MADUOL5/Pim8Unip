# Paqueta Streaming User App

Protótipo Android em Java para o usuário final do PIM VIII.

## O que atende

- Cadastro e login usando `POST /api/auth/cadastro` e `POST /api/auth/login`.
- Consumo autenticado de `usuarios`, `conteudos` e `playlists`.
- Catálogo com busca e filtros por vídeo, música e podcast.
- Player mobile com `VideoView` e `MediaController`.
- Área de playlists do usuário logado.
- Área de comunidade listando usuários da plataforma.
- Interação social local com curtidas e comentários por conteúdo.

## URL da API

Para usar `localhost` no Android, primeiro encaminhe a porta com ADB:

```powershell
adb reverse tcp:5065 tcp:5065
```

Depois use:

```txt
http://localhost:5065
```

No navegador, teste com `/swagger`:

```txt
http://localhost:5065/swagger
```

A raiz `http://localhost:5065/` pode responder 404 porque a API nao tem pagina inicial.

## Abrir

Abra a pasta `AndroidUserApp` no Android Studio e rode o módulo `app`.
