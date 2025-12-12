using CsvHelper.Configuration;
using ETL.Worker.Models;

namespace ETL.Worker.Mappings
{
    public class FuenteDatosMap : ClassMap<FuenteDatosRecord>
    {
        public FuenteDatosMap()
        {
            Map(m => m.IdFuente).Name("IdFuente");
            Map(m => m.TipoFuente).Name("TipoFuente");
            Map(m => m.FechaCarga).Name("FechaCarga");
        }
    }
}
