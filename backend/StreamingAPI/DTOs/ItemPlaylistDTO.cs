namespace StreamingAPI.DTOs
{
    public class ItemPlaylistDTO
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int ConteudoId { get; set; }
        public ConteudoDTO? Conteudo { get; set; }
    }

    public class AddItemPlaylistDTO
    {
        public int ConteudoId { get; set; }
    }
}
