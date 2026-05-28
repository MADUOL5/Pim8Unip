namespace StreamingAPI.Docs
{
    public static class ApiDocsHtml
    {
        public static string SwaggerUi() => """
<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>StreamingAPI - Swagger</title>
  <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css">
  <style>
    body { margin: 0; background: #f6f8fb; }
    .topbar { display: none; }
    .doc-header {
      padding: 20px 32px;
      color: #152238;
      background: #ffffff;
      border-bottom: 1px solid #dbe3ef;
      font-family: Arial, sans-serif;
    }
    .doc-header h1 { margin: 0 0 6px; font-size: 24px; }
    .doc-header p { margin: 0; color: #526174; }
    .doc-header a { color: #1167b1; font-weight: 700; }
    #swagger-ui { max-width: 1400px; margin: 0 auto; }
  </style>
</head>
<body>
  <header class="doc-header">
    <h1>StreamingAPI - Swagger</h1>
    <p>Use <strong>Authorize</strong> com <code>Bearer &lt;token&gt;</code>. Fluxo completo e exemplos para o frontend: <a href="/docs/frontend">/docs/frontend</a>.</p>
  </header>
  <main id="swagger-ui"></main>

  <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
  <script>
    async function loadSwagger() {
      const response = await fetch('/swagger/v1/swagger.json');
      const spec = await response.json();

      spec.info = {
        title: 'StreamingAPI',
        version: 'v1',
        description: 'API de streaming com usuarios, autenticacao JWT, criadores, conteudos e playlists.'
      };

      spec.components = spec.components || {};
      spec.components.securitySchemes = spec.components.securitySchemes || {};
      spec.components.securitySchemes.BearerAuth = {
        type: 'http',
        scheme: 'bearer',
        bearerFormat: 'JWT',
        description: 'Cole o token recebido em /api/auth/login ou /api/auth/cadastro. Exemplo: Bearer eyJ...'
      };

      for (const [path, methods] of Object.entries(spec.paths || {})) {
        if (path.startsWith('/api/auth')) continue;
        if (path.endsWith('/stream')) continue;

        for (const operation of Object.values(methods)) {
          if (operation && typeof operation === 'object') {
            operation.security = [{ BearerAuth: [] }];
          }
        }
      }

      SwaggerUIBundle({
        spec,
        dom_id: '#swagger-ui',
        deepLinking: true,
        displayRequestDuration: true,
        persistAuthorization: true,
        tryItOutEnabled: true,
        docExpansion: 'list'
      });
    }

    loadSwagger();
  </script>
</body>
</html>
""";

        public static string FrontendGuide() => """
<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>StreamingAPI - Guia do Frontend</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #f5f7fb;
      --panel: #ffffff;
      --text: #152238;
      --muted: #5b6878;
      --line: #dce3ee;
      --accent: #1468a8;
      --code: #101828;
    }

    * { box-sizing: border-box; }
    body {
      margin: 0;
      color: var(--text);
      background: var(--bg);
      font: 15px/1.55 Arial, sans-serif;
    }

    header {
      padding: 28px 32px;
      background: var(--panel);
      border-bottom: 1px solid var(--line);
    }

    header h1 {
      margin: 0 0 8px;
      font-size: clamp(24px, 4vw, 36px);
      line-height: 1.1;
      letter-spacing: 0;
    }

    header p { margin: 0; color: var(--muted); max-width: 920px; }
    main { width: min(1120px, calc(100% - 32px)); margin: 24px auto 48px; }
    section {
      margin: 18px 0;
      padding: 22px;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 8px;
    }

    h2 { margin: 0 0 14px; font-size: 22px; letter-spacing: 0; }
    h3 { margin: 20px 0 8px; font-size: 17px; letter-spacing: 0; }
    p { margin: 8px 0; }
    a { color: var(--accent); font-weight: 700; }
    code, pre {
      color: var(--code);
      background: #edf2f8;
      border: 1px solid #d7e0eb;
      border-radius: 6px;
    }
    code { padding: 2px 6px; }
    pre {
      overflow: auto;
      padding: 14px;
      white-space: pre;
    }
    table { width: 100%; border-collapse: collapse; margin: 12px 0; }
    th, td { padding: 10px 8px; border-bottom: 1px solid var(--line); text-align: left; vertical-align: top; }
    th { color: #344256; font-size: 13px; text-transform: uppercase; }
    .pill {
      display: inline-block;
      min-width: 62px;
      padding: 3px 8px;
      border-radius: 999px;
      color: #ffffff;
      background: #2b7a3d;
      font-size: 12px;
      font-weight: 700;
      text-align: center;
    }
    .post { background: #1468a8; }
    .put { background: #8a5a00; }
    .delete { background: #b42318; }
    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); gap: 14px; }
  </style>
</head>
<body>
  <header>
    <h1>StreamingAPI - Guia para o Frontend</h1>
    <p>Contrato pratico da API: fluxo de autenticacao, headers, formatos de request/response e exemplos de chamada. Swagger interativo em <a href="/swagger">/swagger</a>; OpenAPI JSON em <a href="/swagger/v1/swagger.json">/swagger/v1/swagger.json</a>.</p>
  </header>

  <main>
    <section>
      <h2>Base e autenticacao</h2>
      <div class="grid">
        <div>
          <h3>Base URL local</h3>
          <pre><code>http://localhost:&lt;porta&gt;</code></pre>
          <p>A porta vem do terminal quando rodar <code>dotnet run</code>.</p>
        </div>
        <div>
          <h3>Header nas rotas protegidas</h3>
          <pre><code>Authorization: Bearer &lt;token&gt;
Content-Type: application/json</code></pre>
          <p>Somente <code>/api/auth/cadastro</code> e <code>/api/auth/login</code> nao precisam de token.</p>
        </div>
      </div>
    </section>

    <section>
      <h2>Fluxo recomendado no frontend</h2>
      <pre><code>1. Fazer cadastro ou login.
2. Salvar response.token no estado seguro da aplicacao.
3. Enviar Authorization: Bearer token em toda chamada /api/*.
4. Se receber 401, limpar sessao local e mandar usuario para login.</code></pre>

      <h3>Helper fetch sugerido</h3>
      <pre><code>const API_URL = 'http://localhost:&lt;porta&gt;';

async function apiFetch(path, options = {}) {
  const token = localStorage.getItem('token');

  const headers = {
    ...(options.body instanceof FormData ? {} : { 'Content-Type': 'application/json' }),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers
  };

  const response = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (response.status === 401) {
    localStorage.removeItem('token');
    throw new Error('Sessao expirada ou token invalido.');
  }

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || `Erro HTTP ${response.status}`);
  }

  if (response.status === 204) return null;
  return response.json();
}</code></pre>
    </section>

    <section>
      <h2>Autenticacao</h2>
      <table>
        <thead><tr><th>Metodo</th><th>Rota</th><th>Body</th><th>Resposta</th></tr></thead>
        <tbody>
          <tr>
            <td><span class="pill post">POST</span></td>
            <td><code>/api/auth/cadastro</code></td>
            <td><code>{ nome, email, senha }</code></td>
            <td><code>{ usuario, token, expiraEm }</code></td>
          </tr>
          <tr>
            <td><span class="pill post">POST</span></td>
            <td><code>/api/auth/login</code></td>
            <td><code>{ email, senha }</code></td>
            <td><code>{ usuario, token, expiraEm }</code></td>
          </tr>
        </tbody>
      </table>

      <h3>Cadastro</h3>
      <pre><code>await apiFetch('/api/auth/cadastro', {
  method: 'POST',
  body: JSON.stringify({
    nome: 'Maria Silva',
    email: 'maria@email.com',
    senha: 'Senha123'
  })
});</code></pre>

      <h3>Resposta de autenticacao</h3>
      <pre><code>{
  "usuario": {
    "id": 1,
    "nome": "Maria Silva",
    "email": "maria@email.com"
  },
  "token": "eyJhbGciOi...",
  "expiraEm": "2026-05-28T02:30:00Z"
}</code></pre>
    </section>

    <section>
      <h2>Usuarios</h2>
      <table>
        <thead><tr><th>Metodo</th><th>Rota</th><th>Uso</th></tr></thead>
        <tbody>
          <tr><td><span class="pill">GET</span></td><td><code>/api/usuarios</code></td><td>Lista usuarios.</td></tr>
          <tr><td><span class="pill">GET</span></td><td><code>/api/usuarios/{id}</code></td><td>Busca usuario por id.</td></tr>
          <tr><td><span class="pill post">POST</span></td><td><code>/api/usuarios</code></td><td>Cria usuario protegido por token. Body: <code>{ nome, email, senha }</code>.</td></tr>
          <tr><td><span class="pill put">PUT</span></td><td><code>/api/usuarios/{id}</code></td><td>Atualiza nome/e-mail. Body: <code>{ nome, email }</code>.</td></tr>
          <tr><td><span class="pill delete">DELETE</span></td><td><code>/api/usuarios/{id}</code></td><td>Remove usuario.</td></tr>
        </tbody>
      </table>
    </section>

    <section>
      <h2>Criadores</h2>
      <table>
        <thead><tr><th>Metodo</th><th>Rota</th><th>Uso</th></tr></thead>
        <tbody>
          <tr><td><span class="pill">GET</span></td><td><code>/api/criadores</code></td><td>Lista criadores.</td></tr>
          <tr><td><span class="pill">GET</span></td><td><code>/api/criadores/{id}</code></td><td>Busca criador por id.</td></tr>
          <tr><td><span class="pill post">POST</span></td><td><code>/api/criadores</code></td><td>Cria criador. Body: <code>{ nome }</code>.</td></tr>
          <tr><td><span class="pill put">PUT</span></td><td><code>/api/criadores/{id}</code></td><td>Atualiza criador. Body: <code>{ nome }</code>.</td></tr>
          <tr><td><span class="pill delete">DELETE</span></td><td><code>/api/criadores/{id}</code></td><td>Remove criador.</td></tr>
        </tbody>
      </table>
    </section>

    <section>
      <h2>Conteudos</h2>
      <table>
        <thead><tr><th>Metodo</th><th>Rota</th><th>Uso</th></tr></thead>
        <tbody>
          <tr><td><span class="pill">GET</span></td><td><code>/api/conteudos</code></td><td>Lista conteudos.</td></tr>
          <tr><td><span class="pill">GET</span></td><td><code>/api/conteudos/{id}</code></td><td>Busca conteudo por id.</td></tr>
          <tr><td><span class="pill">GET</span></td><td><code>/api/conteudos/{id}/reproducao</code></td><td>Valida se existe video pronto para o player.</td></tr>
          <tr><td><span class="pill">HEAD</span></td><td><code>/api/conteudos/{id}/stream</code></td><td>Valida o stream sem baixar o video.</td></tr>
          <tr><td><span class="pill">GET</span></td><td><code>/api/conteudos/{id}/stream</code></td><td>Entrega video com suporte a range para <code>&lt;video&gt;</code>.</td></tr>
          <tr><td><span class="pill post">POST</span></td><td><code>/api/conteudos</code></td><td>Cria conteudo via <code>multipart/form-data</code>.</td></tr>
          <tr><td><span class="pill put">PUT</span></td><td><code>/api/conteudos/{id}</code></td><td>Atualiza conteudo via JSON.</td></tr>
          <tr><td><span class="pill delete">DELETE</span></td><td><code>/api/conteudos/{id}</code></td><td>Remove conteudo.</td></tr>
        </tbody>
      </table>

      <h3>Criar conteudo com upload</h3>
      <pre><code>const form = new FormData();
form.append('titulo', 'Aula 01');
form.append('tipo', 'video');
form.append('criadorId', '1');
form.append('arquivo', fileInput.files[0]); // opcional

await apiFetch('/api/conteudos', {
  method: 'POST',
  body: form
});</code></pre>

      <h3>Atualizar conteudo</h3>
      <pre><code>await apiFetch('/api/conteudos/1', {
  method: 'PUT',
  body: JSON.stringify({
    titulo: 'Aula 01 atualizada',
    tipo: 'video',
    criadorId: 1,
    arquivoMidia: 'arquivo.mp4'
  })
});</code></pre>
      <p>Quando houver arquivo salvo, o frontend acessa por <code>/Media/{arquivoMidia}</code>.</p>

      <h3>Validar e abrir player</h3>
      <pre><code>const reproducao = await apiFetch(`/api/conteudos/${conteudoId}/reproducao`);

if (reproducao.disponivelParaReproducao) {
  videoRef.current.src = reproducao.urlStream;
  await videoRef.current.play();
} else {
  console.warn(reproducao.mensagem);
}</code></pre>
      <p>Use <code>urlStream</code> no <code>src</code> da tag <code>&lt;video&gt;</code>. Esse endpoint e publico para funcionar no player nativo do navegador.</p>
    </section>

    <section>
      <h2>Playlists</h2>
      <table>
        <thead><tr><th>Metodo</th><th>Rota</th><th>Uso</th></tr></thead>
        <tbody>
          <tr><td><span class="pill">GET</span></td><td><code>/api/playlists</code></td><td>Lista playlists com itens.</td></tr>
          <tr><td><span class="pill">GET</span></td><td><code>/api/playlists/{id}</code></td><td>Busca playlist por id.</td></tr>
          <tr><td><span class="pill">GET</span></td><td><code>/api/playlists/usuario/{usuarioId}</code></td><td>Lista playlists de um usuario.</td></tr>
          <tr><td><span class="pill post">POST</span></td><td><code>/api/playlists</code></td><td>Cria playlist. Body: <code>{ nome, usuarioId }</code>.</td></tr>
          <tr><td><span class="pill put">PUT</span></td><td><code>/api/playlists/{id}</code></td><td>Atualiza nome da playlist. Body: <code>{ nome }</code>.</td></tr>
          <tr><td><span class="pill delete">DELETE</span></td><td><code>/api/playlists/{id}</code></td><td>Remove playlist.</td></tr>
          <tr><td><span class="pill post">POST</span></td><td><code>/api/playlists/{playlistId}/itens</code></td><td>Adiciona conteudo. Body: <code>{ conteudoId }</code>.</td></tr>
          <tr><td><span class="pill delete">DELETE</span></td><td><code>/api/playlists/itens/{itemId}</code></td><td>Remove item da playlist.</td></tr>
        </tbody>
      </table>

      <h3>Criar playlist e adicionar item</h3>
      <pre><code>const playlist = await apiFetch('/api/playlists', {
  method: 'POST',
  body: JSON.stringify({ nome: 'Favoritos', usuarioId: 1 })
});

await apiFetch(`/api/playlists/${playlist.id}/itens`, {
  method: 'POST',
  body: JSON.stringify({ conteudoId: 3 })
});</code></pre>
    </section>

    <section>
      <h2>Tipos principais para TypeScript</h2>
      <pre><code>type Usuario = {
  id: number;
  nome: string;
  email: string;
};

type AuthResponse = {
  usuario: Usuario;
  token: string;
  expiraEm: string;
};

type Criador = {
  id: number;
  nome: string;
};

type Conteudo = {
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

type ReproducaoConteudo = {
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

type ItemPlaylist = {
  id: number;
  playlistId: number;
  conteudoId: number;
  conteudo?: Conteudo | null;
};

type Playlist = {
  id: number;
  nome: string;
  usuarioId: number;
  items: ItemPlaylist[];
};</code></pre>
    </section>

    <section>
      <h2>Status comuns</h2>
      <table>
        <thead><tr><th>Status</th><th>Quando acontece</th></tr></thead>
        <tbody>
          <tr><td><code>200</code></td><td>Consulta ou login executado com sucesso.</td></tr>
          <tr><td><code>201</code></td><td>Cadastro/criacao executado com sucesso.</td></tr>
          <tr><td><code>204</code></td><td>Atualizacao ou exclusao sem corpo de resposta.</td></tr>
          <tr><td><code>400</code></td><td>Dados invalidos ou relacao inexistente, como criador/conteudo nao encontrado.</td></tr>
          <tr><td><code>401</code></td><td>Token ausente, invalido ou expirado.</td></tr>
          <tr><td><code>404</code></td><td>Registro nao encontrado.</td></tr>
          <tr><td><code>409</code></td><td>E-mail ja cadastrado.</td></tr>
        </tbody>
      </table>
    </section>
  </main>
</body>
</html>
""";
    }
}
