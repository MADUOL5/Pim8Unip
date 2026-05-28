namespace StreamingAPI.DTOs
{
    public class PlaylistDTO
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public int UsuarioId { get; set; }
        public List<ItemPlaylistDTO> Items { get; set; } = new();
    }

    public class CreatePlaylistDTO
    {
        public required string Nome { get; set; }
        public int UsuarioId { get; set; }
    }
}
