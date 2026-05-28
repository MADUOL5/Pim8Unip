namespace StreamingAPI.DTOs
{
    public class CriadorDTO
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
    }

    public class CreateCriadorDTO
    {
        public required string Nome { get; set; }
    }
}
