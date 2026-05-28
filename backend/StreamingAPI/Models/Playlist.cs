namespace StreamingAPI.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = default!;
        public ICollection<ItemPlaylist> Items { get; set; } = new List<ItemPlaylist>();
    }
}
