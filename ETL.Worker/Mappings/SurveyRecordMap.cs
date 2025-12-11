using CsvHelper.Configuration;
using ETL.Worker.Models;

namespace ETL.Worker.Mappings
{
    public class SurveyRecordMap : ClassMap<SurveyRecord>
    {
        public SurveyRecordMap()
        {
            Map(m => m.IdOpinion).Name("IdOpinion");
            Map(m => m.IdCliente).Name("IdCliente");
            Map(m => m.IdProducto).Name("IdProducto");
            Map(m => m.Fecha).Name("Fecha");
            Map(m => m.Comentario).Name("Comentario");
            Map(m => m.Clasificacion).Name("Clasificacion");
            Map(m => m.PuntajeSatisfaccion).Name("PuntajeSatisfaccion");
            Map(m => m.Fuente).Name("Fuente");
        }
    }
}
