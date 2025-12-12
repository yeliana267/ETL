using CsvHelper.Configuration;
using ETL.Worker.Models;

namespace ETL.Worker.Mappings
{
    public class SocialCommentsMap : ClassMap<SocialCommentRecord>
    {
        public SocialCommentsMap()
        {
            Map(m => m.IdComment).Name("IdComment");
            Map(m => m.IdCliente).Name("IdCliente");
            Map(m => m.IdProducto).Name("IdProducto");
            Map(m => m.Fuente).Name("Fuente");
            Map(m => m.Fecha).Name("Fecha");
            Map(m => m.Comentario).Name("Comentario");
        }
    }
}
