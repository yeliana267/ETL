namespace ETL.Worker.Models
{
    public class FuenteDatosRecord
    {
        public string IdFuente { get; set; } = string.Empty;
        public string TipoFuente { get; set; } = string.Empty;
        public DateTime FechaCarga { get; set; }
    }
}
