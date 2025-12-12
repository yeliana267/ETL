using CsvHelper;
using ETL.Worker.Models;
using Npgsql;
using NpgsqlTypes;

namespace ETL.Worker.Loaders
{
    public class RedSocialLoader : IRedSocialLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<RedSocialLoader> _logger;

        public RedSocialLoader(IConfiguration config, ILogger<RedSocialLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<RedSocialRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string sql = @"
                INSERT INTO dimension.dim_red_social (nombre, fuente_key)
                SELECT @nombre, f.fuente_key
                FROM dimension.dim_fuente_datos f
                WHERE f.tipo_fuente = 'Red Social'
                ORDER BY f.fuente_key
                LIMIT 1
                ON CONFLICT (nombre, fuente_key) DO NOTHING;
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
                "Se insertaron/aseguraron {Count} filas en dimension.dim_red_social",
                count
            );
        }
    }
}
