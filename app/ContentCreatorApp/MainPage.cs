using ContentCreatorApp.Models;
using ContentCreatorApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace ContentCreatorApp;

public sealed class MainPage : ContentPage
{
    private const string ApiBaseUrlKey = "paqueta.creator.apiBaseUrl";
    private static readonly Color PageBackground = Color.FromArgb("#101010");
    private static readonly Color CardBackground = Color.FromArgb("#191919");
    private static readonly Color CardBorder = Color.FromArgb("#363636");
    private static readonly Color PrimaryRed = Color.FromArgb("#b10f1f");
    private static readonly Color Gold = Color.FromArgb("#d7b455");
    private static readonly Color MutedText = Color.FromArgb("#b8b8b8");

    private readonly StreamingApiClient _api;
    private string _apiBaseUrl = "http://localhost:5065";
    private string _activeSection = "metricas";
    private string _statusMessage = "Informe a URL da API, faca login e carregue os dados do sistema.";
    private bool _busy;
    private bool _isError;

    private AuthResponse? _session;
    private AuthCadastroRequest _cadastro = new();
    private AuthLoginRequest _login = new();

    private List<Usuario> _usuarios = [];
    private List<Criador> _criadores = [];
    private List<Conteudo> _conteudos = [];
    private List<Playlist> _playlists = [];

    private ConteudoCreateRequest _conteudoNovo = new() { Tipo = "video" };
    private ConteudoUpdateRequest _conteudoEdit = new() { Tipo = "video" };
    private int _conteudoEditId;
    private MediaUpload? _conteudoArquivo;
    private string? _conteudoArquivoNome;

    private PlaylistRequest _playlistNova = new();
    private PlaylistRequest _playlistEdit = new();
    private int _playlistEditId;
    private readonly Dictionary<int, int> _conteudoPorPlaylist = [];

    private CriadorRequest _criadorNovo = new();
    private CriadorRequest _criadorEdit = new();
    private int _criadorEditId;

    private UsuarioCreateRequest _usuarioNovo = new();
    private UsuarioUpdateRequest _usuarioEdit = new();
    private int _usuarioEditId;

    private Conteudo? _conteudoEmReproducao;
    private ReproducaoConteudo? _reproducaoAtual;

    public MainPage(StreamingApiClient api)
    {
        _api = api;
        Title = "Paqueta's Streaming Creator";
        BackgroundColor = PageBackground;
        _apiBaseUrl = Preferences.Default.Get(ApiBaseUrlKey, _apiBaseUrl);
        Render();
    }

    private void Render()
    {
        var shell = new VerticalStackLayout
        {
            Padding = new Thickness(22, 20, 22, 28),
            Spacing = 16,
            BackgroundColor = PageBackground
        };

        shell.Add(BuildHeader());
        shell.Add(BuildStatus());

        if (_session is null)
        {
            shell.Add(BuildAuth());
        }
        else
        {
            shell.Add(BuildSession());
            shell.Add(BuildMetricsSummary());
            shell.Add(BuildNavigation());
            shell.Add(BuildActiveSection());
        }

        Content = new ScrollView { Content = shell };
    }

    private View BuildHeader()
    {
        var title = new Label
        {
            Text = "PAQUETA'S STREAMING",
            TextColor = Colors.White,
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            CharacterSpacing = 1.5
        };

        var subtitle = new Label
        {
            Text = "Interface .NET MAUI para criadores de conteudo, administracao do catalogo, playlists e metricas.",
            TextColor = MutedText,
            FontSize = 14
        };

        var apiEntry = new Entry
        {
            Text = _apiBaseUrl,
            Placeholder = "http://localhost:5065",
            TextColor = Colors.White,
            PlaceholderColor = MutedText,
            BackgroundColor = Color.FromArgb("#252525")
        };
        apiEntry.TextChanged += (_, args) =>
        {
            _apiBaseUrl = args.NewTextValue ?? string.Empty;
            Preferences.Default.Set(ApiBaseUrlKey, _apiBaseUrl);
        };

        return Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label { Text = ".NET MAUI", TextColor = Gold, FontAttributes = FontAttributes.Bold },
                title,
                subtitle,
                new Label { Text = "URL da API", TextColor = Colors.White, FontAttributes = FontAttributes.Bold },
                apiEntry
            }
        });
    }

    private View BuildStatus()
    {
        return new Border
        {
            Padding = 12,
            BackgroundColor = _isError ? Color.FromArgb("#351016") : Color.FromArgb("#172316"),
            Stroke = _isError ? PrimaryRed : Color.FromArgb("#2f6f3a"),
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new Label
            {
                Text = _busy ? "Processando..." : _statusMessage,
                TextColor = Colors.White,
                FontSize = 13
            }
        };
    }

    private View BuildAuth()
    {
        var cadastroNome = BoundEntry(_cadastro.Nome, "Nome", value => _cadastro.Nome = value);
        var cadastroEmail = BoundEntry(_cadastro.Email, "E-mail", value => _cadastro.Email = value, Keyboard.Email);
        var cadastroSenha = BoundEntry(_cadastro.Senha, "Senha", value => _cadastro.Senha = value, isPassword: true);
        var cadastrar = ActionButton("Cadastrar e entrar");
        cadastrar.Clicked += async (_, _) => await RunAsync(CadastrarAsync);

        var loginEmail = BoundEntry(_login.Email, "E-mail", value => _login.Email = value, Keyboard.Email);
        var loginSenha = BoundEntry(_login.Senha, "Senha", value => _login.Senha = value, isPassword: true);
        var entrar = ActionButton("Entrar");
        entrar.Clicked += async (_, _) => await RunAsync(LoginAsync);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 14
        };

        grid.Add(Card(Form("Criar usuario", cadastroNome, cadastroEmail, cadastroSenha, cadastrar)), 0, 0);
        grid.Add(Card(Form("Login", loginEmail, loginSenha, entrar)), 1, 0);

        return grid;
    }

    private View BuildSession()
    {
        var session = _session!;
        var logout = SecondaryButton("Sair");
        logout.Clicked += (_, _) =>
        {
            _session = null;
            _usuarios = [];
            _criadores = [];
            _conteudos = [];
            _playlists = [];
            _statusMessage = "Sessao encerrada.";
            _isError = false;
            Render();
        };

        var refresh = SecondaryButton("Atualizar dados");
        refresh.Clicked += async (_, _) => await RunAsync(async () => await LoadProtectedDataAsync("Dados atualizados."));

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };

        row.Add(new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                new Label { Text = "Sessao ativa", TextColor = Gold, FontAttributes = FontAttributes.Bold },
                new Label { Text = session.Usuario.Nome, TextColor = Colors.White, FontSize = 20, FontAttributes = FontAttributes.Bold },
                new Label { Text = $"{session.Usuario.Email} | expira em {session.ExpiraEm.LocalDateTime:dd/MM/yyyy HH:mm}", TextColor = MutedText, FontSize = 12 }
            }
        }, 0, 0);
        row.Add(refresh, 1, 0);
        row.Add(logout, 2, 0);

        return Card(row);
    }

    private View BuildMetricsSummary()
    {
        var playable = _conteudos.Count(IsPlayableMedia);
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10
        };

        grid.Add(MetricCard("Usuarios", _usuarios.Count.ToString()), 0, 0);
        grid.Add(MetricCard("Criadores", _criadores.Count.ToString()), 1, 0);
        grid.Add(MetricCard("Conteudos", _conteudos.Count.ToString()), 2, 0);
        grid.Add(MetricCard("Midias prontas", playable.ToString()), 3, 0);

        return grid;
    }

    private View BuildNavigation()
    {
        var items = new[]
        {
            ("metricas", "Metricas"),
            ("player", "Player"),
            ("cine", "Cine"),
            ("conteudos", "Conteudos"),
            ("playlists", "Playlists"),
            ("criadores", "Criadores"),
            ("usuarios", "Usuarios")
        };

        var row = new HorizontalStackLayout { Spacing = 8 };
        foreach (var (key, label) in items)
        {
            var button = _activeSection == key ? ActionButton(label) : SecondaryButton(label);
            button.Clicked += (_, _) =>
            {
                _activeSection = key;
                Render();
            };
            row.Add(button);
        }

        return new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = row
        };
    }

    private View BuildActiveSection() =>
        _activeSection switch
        {
            "player" => BuildPlayer(),
            "cine" => BuildCine(),
            "conteudos" => BuildConteudos(),
            "playlists" => BuildPlaylists(),
            "criadores" => BuildCriadores(),
            "usuarios" => BuildUsuarios(),
            _ => BuildMetricas()
        };

    private View BuildMetricas()
    {
        var videos = _conteudos.Count(conteudo => NormalizeTipo(conteudo.Tipo) == "video");
        var musicas = _conteudos.Count(conteudo => NormalizeTipo(conteudo.Tipo) == "musica");
        var podcasts = _conteudos.Count(conteudo => NormalizeTipo(conteudo.Tipo) == "podcast");
        var playable = _conteudos.Count(IsPlayableMedia);
        var coverage = _conteudos.Count == 0 ? "0%" : $"{(playable * 100d / _conteudos.Count):0.#}%";
        var playlistItems = _playlists.Sum(playlist => playlist.Items.Count);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        grid.Add(MetricCard("Videos", videos.ToString()), 0, 0);
        grid.Add(MetricCard("Musicas", musicas.ToString()), 1, 0);
        grid.Add(MetricCard("Podcasts", podcasts.ToString()), 2, 0);
        grid.Add(MetricCard("Cobertura pronta", coverage), 0, 1);
        grid.Add(MetricCard("Playlists", _playlists.Count.ToString()), 1, 1);
        grid.Add(MetricCard("Itens em playlists", playlistItems.ToString()), 2, 1);

        var ranking = new VerticalStackLayout { Spacing = 8 };
        foreach (var criador in _criadores)
        {
            var total = _conteudos.Count(conteudo => conteudo.CriadorId == criador.Id);
            ranking.Add(new Label
            {
                Text = $"{criador.Nome}: {total} conteudo(s)",
                TextColor = Colors.White
            });
        }

        return Card(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                SectionTitle("Metricas do criador"),
                new Label { Text = "Painel de desempenho com volume de catalogo, prontidao de midias e distribuicao por criador.", TextColor = MutedText },
                grid,
                SectionTitle("Desempenho por criador"),
                ranking
            }
        });
    }

    private View BuildPlayer()
    {
        var source = BuildPlayerSource(_conteudoEmReproducao);
        var open = ActionButton("Abrir stream");
        open.IsEnabled = !_busy && !string.IsNullOrWhiteSpace(source);
        open.Clicked += async (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                await Browser.Default.OpenAsync(source);
            }
        };

        return Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                SectionTitle("Player"),
                new Label
                {
                    Text = _conteudoEmReproducao?.Titulo ?? "Escolha uma midia na aba Cine para preparar a reproducao.",
                    TextColor = Colors.White,
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold
                },
                new Label
                {
                    Text = _reproducaoAtual is null
                        ? "O player consulta a API e valida arquivo, tipo, tamanho e URL de streaming."
                        : $"Tipo: {MediaKindLabel(_conteudoEmReproducao)} | Tamanho: {FormatBytes(_reproducaoAtual.TamanhoBytes)} | {_reproducaoAtual.Mensagem}",
                    TextColor = MutedText
                },
                new Label { Text = source ?? "Sem URL de reproducao preparada.", TextColor = Gold, LineBreakMode = LineBreakMode.WordWrap },
                open
            }
        });
    }

    private View BuildCine()
    {
        var list = new VerticalStackLayout { Spacing = 10 };
        foreach (var conteudo in _conteudos.Where(IsSupportedMedia))
        {
            var prepare = SecondaryButton("Preparar reproducao");
            prepare.Clicked += async (_, _) => await RunAsync(async () => await PrepararReproducaoAsync(conteudo));

            list.Add(RowCard(
                conteudo.Titulo,
                $"{MediaKindLabel(conteudo)} | {CriadorNome(conteudo.CriadorId)} | {(IsPlayableMedia(conteudo) ? "pronto" : "sem midia")}",
                prepare));
        }

        if (list.Children.Count == 0)
        {
            list.Add(EmptyText("Nenhuma midia cadastrada."));
        }

        return Card(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                SectionTitle("Cine"),
                new Label { Text = "Catalogo para conferir disponibilidade e preparar a reproducao de videos, musicas e podcasts.", TextColor = MutedText },
                list
            }
        });
    }

    private View BuildConteudos()
    {
        var title = BoundEntry(_conteudoEditId > 0 ? _conteudoEdit.Titulo : _conteudoNovo.Titulo, "Titulo", value =>
        {
            if (_conteudoEditId > 0) _conteudoEdit.Titulo = value;
            else _conteudoNovo.Titulo = value;
        });

        var tipo = new Picker
        {
            Title = "Tipo",
            TextColor = Colors.White,
            TitleColor = MutedText,
            BackgroundColor = Color.FromArgb("#252525"),
            ItemsSource = new List<string> { "video", "musica", "podcast" },
            SelectedItem = _conteudoEditId > 0 ? _conteudoEdit.Tipo : _conteudoNovo.Tipo
        };
        tipo.SelectedIndexChanged += (_, _) =>
        {
            var value = tipo.SelectedItem?.ToString() ?? "video";
            if (_conteudoEditId > 0) _conteudoEdit.Tipo = value;
            else _conteudoNovo.Tipo = value;
        };

        var criadorPicker = ObjectPicker(_criadores, nameof(Criador.Nome), _conteudoEditId > 0 ? _conteudoEdit.CriadorId : _conteudoNovo.CriadorId);
        criadorPicker.SelectedIndexChanged += (_, _) =>
        {
            if (criadorPicker.SelectedItem is not Criador criador) return;
            if (_conteudoEditId > 0) _conteudoEdit.CriadorId = criador.Id;
            else _conteudoNovo.CriadorId = criador.Id;
        };

        var pickFile = SecondaryButton(_conteudoArquivoNome is null ? "Selecionar arquivo" : _conteudoArquivoNome);
        pickFile.IsEnabled = _conteudoEditId == 0 && !_busy;
        pickFile.Clicked += async (_, _) => await SelecionarArquivoAsync();

        var save = ActionButton(_conteudoEditId > 0 ? "Salvar conteudo" : "Cadastrar conteudo");
        save.Clicked += async (_, _) => await RunAsync(_conteudoEditId > 0 ? AtualizarConteudoAsync : CriarConteudoAsync);

        var clear = SecondaryButton("Limpar");
        clear.Clicked += (_, _) =>
        {
            _conteudoEditId = 0;
            _conteudoEdit = new ConteudoUpdateRequest { Tipo = "video" };
            _conteudoNovo = new ConteudoCreateRequest { Tipo = "video", CriadorId = _criadores.FirstOrDefault()?.Id ?? 0 };
            _conteudoArquivo = null;
            _conteudoArquivoNome = null;
            Render();
        };

        var list = new VerticalStackLayout { Spacing = 10 };
        foreach (var conteudo in _conteudos)
        {
            var edit = SecondaryButton("Editar");
            edit.Clicked += (_, _) =>
            {
                _conteudoEditId = conteudo.Id;
                _conteudoEdit = new ConteudoUpdateRequest
                {
                    Titulo = conteudo.Titulo,
                    Tipo = conteudo.Tipo,
                    CriadorId = conteudo.CriadorId,
                    ArquivoMidia = conteudo.ArquivoMidia
                };
                Render();
            };

            var remove = DangerButton("Excluir");
            remove.Clicked += async (_, _) => await RunAsync(async () =>
            {
                await _api.RemoverConteudoAsync(ApiBaseUrl, Token, conteudo.Id);
                await LoadProtectedDataAsync("Conteudo removido.");
            });

            list.Add(RowCard(conteudo.Titulo, $"{conteudo.Tipo} | {CriadorNome(conteudo.CriadorId)} | {conteudo.ArquivoMidia ?? "sem arquivo"}", edit, remove));
        }

        return Card(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                SectionTitle("Conteudos"),
                Form(_conteudoEditId > 0 ? $"Editando conteudo #{_conteudoEditId}" : "Novo conteudo", title, tipo, criadorPicker, pickFile, Horizontal(save, clear)),
                SectionTitle("Catalogo"),
                list
            }
        });
    }

    private View BuildPlaylists()
    {
        var nome = BoundEntry(_playlistEditId > 0 ? _playlistEdit.Nome : _playlistNova.Nome, "Nome da playlist", value =>
        {
            if (_playlistEditId > 0) _playlistEdit.Nome = value;
            else _playlistNova.Nome = value;
        });

        var usuarioPicker = ObjectPicker(_usuarios, nameof(Usuario.Nome), _playlistEditId > 0 ? _playlistEdit.UsuarioId : _playlistNova.UsuarioId);
        usuarioPicker.SelectedIndexChanged += (_, _) =>
        {
            if (usuarioPicker.SelectedItem is not Usuario usuario) return;
            if (_playlistEditId > 0) _playlistEdit.UsuarioId = usuario.Id;
            else _playlistNova.UsuarioId = usuario.Id;
        };

        var save = ActionButton(_playlistEditId > 0 ? "Salvar playlist" : "Criar playlist");
        save.Clicked += async (_, _) => await RunAsync(_playlistEditId > 0 ? AtualizarPlaylistAsync : CriarPlaylistAsync);

        var clear = SecondaryButton("Limpar");
        clear.Clicked += (_, _) =>
        {
            _playlistEditId = 0;
            _playlistEdit = new PlaylistRequest();
            _playlistNova = new PlaylistRequest { UsuarioId = _session?.Usuario.Id ?? _usuarios.FirstOrDefault()?.Id ?? 0 };
            Render();
        };

        var list = new VerticalStackLayout { Spacing = 10 };
        foreach (var playlist in _playlists)
        {
            var edit = SecondaryButton("Editar");
            edit.Clicked += (_, _) =>
            {
                _playlistEditId = playlist.Id;
                _playlistEdit = new PlaylistRequest { Nome = playlist.Nome, UsuarioId = playlist.UsuarioId };
                Render();
            };

            var remove = DangerButton("Excluir");
            remove.Clicked += async (_, _) => await RunAsync(async () =>
            {
                await _api.RemoverPlaylistAsync(ApiBaseUrl, Token, playlist.Id);
                await LoadProtectedDataAsync("Playlist removida.");
            });

            var contentPicker = ObjectPicker(_conteudos, nameof(Conteudo.Titulo), _conteudoPorPlaylist.TryGetValue(playlist.Id, out var id) ? id : 0);
            contentPicker.SelectedIndexChanged += (_, _) =>
            {
                if (contentPicker.SelectedItem is Conteudo conteudo)
                {
                    _conteudoPorPlaylist[playlist.Id] = conteudo.Id;
                }
            };

            var addItem = SecondaryButton("Adicionar item");
            addItem.Clicked += async (_, _) => await RunAsync(async () =>
            {
                var conteudoId = _conteudoPorPlaylist.TryGetValue(playlist.Id, out var selectedId) ? selectedId : 0;
                if (conteudoId == 0)
                {
                    SetError("Selecione um conteudo para adicionar na playlist.");
                    return;
                }

                await _api.AdicionarItemPlaylistAsync(ApiBaseUrl, Token, playlist.Id, new AddItemPlaylistRequest { ConteudoId = conteudoId });
                await LoadProtectedDataAsync("Item adicionado na playlist.");
            });

            var items = string.Join(", ", playlist.Items.Select(item => item.Conteudo?.Titulo ?? ConteudoTitulo(item.ConteudoId)));
            list.Add(RowCard(playlist.Nome, $"{UsuarioNome(playlist.UsuarioId)} | {playlist.Items.Count} item(ns) {items}", edit, remove, contentPicker, addItem));
        }

        return Card(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                SectionTitle("Playlists"),
                Form(_playlistEditId > 0 ? $"Editando playlist #{_playlistEditId}" : "Nova playlist", nome, usuarioPicker, Horizontal(save, clear)),
                SectionTitle("Listagem"),
                list
            }
        });
    }

    private View BuildCriadores()
    {
        var nome = BoundEntry(_criadorEditId > 0 ? _criadorEdit.Nome : _criadorNovo.Nome, "Nome do criador", value =>
        {
            if (_criadorEditId > 0) _criadorEdit.Nome = value;
            else _criadorNovo.Nome = value;
        });

        var save = ActionButton(_criadorEditId > 0 ? "Salvar criador" : "Criar criador");
        save.Clicked += async (_, _) => await RunAsync(_criadorEditId > 0 ? AtualizarCriadorAsync : CriarCriadorAsync);

        var list = new VerticalStackLayout { Spacing = 10 };
        foreach (var criador in _criadores)
        {
            var edit = SecondaryButton("Editar");
            edit.Clicked += (_, _) =>
            {
                _criadorEditId = criador.Id;
                _criadorEdit = new CriadorRequest { Nome = criador.Nome };
                Render();
            };

            var remove = DangerButton("Excluir");
            remove.Clicked += async (_, _) => await RunAsync(async () =>
            {
                await _api.RemoverCriadorAsync(ApiBaseUrl, Token, criador.Id);
                await LoadProtectedDataAsync("Criador removido.");
            });

            list.Add(RowCard(criador.Nome, $"{_conteudos.Count(conteudo => conteudo.CriadorId == criador.Id)} conteudo(s)", edit, remove));
        }

        return Card(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                SectionTitle("Criadores"),
                Form(_criadorEditId > 0 ? $"Editando criador #{_criadorEditId}" : "Novo criador", nome, save),
                SectionTitle("Listagem"),
                list
            }
        });
    }

    private View BuildUsuarios()
    {
        var nome = BoundEntry(_usuarioEditId > 0 ? _usuarioEdit.Nome : _usuarioNovo.Nome, "Nome", value =>
        {
            if (_usuarioEditId > 0) _usuarioEdit.Nome = value;
            else _usuarioNovo.Nome = value;
        });
        var email = BoundEntry(_usuarioEditId > 0 ? _usuarioEdit.Email : _usuarioNovo.Email, "E-mail", value =>
        {
            if (_usuarioEditId > 0) _usuarioEdit.Email = value;
            else _usuarioNovo.Email = value;
        }, Keyboard.Email);
        var senha = BoundEntry(_usuarioNovo.Senha, "Senha inicial", value => _usuarioNovo.Senha = value, isPassword: true);
        senha.IsVisible = _usuarioEditId == 0;

        var save = ActionButton(_usuarioEditId > 0 ? "Salvar usuario" : "Criar usuario");
        save.Clicked += async (_, _) => await RunAsync(_usuarioEditId > 0 ? AtualizarUsuarioAsync : CriarUsuarioAsync);

        var list = new VerticalStackLayout { Spacing = 10 };
        foreach (var usuario in _usuarios)
        {
            var edit = SecondaryButton("Editar");
            edit.Clicked += (_, _) =>
            {
                _usuarioEditId = usuario.Id;
                _usuarioEdit = new UsuarioUpdateRequest { Nome = usuario.Nome, Email = usuario.Email };
                Render();
            };

            var remove = DangerButton("Excluir");
            remove.Clicked += async (_, _) => await RunAsync(async () =>
            {
                await _api.RemoverUsuarioAsync(ApiBaseUrl, Token, usuario.Id);
                await LoadProtectedDataAsync("Usuario removido.");
            });

            list.Add(RowCard(usuario.Nome, usuario.Email, edit, remove));
        }

        return Card(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                SectionTitle("Usuarios"),
                Form(_usuarioEditId > 0 ? $"Editando usuario #{_usuarioEditId}" : "Novo usuario", nome, email, senha, save),
                SectionTitle("Listagem"),
                list
            }
        });
    }

    private async Task CadastrarAsync()
    {
        _session = await _api.CadastrarAsync(ApiBaseUrl, _cadastro);
        _cadastro = new();
        await LoadProtectedDataAsync("Cadastro realizado e sessao iniciada.");
    }

    private async Task LoginAsync()
    {
        _session = await _api.LoginAsync(ApiBaseUrl, _login);
        _login = new();
        await LoadProtectedDataAsync("Login realizado com sucesso.");
    }

    private async Task LoadProtectedDataAsync(string message)
    {
        _usuarios = await _api.ListarUsuariosAsync(ApiBaseUrl, Token);
        _criadores = await _api.ListarCriadoresAsync(ApiBaseUrl, Token);
        _conteudos = await _api.ListarConteudosAsync(ApiBaseUrl, Token);
        _playlists = await _api.ListarPlaylistsAsync(ApiBaseUrl, Token);

        _conteudoNovo.CriadorId = _conteudoNovo.CriadorId == 0 ? _criadores.FirstOrDefault()?.Id ?? 0 : _conteudoNovo.CriadorId;
        _playlistNova.UsuarioId = _playlistNova.UsuarioId == 0 ? _session?.Usuario.Id ?? _usuarios.FirstOrDefault()?.Id ?? 0 : _playlistNova.UsuarioId;
        SetStatus(message);
    }

    private async Task CriarUsuarioAsync()
    {
        await _api.CriarUsuarioAsync(ApiBaseUrl, Token, _usuarioNovo);
        _usuarioNovo = new();
        await LoadProtectedDataAsync("Usuario criado.");
    }

    private async Task AtualizarUsuarioAsync()
    {
        await _api.AtualizarUsuarioAsync(ApiBaseUrl, Token, _usuarioEditId, _usuarioEdit);
        _usuarioEditId = 0;
        _usuarioEdit = new();
        await LoadProtectedDataAsync("Usuario atualizado.");
    }

    private async Task CriarCriadorAsync()
    {
        await _api.CriarCriadorAsync(ApiBaseUrl, Token, _criadorNovo);
        _criadorNovo = new();
        _criadorEditId = 0;
        await LoadProtectedDataAsync("Criador criado.");
    }

    private async Task AtualizarCriadorAsync()
    {
        await _api.AtualizarCriadorAsync(ApiBaseUrl, Token, _criadorEditId, _criadorEdit);
        _criadorEditId = 0;
        _criadorEdit = new();
        await LoadProtectedDataAsync("Criador atualizado.");
    }

    private async Task CriarConteudoAsync()
    {
        if (_conteudoNovo.CriadorId == 0)
        {
            SetError("Selecione um criador antes de cadastrar o conteudo.");
            return;
        }

        await _api.CriarConteudoAsync(ApiBaseUrl, Token, _conteudoNovo, _conteudoArquivo);
        _conteudoNovo = new ConteudoCreateRequest { Tipo = "video", CriadorId = _criadores.FirstOrDefault()?.Id ?? 0 };
        _conteudoArquivo = null;
        _conteudoArquivoNome = null;
        await LoadProtectedDataAsync("Conteudo criado.");
    }

    private async Task AtualizarConteudoAsync()
    {
        await _api.AtualizarConteudoAsync(ApiBaseUrl, Token, _conteudoEditId, _conteudoEdit);
        _conteudoEditId = 0;
        _conteudoEdit = new ConteudoUpdateRequest { Tipo = "video" };
        await LoadProtectedDataAsync("Conteudo atualizado.");
    }

    private async Task CriarPlaylistAsync()
    {
        await _api.CriarPlaylistAsync(ApiBaseUrl, Token, _playlistNova);
        _playlistNova = new PlaylistRequest { UsuarioId = _session?.Usuario.Id ?? _usuarios.FirstOrDefault()?.Id ?? 0 };
        await LoadProtectedDataAsync("Playlist criada.");
    }

    private async Task AtualizarPlaylistAsync()
    {
        await _api.AtualizarPlaylistAsync(ApiBaseUrl, Token, _playlistEditId, _playlistEdit);
        _playlistEditId = 0;
        _playlistEdit = new();
        await LoadProtectedDataAsync("Playlist atualizada.");
    }

    private async Task PrepararReproducaoAsync(Conteudo conteudo)
    {
        _conteudoEmReproducao = conteudo;
        _reproducaoAtual = await _api.ObterReproducaoAsync(ApiBaseUrl, Token, conteudo.Id);
        _activeSection = "player";
        SetStatus($"{MediaKindLabel(conteudo)} preparado para reproducao.");
    }

    private async Task SelecionarArquivoAsync()
    {
        try
        {
            var arquivo = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione video, musica ou podcast"
            });

            if (arquivo is null)
            {
                return;
            }

            _conteudoArquivoNome = arquivo.FileName;
            _conteudoArquivo = new MediaUpload(
                arquivo.FileName,
                string.IsNullOrWhiteSpace(arquivo.ContentType) ? "application/octet-stream" : arquivo.ContentType,
                arquivo.OpenReadAsync);
            SetStatus($"Arquivo selecionado: {arquivo.FileName}");
            Render();
        }
        catch (Exception ex)
        {
            SetError($"Nao foi possivel selecionar o arquivo: {ex.Message}");
            Render();
        }
    }

    private async Task RunAsync(Func<Task> action)
    {
        if (_busy)
        {
            return;
        }

        _busy = true;
        Render();

        try
        {
            await action();
        }
        catch (StreamingApiException ex) when (ex.StatusCode == 401)
        {
            _session = null;
            SetError("Sessao expirada ou token invalido. Faca login novamente.");
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            _busy = false;
            Render();
        }
    }

    private Entry BoundEntry(string value, string placeholder, Action<string> update, Keyboard? keyboard = null, bool isPassword = false)
    {
        var entry = new Entry
        {
            Text = value,
            Placeholder = placeholder,
            Keyboard = keyboard ?? Keyboard.Text,
            IsPassword = isPassword,
            TextColor = Colors.White,
            PlaceholderColor = MutedText,
            BackgroundColor = Color.FromArgb("#252525")
        };
        entry.TextChanged += (_, args) => update(args.NewTextValue ?? string.Empty);
        return entry;
    }

    private Picker ObjectPicker<T>(IReadOnlyList<T> items, string displayMember, int selectedId)
    {
        var picker = new Picker
        {
            Title = "Selecione",
            ItemsSource = items.ToList(),
            ItemDisplayBinding = new Binding(displayMember),
            TextColor = Colors.White,
            TitleColor = MutedText,
            BackgroundColor = Color.FromArgb("#252525")
        };

        var selected = items.FirstOrDefault(item =>
            item?.GetType().GetProperty("Id")?.GetValue(item) is int id && id == selectedId);
        if (selected is not null)
        {
            picker.SelectedItem = selected;
        }

        return picker;
    }

    private View Form(string title, params View[] children)
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                new Label { Text = title, TextColor = Gold, FontAttributes = FontAttributes.Bold, FontSize = 16 }
            }
        };

        foreach (var child in children)
        {
            stack.Add(child);
        }

        return stack;
    }

    private View RowCard(string title, string subtitle, params View[] actions)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };

        row.Add(new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                new Label { Text = title, TextColor = Colors.White, FontAttributes = FontAttributes.Bold },
                new Label { Text = subtitle, TextColor = MutedText, FontSize = 12, LineBreakMode = LineBreakMode.WordWrap }
            }
        }, 0, 0);
        row.Add(Horizontal(actions), 1, 0);

        return new Border
        {
            Padding = 10,
            BackgroundColor = Color.FromArgb("#222222"),
            Stroke = CardBorder,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = row
        };
    }

    private static View Card(View content) =>
        new Border
        {
            Padding = 16,
            BackgroundColor = CardBackground,
            Stroke = CardBorder,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = content
        };

    private static View MetricCard(string label, string value) =>
        new Border
        {
            Padding = 14,
            BackgroundColor = Color.FromArgb("#202020"),
            Stroke = CardBorder,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label { Text = label, TextColor = MutedText, FontSize = 12 },
                    new Label { Text = value, TextColor = Colors.White, FontSize = 24, FontAttributes = FontAttributes.Bold }
                }
            }
        };

    private static Label SectionTitle(string text) =>
        new()
        {
            Text = text,
            TextColor = Colors.White,
            FontSize = 22,
            FontAttributes = FontAttributes.Bold
        };

    private Button ActionButton(string text) =>
        new()
        {
            Text = text,
            BackgroundColor = PrimaryRed,
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 8,
            IsEnabled = !_busy
        };

    private Button SecondaryButton(string text) =>
        new()
        {
            Text = text,
            BackgroundColor = Color.FromArgb("#2a2a2a"),
            TextColor = Colors.White,
            CornerRadius = 8,
            IsEnabled = !_busy
        };

    private Button DangerButton(string text) =>
        new()
        {
            Text = text,
            BackgroundColor = Color.FromArgb("#4a151b"),
            TextColor = Colors.White,
            CornerRadius = 8,
            IsEnabled = !_busy
        };

    private static View Horizontal(params View[] children)
    {
        var row = new HorizontalStackLayout { Spacing = 8 };
        foreach (var child in children.Where(child => child.IsVisible))
        {
            row.Add(child);
        }

        return row;
    }

    private static View EmptyText(string text) =>
        new Label { Text = text, TextColor = MutedText, FontAttributes = FontAttributes.Italic };

    private void SetStatus(string message)
    {
        _statusMessage = message;
        _isError = false;
    }

    private void SetError(string message)
    {
        _statusMessage = message;
        _isError = true;
    }

    private string ApiBaseUrl => string.IsNullOrWhiteSpace(_apiBaseUrl) ? "http://localhost:5065" : _apiBaseUrl.Trim().TrimEnd('/');

    private string Token => _session?.Token ?? string.Empty;

    private string UsuarioNome(int id) => _usuarios.FirstOrDefault(usuario => usuario.Id == id)?.Nome ?? $"Usuario #{id}";

    private string CriadorNome(int id) => _criadores.FirstOrDefault(criador => criador.Id == id)?.Nome ?? $"Criador #{id}";

    private string ConteudoTitulo(int id) => _conteudos.FirstOrDefault(conteudo => conteudo.Id == id)?.Titulo ?? $"Conteudo #{id}";

    private static bool IsPlayableMedia(Conteudo conteudo) =>
        IsSupportedMedia(conteudo) &&
        (conteudo.DisponivelParaReproducao ||
         !string.IsNullOrWhiteSpace(conteudo.UrlReproducao) ||
         !string.IsNullOrWhiteSpace(conteudo.UrlMidia) ||
         !string.IsNullOrWhiteSpace(conteudo.ArquivoMidia));

    private static bool IsSupportedMedia(Conteudo conteudo)
    {
        var tipo = NormalizeTipo(conteudo.Tipo);
        return tipo is "video" or "musica" or "podcast" or "audio";
    }

    private static string MediaKindLabel(Conteudo? conteudo)
    {
        var tipo = NormalizeTipo(conteudo?.Tipo);
        return tipo switch
        {
            "video" => "Video",
            "podcast" => "Podcast",
            "musica" or "audio" => "Musica",
            _ => "Midia"
        };
    }

    private string? BuildPlayerSource(Conteudo? conteudo)
    {
        if (conteudo is null)
        {
            return null;
        }

        return AbsolutizarUrl(_reproducaoAtual?.UrlStream)
               ?? AbsolutizarUrl(conteudo.UrlReproducao)
               ?? AbsolutizarUrl(conteudo.UrlMidia)
               ?? (string.IsNullOrWhiteSpace(conteudo.ArquivoMidia)
                   ? null
                   : $"{ApiBaseUrl}/Media/{Uri.EscapeDataString(conteudo.ArquivoMidia)}");
    }

    private string? AbsolutizarUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return url;
        }

        return $"{ApiBaseUrl}/{url.TrimStart('/')}";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
        {
            return "0 MB";
        }

        var mb = bytes / 1024d / 1024d;
        return $"{mb:0.##} MB";
    }

    private static string NormalizeTipo(string? tipo)
    {
        var normalized = (tipo ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }
}
