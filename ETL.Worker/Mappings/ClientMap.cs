using CsvHelper.Configuration;
using ETL.Worker.Models;

namespace ETL.Worker.Mappings
{
    public class ClientMap : ClassMap<ClientRecord>
    {
        public ClientMap()
        {
            Map(m => m.IdCliente).Name("IdCliente");
            Map(m => m.Nombre).Name("Nombre");
            Map(m => m.Email).Name("Email");
        }
    }
}
