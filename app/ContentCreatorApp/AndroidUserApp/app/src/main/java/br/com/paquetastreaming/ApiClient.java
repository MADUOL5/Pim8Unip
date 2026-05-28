package br.com.paquetastreaming;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.OutputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;

public class ApiClient {
    public static final String DEFAULT_BASE_URL = "http://localhost:5065";

    private String baseUrl;
    private String token;

    public ApiClient(String baseUrl) {
        setBaseUrl(baseUrl);
    }

    public void setBaseUrl(String baseUrl) {
        this.baseUrl = normalizeBaseUrl(baseUrl);
    }

    private String normalizeBaseUrl(String baseUrl) {
        if (baseUrl == null || baseUrl.trim().isEmpty()) {
            return DEFAULT_BASE_URL;
        }

        String value = baseUrl.trim().replaceAll("/+$", "");
        String lowerValue = value.toLowerCase();

        if (lowerValue.contains("10.0.2.2")
                || lowerValue.contains("127.0.0.1")
                || lowerValue.contains("192.168.0.5")
                || lowerValue.contains("192.168.0.10")) {
            return DEFAULT_BASE_URL;
        }

        return value;
    }

    public String getBaseUrl() {
        return baseUrl;
    }

    public void setToken(String token) {
        this.token = token;
    }

    public JSONObject cadastrar(String nome, String email, String senha) throws Exception {
        JSONObject body = new JSONObject();
        body.put("nome", nome);
        body.put("email", email);
        body.put("senha", senha);
        return new JSONObject(request("POST", "/api/auth/cadastro", body.toString(), false));
    }

    public JSONObject login(String email, String senha) throws Exception {
        JSONObject body = new JSONObject();
        body.put("email", email);
        body.put("senha", senha);
        return new JSONObject(request("POST", "/api/auth/login", body.toString(), false));
    }

    public JSONArray listarConteudos() throws Exception {
        return new JSONArray(request("GET", "/api/conteudos", null, true));
    }

    public JSONArray listarPlaylists() throws Exception {
        return new JSONArray(request("GET", "/api/playlists", null, true));
    }

    public JSONObject criarPlaylist(String nome, int usuarioId) throws Exception {
        JSONObject body = new JSONObject();
        body.put("nome", nome);
        body.put("usuarioId", usuarioId);
        return new JSONObject(request("POST", "/api/playlists", body.toString(), true));
    }

    public JSONObject adicionarItemPlaylist(int playlistId, int conteudoId) throws Exception {
        JSONObject body = new JSONObject();
        body.put("conteudoId", conteudoId);
        return new JSONObject(request("POST", "/api/playlists/" + playlistId + "/itens", body.toString(), true));
    }

    private String request(String method, String path, String body, boolean auth) throws Exception {
        URL url = new URL(baseUrl + path);
        HttpURLConnection connection = (HttpURLConnection) url.openConnection();
        connection.setRequestMethod(method);
        connection.setConnectTimeout(12000);
        connection.setReadTimeout(16000);
        connection.setRequestProperty("Accept", "application/json");

        if (auth && token != null && !token.isEmpty()) {
            connection.setRequestProperty("Authorization", "Bearer " + token);
        }

        if (body != null) {
            byte[] payload = body.getBytes(StandardCharsets.UTF_8);
            connection.setDoOutput(true);
            connection.setRequestProperty("Content-Type", "application/json; charset=utf-8");
            connection.setFixedLengthStreamingMode(payload.length);
            try (OutputStream output = connection.getOutputStream()) {
                output.write(payload);
            }
        }

        int status = connection.getResponseCode();
        BufferedReader reader = new BufferedReader(new InputStreamReader(
                status >= 200 && status < 300
                        ? connection.getInputStream()
                        : connection.getErrorStream(),
                StandardCharsets.UTF_8));

        StringBuilder result = new StringBuilder();
        String line;
        while ((line = reader.readLine()) != null) {
            result.append(line);
        }

        if (status < 200 || status >= 300) {
            throw new Exception(result.length() == 0 ? "Erro HTTP " + status : result.toString());
        }

        return result.length() == 0 ? "{}" : result.toString();
    }
}
