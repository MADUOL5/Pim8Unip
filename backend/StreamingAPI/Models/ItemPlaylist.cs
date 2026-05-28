namespace StreamingAPI.Models
{
    public class ItemPlaylist
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int ConteudoId { get; set; }
        public virtual Playlist Playlist { get; set; } = default!;
        public virtual Conteudo Conteudo { get; set; } = default!;
    }
}
