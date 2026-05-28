namespace StreamingAPI.Models
{
    public class Criador
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public ICollection<Conteudo> Conteudos { get; set; } = new List<Conteudo>();
    }
}
