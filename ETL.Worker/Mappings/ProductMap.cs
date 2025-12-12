using CsvHelper.Configuration;
using ETL.Worker.Models;

namespace ETL.Worker.Mappings
{
    public class ProductMap : ClassMap<ProductRecord>
    {
        public ProductMap()
        {
            Map(m => m.IdProducto).Name("IdProducto");
            Map(m => m.Nombre).Name("Nombre");
            Map(m => m.Categoria).Name("Categoría");
        }
    }
}
