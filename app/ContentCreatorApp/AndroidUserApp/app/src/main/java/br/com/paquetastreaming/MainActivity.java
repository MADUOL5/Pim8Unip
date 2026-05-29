package br.com.paquetastreaming;

import android.app.Activity;
import android.animation.AnimatorSet;
import android.animation.ObjectAnimator;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.media.MediaMetadataRetriever;
import android.media.MediaPlayer;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.webkit.WebChromeClient;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Button;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.HorizontalScrollView;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.VideoView;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.HashMap;
import java.util.ArrayList;
import java.util.List;

public class MainActivity extends Activity {
    private static final String PREFS = "paqueta_streaming_user_app";
    private static final int RED = Color.rgb(220, 38, 38);
    private static final int RED_DARK = Color.rgb(127, 16, 24);
    private static final int GOLD = Color.rgb(250, 204, 21);
    private static final int BG = Color.rgb(8, 9, 12);
    private static final int BG_2 = Color.rgb(18, 8, 11);
    private static final int PANEL = Color.rgb(20, 22, 29);
    private static final int PANEL_2 = Color.rgb(15, 17, 23);
    private static final int STROKE = Color.rgb(58, 31, 39);
    private static final int TEXT = Color.rgb(255, 247, 237);
    private static final int MUTED = Color.rgb(180, 170, 178);

    private SharedPreferences prefs;
    private ApiClient api;
    private LinearLayout root;
    private LinearLayout body;
    private Button catalogTab;
    private Button playlistsTab;
    private Button profileTab;
    private final Handler handler = new Handler(Looper.getMainLooper());
    private MediaPlayer openingPlayer;
    private boolean openingStarted;

    private String token = "";
    private int usuarioId;
    private String usuarioNome = "";
    private String usuarioEmail = "";
    private String filtroCatalogo = "";
    private String tipoCatalogo = "todos";

    private final List<Models.Conteudo> conteudos = new ArrayList<>();
    private final List<Models.Playlist> playlists = new ArrayList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        prefs = getSharedPreferences(PREFS, MODE_PRIVATE);
        api = new ApiClient(prefs.getString("apiBase", ApiClient.DEFAULT_BASE_URL));
        prefs.edit().putString("apiBase", api.getBaseUrl()).apply();
        token = prefs.getString("token", "");
        usuarioId = prefs.getInt("usuarioId", 0);
        usuarioNome = prefs.getString("usuarioNome", "");
        usuarioEmail = prefs.getString("usuarioEmail", "");
        api.setToken(token);

        showOpening();
    }

    @Override
    protected void onDestroy() {
        if (openingPlayer != null) {
            openingPlayer.release();
            openingPlayer = null;
        }

        super.onDestroy();
    }

    private void showOpening() {
        openingStarted = false;

        FrameLayout screen = new FrameLayout(this);
        screen.setBackground(gradient(BG, Color.rgb(23, 7, 10), GradientDrawable.Orientation.TOP_BOTTOM, dp(0), 0, 0));
        setContentView(screen);

        LinearLayout rays = vertical();
        rays.setAlpha(0.42f);
        rays.setBackground(stripesBackground());
        screen.addView(rays, new FrameLayout.LayoutParams(-1, -1));

        LinearLayout logo = vertical();
        logo.setGravity(Gravity.CENTER);
        logo.setPadding(dp(24), dp(28), dp(24), dp(28));
        logo.setBackground(gradient(RED, Color.rgb(6, 7, 10), GradientDrawable.Orientation.TL_BR, dp(8), GOLD, 1));
        logo.setElevation(dp(14));

        TextView crest = text("P", GOLD, 54);
        crest.setGravity(Gravity.CENTER);
        crest.setTypeface(Typeface.create("sans-serif-black", Typeface.BOLD));
        crest.setBackground(rounded(Color.argb(98, 0, 0, 0), GOLD, 2, dp(8)));
        LinearLayout.LayoutParams crestParams = new LinearLayout.LayoutParams(dp(104), dp(104));
        crestParams.setMargins(0, 0, 0, dp(12));
        logo.addView(crest, crestParams);

        TextView name = title("PAQUETA");
        name.setTextSize(48);
        name.setGravity(Gravity.CENTER);
        logo.addView(name);
        TextView streaming = text("STREAMING", GOLD, 22);
        streaming.setGravity(Gravity.CENTER);
        streaming.setTypeface(Typeface.create("sans-serif-black", Typeface.BOLD));
        logo.addView(streaming);
        logo.addView(text("Abertura rubro-negra", TEXT, 13));

        FrameLayout.LayoutParams logoParams = new FrameLayout.LayoutParams(-1, -2, Gravity.CENTER);
        logoParams.setMargins(dp(22), 0, dp(22), 0);
        screen.addView(logo, logoParams);

        Button enter = primaryButton("Entrar", () -> startOpeningAnimation(screen, logo, rays));
        enter.setTextColor(Color.rgb(20, 11, 5));
        enter.setBackground(rounded(GOLD, GOLD, 1, dp(8)));
        FrameLayout.LayoutParams buttonParams = new FrameLayout.LayoutParams(dp(190), dp(54), Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL);
        buttonParams.setMargins(0, 0, 0, dp(44));
        screen.addView(enter, buttonParams);

        animatePulse(logo);
    }

    private void startOpeningAnimation(FrameLayout screen, View logo, View rays) {
        if (openingStarted) {
            return;
        }

        openingStarted = true;
        playOpeningSound();

        AnimatorSet logoSet = new AnimatorSet();
        logoSet.playTogether(
                ObjectAnimator.ofFloat(logo, View.SCALE_X, 1f, 1.1f, 0.86f),
                ObjectAnimator.ofFloat(logo, View.SCALE_Y, 1f, 1.1f, 0.86f),
                ObjectAnimator.ofFloat(logo, View.ALPHA, 1f, 1f, 0f),
                ObjectAnimator.ofFloat(rays, View.TRANSLATION_X, -dp(180), dp(180)));
        logoSet.setDuration(2500);
        logoSet.start();

        handler.postDelayed(() -> {
            ObjectAnimator fade = ObjectAnimator.ofFloat(screen, View.ALPHA, 1f, 0f);
            fade.setDuration(420);
            fade.start();
        }, 2100);

        handler.postDelayed(this::continueAfterOpening, 2650);
    }

    private void continueAfterOpening() {
        if (token.isEmpty()) {
            showAuth();
        } else {
            showMain();
        }
    }

    private void playOpeningSound() {
        try {
            if (openingPlayer != null) {
                openingPlayer.release();
            }

            openingPlayer = MediaPlayer.create(this, R.raw.opening);
            if (openingPlayer != null) {
                openingPlayer.setVolume(0.78f, 0.78f);
                openingPlayer.start();
            }
        } catch (Exception ignored) {
        }
    }

    private void showAuth() {
        ScrollView scroll = new ScrollView(this);
        scroll.setFillViewport(true);
        scroll.setBackground(gradient(BG, BG_2, GradientDrawable.Orientation.TOP_BOTTOM, dp(0), 0, 0));
        root = vertical();
        root.setPadding(dp(18), dp(22), dp(18), dp(24));
        scroll.addView(root);
        setContentView(scroll);

        LinearLayout hero = brandedPanel();
        hero.setGravity(Gravity.CENTER_HORIZONTAL);
        hero.addView(pill("Rubro-negro streaming"));
        TextView brand = title("PAQUETA'S\nSTREAMING");
        brand.setTextSize(42);
        brand.setGravity(Gravity.CENTER);
        hero.addView(brand);
        TextView subtitle = text("Bem-vindo ao seu novo streaming favorito. Entre para assistir, curtir e comentar.", Color.rgb(240, 217, 202), 15);
        subtitle.setGravity(Gravity.CENTER);
        hero.addView(subtitle);
        hero.addView(flagBars());
        root.addView(hero);

        LinearLayout login = panel();
        login.addView(pill("Entrar"));
        login.addView(sectionTitle("Login"));
        EditText email = input("E-mail");
        EditText senha = input("Senha");
        senha.setInputType(0x00000081);
        login.addView(email);
        login.addView(senha);
        login.addView(primaryButton("Entrar", () -> runAsync(() -> {
            api.setBaseUrl(api.getBaseUrl());
            JSONObject response = api.login(email.getText().toString(), senha.getText().toString());
            saveSession(response, api.getBaseUrl());
            runOnUiThread(this::showMain);
        })));
        root.addView(login);

        LinearLayout cadastro = panel();
        cadastro.addView(pill("Primeiro acesso"));
        cadastro.addView(sectionTitle("Criar conta"));
        EditText nome = input("Nome");
        EditText emailCadastro = input("E-mail");
        EditText senhaCadastro = input("Senha");
        senhaCadastro.setInputType(0x00000081);
        cadastro.addView(nome);
        cadastro.addView(emailCadastro);
        cadastro.addView(senhaCadastro);
        cadastro.addView(primaryButton("Cadastrar e entrar", () -> runAsync(() -> {
            api.setBaseUrl(api.getBaseUrl());
            JSONObject response = api.cadastrar(
                    nome.getText().toString(),
                    emailCadastro.getText().toString(),
                    senhaCadastro.getText().toString());
            saveSession(response, api.getBaseUrl());
            runOnUiThread(this::showMain);
        })));
        root.addView(cadastro);
    }

    private void showMain() {
        root = vertical();
        root.setBackground(gradient(BG, Color.rgb(12, 8, 10), GradientDrawable.Orientation.TOP_BOTTOM, dp(0), 0, 0));
        setContentView(root);

        LinearLayout header = brandedPanel();
        header.setPadding(dp(18), dp(18), dp(18), dp(16));
        header.addView(pill("Ao vivo na arquibancada digital"));
        TextView appTitle = title("PAQUETA'S STREAMING");
        appTitle.setTextSize(30);
        header.addView(appTitle);
        header.addView(text("Ola, " + usuarioNome + ". Escolha sua sessao e aperte o play.", GOLD, 14));
        root.addView(header);

        HorizontalScrollView tabScroll = new HorizontalScrollView(this);
        tabScroll.setHorizontalScrollBarEnabled(false);
        LinearLayout tabs = horizontal();
        tabs.setPadding(dp(10), dp(6), dp(10), dp(6));
        catalogTab = tab("Catalogo", this::showCatalog);
        playlistsTab = tab("Playlists", this::showPlaylists);
        profileTab = tab("Perfil", this::showProfile);
        tabs.addView(catalogTab);
        tabs.addView(playlistsTab);
        tabs.addView(profileTab);
        tabScroll.addView(tabs);
        root.addView(tabScroll);

        ScrollView scroll = new ScrollView(this);
        body = vertical();
        body.setPadding(dp(14), dp(10), dp(14), dp(24));
        scroll.addView(body);
        root.addView(scroll, new LinearLayout.LayoutParams(-1, 0, 1));

        loadProtectedData(this::showCatalog);
    }

    private void loadProtectedData(Runnable afterLoad) {
        runAsync(() -> {
            JSONArray conteudosJson = api.listarConteudos();
            JSONArray playlistsJson = api.listarPlaylists();

            conteudos.clear();
            playlists.clear();

            for (int i = 0; i < conteudosJson.length(); i++) {
                conteudos.add(Models.Conteudo.fromJson(conteudosJson.getJSONObject(i)));
            }

            for (int i = 0; i < playlistsJson.length(); i++) {
                playlists.add(Models.Playlist.fromJson(playlistsJson.getJSONObject(i)));
            }

            runOnUiThread(afterLoad);
        });
    }

    private void showCatalog() {
        setActiveTab("Catalogo");
        body.removeAllViews();
        LinearLayout hero = brandedPanel();
        hero.addView(pill("Cine Paqueta"));
        TextView title = sectionTitle("Catalogo para assistir");
        title.setTextSize(26);
        hero.addView(title);
        hero.addView(text("Videos, musicas e podcasts com visual de plataforma de streaming.", MUTED, 14));
        hero.addView(flagBars());
        body.addView(hero);

        EditText busca = input("Buscar por titulo ou criador");
        busca.setText(filtroCatalogo);
        body.addView(busca);

        LinearLayout chips = horizontal();
        chips.addView(filter("Todos", "todos"));
        chips.addView(filter("Videos", "video"));
        chips.addView(filter("Musicas", "musica"));
        chips.addView(filter("Podcasts", "podcast"));
        body.addView(chips);

        LinearLayout list = vertical();
        body.addView(list);

        Runnable render = () -> renderCatalogList(list);
        busca.addTextChangedListener(new TextWatcher() {
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            public void onTextChanged(CharSequence s, int start, int before, int count) {
                filtroCatalogo = s.toString();
                render.run();
            }

            public void afterTextChanged(Editable s) {
            }
        });

        render.run();
    }

    private void renderCatalogList(LinearLayout list) {
        list.removeAllViews();
        String query = filtroCatalogo.toLowerCase().trim();
        int count = 0;

        for (Models.Conteudo conteudo : conteudos) {
            boolean tipoOk = "todos".equals(tipoCatalogo) || conteudo.tipo.equalsIgnoreCase(tipoCatalogo);
            boolean buscaOk = query.isEmpty()
                    || conteudo.titulo.toLowerCase().contains(query)
                    || conteudo.criadorNome.toLowerCase().contains(query);

            if (!tipoOk || !buscaOk) {
                continue;
            }

            count++;
            LinearLayout card = mediaCard(conteudo);
            card.addView(horizontalButtons(
                    primaryButton("Assistir", () -> showPlayer(conteudo)),
                    ghostButton(isLiked(conteudo.id) ? "Curtido" : "Curtir", () -> {
                        toggleLike(conteudo.id);
                        showCatalog();
                    })
            ));
            list.addView(card);
        }

        if (count == 0) {
            LinearLayout empty = panel();
            empty.addView(sectionTitle("Nada por aqui ainda"));
            empty.addView(text("Tente outro filtro ou publique conteudos pela interface .NET MAUI.", MUTED, 15));
            list.addView(empty);
        }
    }

    private void showPlayer(Models.Conteudo conteudo) {
        body.removeAllViews();
        LinearLayout stage = brandedPanel();
        stage.addView(pill(conteudo.tipo.toUpperCase()));
        TextView playerTitle = sectionTitle(conteudo.titulo);
        playerTitle.setTextSize(28);
        stage.addView(playerTitle);
        stage.addView(text(conteudo.criadorNome + " | pronto para reproducao", MUTED, 14));

        String mediaUrl = conteudo.mediaUrl(api.getBaseUrl());
        if (mediaUrl.isEmpty()) {
            stage.addView(emptyPoster("Sem midia", "Este conteudo ainda nao possui arquivo de midia."));
        } else {
            WebView player = buildWebPlayer(conteudo, mediaUrl);
            LinearLayout.LayoutParams playerParams = new LinearLayout.LayoutParams(-1, conteudo.isVideo() ? dp(260) : dp(150));
            playerParams.setMargins(0, dp(12), 0, dp(12));
            stage.addView(player, playerParams);
        }
        body.addView(stage);

        body.addView(horizontalButtons(
                primaryButton("Catalogo", this::showCatalog),
                ghostButton(isLiked(conteudo.id) ? "Remover curtida" : "Curtir", () -> {
                    toggleLike(conteudo.id);
                    showPlayer(conteudo);
                })
        ));

        renderPlaylistActions(conteudo);

        body.addView(sectionTitle("Comentarios da comunidade"));
        EditText comentario = input("Escreva uma opiniao para a comunidade");
        body.addView(comentario);
        body.addView(primaryButton("Publicar comentario", () -> {
            addComment(conteudo.id, comentario.getText().toString());
            showPlayer(conteudo);
        }));

        for (String item : commentsFor(conteudo.id)) {
            TextView comment = text(item, TEXT, 14);
            comment.setPadding(dp(12), dp(10), dp(12), dp(10));
            comment.setBackground(rounded(PANEL_2, STROKE, 1, dp(8)));
            body.addView(comment);
        }
    }

    private void showPlaylists() {
        setActiveTab("Playlists");
        body.removeAllViews();
        LinearLayout hero = brandedPanel();
        hero.addView(pill("Minha area"));
        hero.addView(sectionTitle("Minhas playlists"));
        hero.addView(text("Colecoes vinculadas ao usuario logado. No diagrama e no contrato da API, playlist usa nome e usuarioId.", MUTED, 14));
        body.addView(hero);

        LinearLayout create = panel();
        create.addView(pill("Nova playlist"));
        EditText nomePlaylist = input("Nome da playlist");
        create.addView(nomePlaylist);
        create.addView(primaryButton("Criar playlist", () -> runAsync(() -> {
            api.criarPlaylist(nomePlaylist.getText().toString(), usuarioId);
            loadProtectedData(this::showPlaylists);
        })));
        body.addView(create);

        int count = 0;
        for (Models.Playlist playlist : playlists) {
            if (playlist.usuarioId != usuarioId) {
                continue;
            }

            count++;
            LinearLayout card = panel();
            card.addView(pill("playlist #" + playlist.id));
            card.addView(text(playlist.nome, TEXT, 20));
            card.addView(text(playlist.itens.size() + " item(ns)", MUTED, 14));
            for (Models.Conteudo conteudo : playlist.itens) {
                card.addView(ghostButton(conteudo.titulo, () -> showPlayer(conteudo)));
            }
            body.addView(card);
        }

        if (count == 0) {
            LinearLayout empty = panel();
            empty.addView(sectionTitle("Nenhuma playlist ainda"));
            empty.addView(text("Crie playlists pela interface .NET MAUI e elas aparecem aqui.", MUTED, 15));
            body.addView(empty);
        }
    }

    private void renderPlaylistActions(Models.Conteudo conteudo) {
        LinearLayout box = panel();
        box.addView(pill("Playlists"));
        box.addView(sectionTitle("Salvar nesta sessao"));

        int count = 0;
        for (Models.Playlist playlist : playlists) {
            if (playlist.usuarioId != usuarioId) {
                continue;
            }

            count++;
            box.addView(ghostButton("Adicionar em " + playlist.nome, () -> runAsync(() -> {
                api.adicionarItemPlaylist(playlist.id, conteudo.id);
                loadProtectedData(() -> showPlayer(conteudo));
            })));
        }

        if (count == 0) {
            box.addView(text("Crie uma playlist na aba Playlists para salvar este conteudo.", MUTED, 14));
        }

        body.addView(box);
    }

    private WebView buildWebPlayer(Models.Conteudo conteudo, String mediaUrl) {
        WebView webView = new WebView(this);
        webView.setBackgroundColor(Color.BLACK);
        webView.setLayerType(View.LAYER_TYPE_HARDWARE, null);
        webView.setWebChromeClient(new WebChromeClient());
        webView.setWebViewClient(new WebViewClient());

        WebSettings settings = webView.getSettings();
        settings.setJavaScriptEnabled(true);
        settings.setDomStorageEnabled(true);
        settings.setLoadWithOverviewMode(true);
        settings.setUseWideViewPort(true);
        settings.setMediaPlaybackRequiresUserGesture(false);
        settings.setMixedContentMode(WebSettings.MIXED_CONTENT_ALWAYS_ALLOW);

        webView.loadDataWithBaseURL(
                api.getBaseUrl(),
                buildPlayerHtml(conteudo, mediaUrl),
                "text/html",
                "UTF-8",
                null);
        return webView;
    }

    private String buildPlayerHtml(Models.Conteudo conteudo, String mediaUrl) {
        String source = escapeHtml(mediaUrl);
        String title = escapeHtml(conteudo.titulo);
        String mediaTag = conteudo.isVideo()
                ? "<video id=\"player\" src=\"" + source + "\" controls autoplay playsinline webkit-playsinline preload=\"auto\"></video>"
                : "<audio id=\"player\" src=\"" + source + "\" controls autoplay preload=\"auto\"></audio>";

        return "<!doctype html><html><head>"
                + "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1,maximum-scale=1\">"
                + "<style>"
                + "html,body{margin:0;width:100%;height:100%;background:#000;color:#fff7ed;font-family:Arial,sans-serif;overflow:hidden;}"
                + ".wrap{position:relative;width:100%;height:100%;display:flex;align-items:center;justify-content:center;background:#000;}"
                + "video{width:100%;height:100%;background:#000;object-fit:contain;}"
                + "audio{width:92%;}"
                + ".label{position:absolute;left:12px;top:10px;right:12px;padding:7px 10px;border-radius:8px;background:linear-gradient(90deg,rgba(220,38,38,.84),rgba(0,0,0,.44));color:#facc15;font-weight:900;font-size:12px;text-transform:uppercase;pointer-events:none;}"
                + ".hint{position:absolute;left:12px;bottom:10px;color:rgba(255,247,237,.62);font-size:11px;pointer-events:none;}"
                + "</style></head><body><div class=\"wrap\">"
                + mediaTag
                + "<div class=\"label\">" + title + "</div>"
                + "<div class=\"hint\">Player HTML5 do app Android</div>"
                + "</div><script>"
                + "const p=document.getElementById('player');"
                + "if(p){p.addEventListener('loadedmetadata',()=>{try{p.currentTime=Math.min(.25,p.duration||.25)}catch(e){}});"
                + "setTimeout(()=>{try{p.play()}catch(e){}},250);}"
                + "</script></body></html>";
    }

    private String escapeHtml(String value) {
        if (value == null) {
            return "";
        }

        return value
                .replace("&", "&amp;")
                .replace("\"", "&quot;")
                .replace("<", "&lt;")
                .replace(">", "&gt;");
    }

    private void showProfile() {
        setActiveTab("Perfil");
        body.removeAllViews();
        LinearLayout profile = brandedPanel();
        profile.addView(pill("Perfil conectado"));
        profile.addView(sectionTitle(usuarioNome));
        profile.addView(text(usuarioEmail, MUTED, 14));
        profile.addView(flagBars());
        body.addView(profile);
        body.addView(primaryButton("Atualizar dados", () -> loadProtectedData(this::showCatalog)));
        body.addView(ghostButton("Sair", () -> {
            prefs.edit()
                    .remove("token")
                    .remove("usuarioId")
                    .remove("usuarioNome")
                    .remove("usuarioEmail")
                    .apply();
            token = "";
            usuarioId = 0;
            usuarioNome = "";
            usuarioEmail = "";
            api.setToken("");
            showAuth();
        }));
    }

    private Button filter(String label, String value) {
        Button button = ghostButton(label, () -> {
            tipoCatalogo = value;
            showCatalog();
        });
        if (tipoCatalogo.equals(value)) {
            button.setBackground(rounded(GOLD, GOLD, 1, dp(24)));
            button.setTextColor(Color.rgb(20, 11, 5));
        }
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(-2, dp(42));
        params.setMargins(0, dp(6), dp(8), dp(8));
        button.setLayoutParams(params);
        return button;
    }

    private boolean isLiked(int conteudoId) {
        return prefs.getBoolean("like." + usuarioId + "." + conteudoId, false);
    }

    private void toggleLike(int conteudoId) {
        prefs.edit()
                .putBoolean("like." + usuarioId + "." + conteudoId, !isLiked(conteudoId))
                .apply();
    }

    private void addComment(int conteudoId, String message) {
        if (message == null || message.trim().isEmpty()) {
            toast("Digite um comentario.");
            return;
        }

        try {
            JSONArray comments = new JSONArray(prefs.getString("comments." + conteudoId, "[]"));
            comments.put(usuarioNome + ": " + message.trim());
            prefs.edit().putString("comments." + conteudoId, comments.toString()).apply();
        } catch (Exception ex) {
            toast(ex.getMessage());
        }
    }

    private List<String> commentsFor(int conteudoId) {
        List<String> comments = new ArrayList<>();
        try {
            JSONArray array = new JSONArray(prefs.getString("comments." + conteudoId, "[]"));
            for (int i = array.length() - 1; i >= 0; i--) {
                comments.add(array.optString(i));
            }
        } catch (Exception ignored) {
        }
        return comments;
    }

    private void saveSession(JSONObject response, String apiBase) throws Exception {
        JSONObject usuario = response.getJSONObject("usuario");
        token = response.getString("token");
        usuarioId = usuario.getInt("id");
        usuarioNome = usuario.getString("nome");
        usuarioEmail = usuario.getString("email");
        api.setToken(token);

        prefs.edit()
                .putString("apiBase", apiBase)
                .putString("token", token)
                .putInt("usuarioId", usuarioId)
                .putString("usuarioNome", usuarioNome)
                .putString("usuarioEmail", usuarioEmail)
                .apply();
    }

    private void runAsync(ThrowingRunnable runnable) {
        new Thread(() -> {
            try {
                runnable.run();
            } catch (Exception ex) {
                runOnUiThread(() -> toast(ex.getMessage()));
            }
        }).start();
    }

    private LinearLayout panel() {
        LinearLayout layout = vertical();
        layout.setPadding(dp(16), dp(16), dp(16), dp(16));
        layout.setBackground(rounded(PANEL, STROKE, 1, dp(8)));
        layout.setElevation(dp(6));
        layout.setAlpha(0f);
        layout.setTranslationY(dp(12));
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(-1, -2);
        params.setMargins(0, dp(10), 0, dp(10));
        layout.setLayoutParams(params);
        handler.post(() -> layout.animate()
                .alpha(1f)
                .translationY(0f)
                .setDuration(280)
                .start());
        return layout;
    }

    private LinearLayout brandedPanel() {
        LinearLayout layout = panel();
        layout.setBackground(gradient(Color.rgb(25, 9, 12), Color.rgb(9, 10, 14), GradientDrawable.Orientation.TL_BR, dp(8), Color.argb(94, 250, 204, 21), 1));
        return layout;
    }

    private LinearLayout mediaCard(Models.Conteudo conteudo) {
        LinearLayout card = panel();
        card.setBackground(gradient(Color.rgb(19, 20, 26), Color.rgb(8, 9, 12), GradientDrawable.Orientation.TOP_BOTTOM, dp(8), STROKE, 1));

        FrameLayout poster = mediaCover(conteudo);
        LinearLayout.LayoutParams posterParams = new LinearLayout.LayoutParams(-1, dp(168));
        posterParams.setMargins(0, 0, 0, dp(12));
        card.addView(poster, posterParams);

        card.addView(pill(conteudo.tipo.toUpperCase()));
        card.addView(text(conteudo.titulo, TEXT, 20));
        card.addView(text(conteudo.criadorNome, MUTED, 14));
        card.addView(text(conteudo.isPlayable() ? "Pronto para assistir" : "Catalogado", GOLD, 12));
        return card;
    }

    private FrameLayout mediaCover(Models.Conteudo conteudo) {
        FrameLayout cover = new FrameLayout(this);
        cover.setBackground(gradient(cardAccent(conteudo.id), Color.rgb(7, 8, 11), GradientDrawable.Orientation.TL_BR, dp(8), Color.argb(80, 250, 204, 21), 1));

        ImageView thumbnail = new ImageView(this);
        thumbnail.setScaleType(ImageView.ScaleType.CENTER_CROP);
        thumbnail.setAlpha(0f);
        cover.addView(thumbnail, new FrameLayout.LayoutParams(-1, -1));

        String mediaUrl = conteudo.mediaUrl(api.getBaseUrl());
        if (conteudo.isVideo() && !mediaUrl.isEmpty()) {
            VideoView preview = new VideoView(this);
            preview.setVideoURI(Uri.parse(mediaUrl));
            preview.setAlpha(0f);
            preview.setOnPreparedListener(mp -> {
                mp.setVolume(0f, 0f);
                mp.setLooping(true);
                preview.seekTo(700);
                preview.start();
                preview.animate().alpha(0.72f).setDuration(380).start();
            });
            preview.setOnErrorListener((mp, what, extra) -> true);
            cover.addView(preview, new FrameLayout.LayoutParams(-1, -1));
        }

        LinearLayout overlay = vertical();
        overlay.setGravity(Gravity.CENTER);
        overlay.setPadding(dp(12), dp(18), dp(12), dp(18));
        TextView icon = text(conteudo.isVideo() ? "PLAY" : "CAST", GOLD, 20);
        icon.setGravity(Gravity.CENTER);
        icon.setTypeface(Typeface.create("sans-serif-black", Typeface.BOLD));
        overlay.addView(icon);
        TextView posterTitle = text(conteudo.titulo, TEXT, 22);
        posterTitle.setGravity(Gravity.CENTER);
        posterTitle.setTypeface(Typeface.create("sans-serif-black", Typeface.BOLD));
        overlay.addView(posterTitle);
        overlay.addView(text(conteudo.criadorNome, Color.rgb(240, 217, 202), 13));
        cover.addView(overlay, new FrameLayout.LayoutParams(-1, -1));

        if (conteudo.isVideo()) {
            loadVideoThumbnail(thumbnail, overlay, conteudo);
        }

        return cover;
    }

    private void loadVideoThumbnail(ImageView thumbnail, View overlay, Models.Conteudo conteudo) {
        String mediaUrl = conteudo.mediaUrl(api.getBaseUrl());
        if (mediaUrl.isEmpty()) {
            return;
        }

        new Thread(() -> {
            MediaMetadataRetriever retriever = new MediaMetadataRetriever();
            try {
                retriever.setDataSource(mediaUrl, new HashMap<>());
                Bitmap bitmap = retriever.getFrameAtTime(700_000, MediaMetadataRetriever.OPTION_CLOSEST_SYNC);
                if (bitmap != null) {
                    runOnUiThread(() -> {
                        thumbnail.setImageBitmap(bitmap);
                        thumbnail.animate().alpha(1f).setDuration(260).start();
                        overlay.animate().alpha(0.18f).setDuration(260).start();
                    });
                }
            } catch (Exception ignored) {
            } finally {
                try {
                    retriever.release();
                } catch (Exception ignored) {
                }
            }
        }).start();
    }

    private LinearLayout emptyPoster(String headline, String message) {
        LinearLayout poster = vertical();
        poster.setGravity(Gravity.CENTER);
        poster.setPadding(dp(18), dp(18), dp(18), dp(18));
        poster.setBackground(gradient(Color.rgb(38, 12, 16), Color.rgb(8, 9, 12), GradientDrawable.Orientation.TL_BR, dp(8), STROKE, 1));
        poster.addView(pill("Player"));
        TextView title = sectionTitle(headline);
        title.setGravity(Gravity.CENTER);
        poster.addView(title);
        TextView body = text(message, MUTED, 14);
        body.setGravity(Gravity.CENTER);
        poster.addView(body);
        return poster;
    }

    private LinearLayout flagBars() {
        LinearLayout bars = vertical();
        bars.setPadding(0, dp(12), 0, 0);
        bars.addView(flagBar(RED));
        bars.addView(flagBar(Color.BLACK));
        bars.addView(flagBar(RED_DARK));
        return bars;
    }

    private View flagBar(int color) {
        View view = new View(this);
        view.setBackgroundColor(color);
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(-1, dp(8));
        params.setMargins(0, dp(2), 0, dp(2));
        view.setLayoutParams(params);
        return view;
    }

    private LinearLayout vertical() {
        LinearLayout layout = new LinearLayout(this);
        layout.setOrientation(LinearLayout.VERTICAL);
        return layout;
    }

    private LinearLayout horizontal() {
        LinearLayout layout = new LinearLayout(this);
        layout.setOrientation(LinearLayout.HORIZONTAL);
        layout.setGravity(Gravity.CENTER_VERTICAL);
        return layout;
    }

    private LinearLayout horizontalButtons(View first, View second) {
        LinearLayout row = horizontal();
        row.setPadding(0, dp(10), 0, 0);
        LinearLayout.LayoutParams firstParams = new LinearLayout.LayoutParams(0, dp(50), 1);
        firstParams.setMargins(0, 0, dp(6), 0);
        LinearLayout.LayoutParams secondParams = new LinearLayout.LayoutParams(0, dp(50), 1);
        secondParams.setMargins(dp(6), 0, 0, 0);
        row.addView(first, firstParams);
        row.addView(second, secondParams);
        return row;
    }

    private TextView title(String value) {
        TextView view = text(value, TEXT, 28);
        view.setTypeface(Typeface.create("sans-serif-black", Typeface.BOLD));
        view.setAllCaps(true);
        return view;
    }

    private TextView sectionTitle(String value) {
        TextView view = text(value, TEXT, 22);
        view.setTypeface(null, 1);
        view.setPadding(0, dp(8), 0, dp(6));
        return view;
    }

    private TextView text(String value, int color, int size) {
        TextView view = new TextView(this);
        view.setText(value);
        view.setTextColor(color);
        view.setTextSize(size);
        view.setPadding(0, dp(4), 0, dp(4));
        view.setIncludeFontPadding(true);
        return view;
    }

    private TextView pill(String value) {
        TextView view = text(value, GOLD, 12);
        view.setTypeface(Typeface.create("sans-serif-black", Typeface.BOLD));
        view.setGravity(Gravity.CENTER);
        view.setAllCaps(true);
        view.setPadding(dp(10), dp(5), dp(10), dp(5));
        view.setBackground(rounded(Color.argb(72, 250, 204, 21), Color.argb(150, 250, 204, 21), 1, dp(18)));
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(-2, -2);
        params.setMargins(0, 0, 0, dp(8));
        view.setLayoutParams(params);
        return view;
    }

    private EditText input(String hint) {
        EditText input = new EditText(this);
        input.setHint(hint);
        input.setHintTextColor(MUTED);
        input.setTextColor(TEXT);
        input.setTextSize(14);
        input.setSingleLine(true);
        input.setPadding(dp(12), dp(8), dp(12), dp(8));
        input.setBackground(rounded(Color.rgb(13, 16, 22), Color.rgb(76, 48, 58), 1, dp(8)));
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(-1, dp(52));
        params.setMargins(0, dp(8), 0, dp(8));
        input.setLayoutParams(params);
        return input;
    }

    private Button primaryButton(String label, Runnable action) {
        Button button = new Button(this);
        button.setText(label);
        button.setTextColor(TEXT);
        button.setAllCaps(false);
        button.setTypeface(Typeface.create("sans-serif-black", Typeface.BOLD));
        button.setBackground(rounded(RED, Color.argb(130, 248, 113, 113), 1, dp(8)));
        button.setOnClickListener(v -> action.run());
        button.setPadding(dp(10), 0, dp(10), 0);
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(-1, dp(50));
        params.setMargins(0, dp(8), 0, dp(8));
        button.setLayoutParams(params);
        withPressEffect(button);
        return button;
    }

    private Button ghostButton(String label, Runnable action) {
        Button button = new Button(this);
        button.setText(label);
        button.setTextColor(GOLD);
        button.setAllCaps(false);
        button.setTypeface(Typeface.create("sans-serif", Typeface.BOLD));
        button.setBackground(rounded(Color.rgb(18, 20, 26), Color.rgb(63, 36, 45), 1, dp(8)));
        button.setOnClickListener(v -> action.run());
        button.setPadding(dp(10), 0, dp(10), 0);
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(-1, dp(48));
        params.setMargins(0, dp(6), 0, dp(6));
        button.setLayoutParams(params);
        withPressEffect(button);
        return button;
    }

    private Button tab(String label, Runnable action) {
        Button button = ghostButton(label, action);
        styleTab(button, false);
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(dp(132), dp(48));
        params.setMargins(dp(5), dp(4), dp(5), dp(8));
        button.setLayoutParams(params);
        return button;
    }

    private void setActiveTab(String label) {
        styleTab(catalogTab, "Catalogo".equals(label));
        styleTab(playlistsTab, "Playlists".equals(label));
        styleTab(profileTab, "Perfil".equals(label));
    }

    private void styleTab(Button button, boolean active) {
        if (button == null) {
            return;
        }

        if (active) {
            button.setTextColor(Color.rgb(20, 11, 5));
            button.setBackground(rounded(GOLD, GOLD, 1, dp(22)));
            button.animate().scaleX(1.03f).scaleY(1.03f).setDuration(160).start();
            return;
        }

        button.setTextColor(TEXT);
        button.setBackground(gradient(Color.rgb(30, 12, 16), Color.rgb(13, 15, 20), GradientDrawable.Orientation.LEFT_RIGHT, dp(22), Color.rgb(74, 38, 48), 1));
        button.animate().scaleX(1f).scaleY(1f).setDuration(160).start();
    }

    private GradientDrawable rounded(int color, int strokeColor, int strokeWidth, int radius) {
        GradientDrawable drawable = new GradientDrawable();
        drawable.setColor(color);
        drawable.setCornerRadius(radius);
        if (strokeColor != 0 && strokeWidth > 0) {
            drawable.setStroke(dp(strokeWidth), strokeColor);
        }
        return drawable;
    }

    private GradientDrawable gradient(int start, int end, GradientDrawable.Orientation orientation, int radius, int strokeColor, int strokeWidth) {
        GradientDrawable drawable = new GradientDrawable(orientation, new int[]{start, end});
        drawable.setCornerRadius(radius);
        if (strokeColor != 0 && strokeWidth > 0) {
            drawable.setStroke(dp(strokeWidth), strokeColor);
        }
        return drawable;
    }

    private GradientDrawable stripesBackground() {
        return new GradientDrawable(
                GradientDrawable.Orientation.LEFT_RIGHT,
                new int[]{
                        Color.argb(90, 220, 38, 38),
                        Color.argb(220, 0, 0, 0),
                        Color.argb(100, 220, 38, 38),
                        Color.argb(230, 0, 0, 0)
                });
    }

    private int cardAccent(int id) {
        int palette = Math.abs(id % 4);
        if (palette == 0) {
            return Color.rgb(220, 38, 38);
        }

        if (palette == 1) {
            return Color.rgb(127, 16, 24);
        }

        if (palette == 2) {
            return Color.rgb(42, 12, 16);
        }

        return Color.rgb(80, 18, 24);
    }

    private void animatePulse(View view) {
        ObjectAnimator scaleX = ObjectAnimator.ofFloat(view, View.SCALE_X, 0.94f, 1.02f);
        scaleX.setRepeatCount(ObjectAnimator.INFINITE);
        scaleX.setRepeatMode(ObjectAnimator.REVERSE);
        scaleX.setDuration(850);
        ObjectAnimator scaleY = ObjectAnimator.ofFloat(view, View.SCALE_Y, 0.94f, 1.02f);
        scaleY.setRepeatCount(ObjectAnimator.INFINITE);
        scaleY.setRepeatMode(ObjectAnimator.REVERSE);
        scaleY.setDuration(850);
        AnimatorSet set = new AnimatorSet();
        set.playTogether(scaleX, scaleY);
        set.start();
    }

    private void withPressEffect(View view) {
        view.setOnTouchListener((v, event) -> {
            if (event.getAction() == MotionEvent.ACTION_DOWN) {
                v.animate().scaleX(0.97f).scaleY(0.97f).setDuration(90).start();
            } else if (event.getAction() == MotionEvent.ACTION_UP || event.getAction() == MotionEvent.ACTION_CANCEL) {
                v.animate().scaleX(1f).scaleY(1f).setDuration(120).start();
            }

            return false;
        });
    }

    private void toast(String message) {
        Toast.makeText(this, message == null ? "Erro inesperado." : message, Toast.LENGTH_LONG).show();
    }

    private int dp(int value) {
        return (int) (value * getResources().getDisplayMetrics().density);
    }

    private interface ThrowingRunnable {
        void run() throws Exception;
    }
}
