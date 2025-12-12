using CsvHelper;
using ETL.Worker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace ETL.Worker.Loaders
{
    public class ClasificacionOpinionLoader : IClasificacionOpinionLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<ClasificacionOpinionLoader> _logger;

        public ClasificacionOpinionLoader(IConfiguration config, ILogger<ClasificacionOpinionLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<ClasificacionOpinionRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string sql = @"
                INSERT INTO dimension.dim_clasificacion_opinion (nombre)
                VALUES (@nombre)
                ON CONFLICT (nombre) DO NOTHING;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@nombre", NpgsqlDbType.Varchar);

            var count = 0;

            foreach (var r in records)
            {
                cmd.Parameters["@nombre"].Value = r.Nombre;
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                count++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Se insertaron/aseguraron {Count} filas en dimension.dim_clasificacion_opinion",
                count
            );
        }
    }
}
