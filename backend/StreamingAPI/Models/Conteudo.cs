namespace StreamingAPI.Models
{
    public class Conteudo
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public required string Tipo { get; set; }
        public int CriadorId { get; set; }
        public string? ArquivoMidia { get; set; }
        public virtual Criador Criador { get; set; } = default!;
        public ICollection<ItemPlaylist> ItemsPlaylist { get; set; } = new List<ItemPlaylist>();
    }
}
