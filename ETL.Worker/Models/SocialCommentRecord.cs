namespace ETL.Worker.Models
{
    public class SocialCommentRecord
    {
        public string IdComment { get; set; } = default!;
        public string? IdCliente { get; set; }
        public string IdProducto { get; set; } = default!;
        public string Fuente { get; set; } = default!;
        public DateTime Fecha { get; set; }
        public string? Comentario { get; set; }
    }
}
