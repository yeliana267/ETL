using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ETL.Worker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace ETL.Worker.Loaders
{
    public class FuenteDatosLoader : IFuenteDatosLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<FuenteDatosLoader> _logger;

        public FuenteDatosLoader(IConfiguration config, ILogger<FuenteDatosLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<FuenteDatosRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string sql = @"
                INSERT INTO dimension.dim_fuente_datos (
                    id_fuente,
                    tipo_fuente,
                    fecha_carga
                )
                VALUES (
                    @id_fuente,
                    @tipo_fuente,
                    @fecha_carga
                )
                ON CONFLICT (id_fuente) DO UPDATE
                SET tipo_fuente = EXCLUDED.tipo_fuente,
                    fecha_carga = EXCLUDED.fecha_carga;";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@id_fuente", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@tipo_fuente", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@fecha_carga", NpgsqlDbType.Date);

            var count = 0;

            foreach (var r in records)
            {
                cmd.Parameters["@id_fuente"].Value = r.IdFuente;
                cmd.Parameters["@tipo_fuente"].Value = r.TipoFuente;
                cmd.Parameters["@fecha_carga"].Value = r.FechaCarga.Date;

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                count++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Se guardaron/actualizaron {Count} registros en dimension.dim_fuente_datos",
                count
            );
        }
    }
}
