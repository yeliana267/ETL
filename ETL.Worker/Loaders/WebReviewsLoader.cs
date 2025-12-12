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
    public class WebReviewsLoader : IWebReviewsLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<WebReviewsLoader> _logger;

        public WebReviewsLoader(IConfiguration config, ILogger<WebReviewsLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<WebReviewRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            // 👀 OJO: aquí estabas truncando surveys_part1, eso es otro staging.
            const string deleteSql = "TRUNCATE TABLE staging.web_reviews;";

            await using (var deleteCmd = new NpgsqlCommand(deleteSql, conn, tx))
            {
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            const string sql = @"
                INSERT INTO staging.web_reviews (
                    idreview,
                    idcliente,
                    idproducto,
                    fecha,
                    comentario,
                    rating
                )
                VALUES (
                    @idreview,
                    @idcliente,
                    @idproducto,
                    @fecha,
                    @comentario,
                    @rating
                );";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@idreview", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@idcliente", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@idproducto", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@fecha", NpgsqlDbType.Date);
            cmd.Parameters.Add("@comentario", NpgsqlDbType.Text);
            cmd.Parameters.Add("@rating", NpgsqlDbType.Integer);

            var count = 0;

            foreach (var r in records)
            {
                cmd.Parameters["@idreview"].Value = r.IdReview;
                cmd.Parameters["@idcliente"].Value = r.IdCliente;
                cmd.Parameters["@idproducto"].Value = r.IdProducto;
                cmd.Parameters["@fecha"].Value = r.Fecha.Date;
                cmd.Parameters["@comentario"].Value =
                    string.IsNullOrWhiteSpace(r.Comentario)
                        ? DBNull.Value
                        : r.Comentario;
                cmd.Parameters["@rating"].Value = r.Rating;

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                count++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Se guardaron {Count} registros en staging.web_reviews",
                count
            );
        }
    }
}
