namespace ETL.Worker.Models
{
    public class SurveyRecord
    {
        public int IdOpinion { get; set; }
        public int IdCliente { get; set; }   
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public int PuntajeSatisfaccion { get; set; }    
        public string Clasificacion { get; set; }
        public string Comentario { get; set; }
        public string Fuente { get; set; }

    }
}
