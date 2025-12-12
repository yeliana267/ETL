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
    public class ClientLoader : IClientLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<ClientLoader> _logger;

        public ClientLoader(IConfiguration config, ILogger<ClientLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<ClientRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string sql = @"
                INSERT INTO dimension.dim_clientes (
                    nk_cliente,
                    nombre,
                    email
                )
                VALUES (
                    @nk_cliente,
                    @nombre,
                    @email
                )
                ON CONFLICT (nk_cliente) DO UPDATE
                SET nombre = EXCLUDED.nombre,
                    email  = EXCLUDED.email;";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@nk_cliente", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@nombre", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@email", NpgsqlDbType.Varchar);

            var count = 0;

            foreach (var r in records)
            {
                cmd.Parameters["@nk_cliente"].Value = r.IdCliente.ToString();
                cmd.Parameters["@nombre"].Value =
                    string.IsNullOrWhiteSpace(r.Nombre) ? DBNull.Value : r.Nombre;
                cmd.Parameters["@email"].Value =
                    string.IsNullOrWhiteSpace(r.Email) ? DBNull.Value : r.Email;

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                count++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Se guardaron/actualizaron {Count} clientes en dimension.dim_clientes",
                count
            );
        }
    }
}
