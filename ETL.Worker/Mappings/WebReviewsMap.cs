using CsvHelper.Configuration;
using ETL.Worker.Models;

namespace ETL.Worker.Mappings
{
    public class WebReviewsMap : ClassMap<WebReviewRecord>
    {
        public WebReviewsMap()
        {
            Map(m => m.IdReview).Name("IdReview");
            Map(m => m.IdCliente).Name("IdCliente");
            Map(m => m.IdProducto).Name("IdProducto");
            Map(m => m.Fecha).Name("Fecha");
            Map(m => m.Comentario).Name("Comentario");
            Map(m => m.Rating).Name("Rating");


        }
    }
}
