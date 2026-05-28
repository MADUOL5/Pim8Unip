package br.com.paquetastreaming;

import android.net.Uri;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

public final class Models {
    private Models() {
    }

    public static class Conteudo {
        public int id;
        public String titulo;
        public String tipo;
        public int criadorId;
        public String arquivoMidia;
        public String urlMidia;
        public String urlReproducao;
        public String criadorNome;

        public static Conteudo fromJson(JSONObject json) {
            Conteudo conteudo = new Conteudo();
            conteudo.id = json.optInt("id");
            conteudo.titulo = json.optString("titulo");
            conteudo.tipo = json.optString("tipo");
            conteudo.criadorId = json.optInt("criadorId");
            conteudo.arquivoMidia = json.optString("arquivoMidia", "");
            conteudo.urlMidia = json.optString("urlMidia", "");
            conteudo.urlReproducao = json.optString("urlReproducao", "");

            JSONObject criador = json.optJSONObject("criador");
            conteudo.criadorNome = criador == null
                    ? "Criador #" + conteudo.criadorId
                    : criador.optString("nome", "Criador #" + conteudo.criadorId);
            return conteudo;
        }

        public String mediaUrl(String baseUrl) {
            if (urlMidia != null && urlMidia.startsWith("http")) {
                return urlMidia;
            }

            if (arquivoMidia != null && !arquivoMidia.trim().isEmpty()) {
                return baseUrl.replaceAll("/+$", "") + "/Media/" + Uri.encode(arquivoMidia);
            }

            if (urlReproducao != null && urlReproducao.startsWith("http")) {
                return urlReproducao;
            }

            return "";
        }

        public boolean isVideo() {
            return "video".equalsIgnoreCase(tipo);
        }

        public boolean isPlayable() {
            return "video".equalsIgnoreCase(tipo)
                    || "musica".equalsIgnoreCase(tipo)
                    || "podcast".equalsIgnoreCase(tipo)
                    || "audio".equalsIgnoreCase(tipo);
        }
    }

    public static class Playlist {
        public int id;
        public String nome;
        public int usuarioId;
        public List<Conteudo> itens = new ArrayList<>();

        public static Playlist fromJson(JSONObject json) {
            Playlist playlist = new Playlist();
            playlist.id = json.optInt("id");
            playlist.nome = json.optString("nome");
            playlist.usuarioId = json.optInt("usuarioId");

            JSONArray items = json.optJSONArray("items");
            if (items != null) {
                for (int i = 0; i < items.length(); i++) {
                    JSONObject item = items.optJSONObject(i);
                    if (item != null && item.optJSONObject("conteudo") != null) {
                        playlist.itens.add(Conteudo.fromJson(item.optJSONObject("conteudo")));
                    }
                }
            }
            return playlist;
        }
    }
}
