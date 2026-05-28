namespace StreamingAPI.DTOs
{
    public class ConteudoDTO
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public required string Tipo { get; set; }
        public int CriadorId { get; set; }
        public string? ArquivoMidia { get; set; }
        public string? UrlMidia { get; set; }
        public string? UrlReproducao { get; set; }
        public bool DisponivelParaReproducao { get; set; }
        public CriadorDTO? Criador { get; set; }
    }

    public class CreateConteudoDTO
    {
        public required string Titulo { get; set; }
        public required string Tipo { get; set; }
        public int CriadorId { get; set; }
        public string? ArquivoMidia { get; set; }
    }

    public class ReproducaoConteudoDTO
    {
        public int ConteudoId { get; set; }
        public required string Titulo { get; set; }
        public required string Tipo { get; set; }
        public string? ArquivoMidia { get; set; }
        public string? UrlMidia { get; set; }
        public string? UrlStream { get; set; }
        public string? ContentType { get; set; }
        public long? TamanhoBytes { get; set; }
        public bool ArquivoExiste { get; set; }
        public bool EhVideo { get; set; }
        public bool DisponivelParaReproducao { get; set; }
        public required string Mensagem { get; set; }
    }
}
