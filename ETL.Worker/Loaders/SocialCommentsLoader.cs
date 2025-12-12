using ETL.Worker.Models;
using Npgsql;
using NpgsqlTypes;

namespace ETL.Worker.Loaders
{
    public class SocialCommentsLoader : ISocialCommentsLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<SocialCommentsLoader> _logger;

        public SocialCommentsLoader(IConfiguration config, ILogger<SocialCommentsLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<SocialCommentRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string deleteSql = "TRUNCATE TABLE staging.social_comments;";

            await using (var deleteCmd = new NpgsqlCommand(deleteSql, conn, tx))
            {
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            const string sql = @"
                INSERT INTO staging.social_comments (
                    idcomment,
                    idcliente,
                    idproducto,
                    fuente,
                    fecha,
                    comentario
                )
                VALUES (
                    @idcomment,
                    @idcliente,
                    @idproducto,
                    @fuente,
                    @fecha,
                    @comentario
                );";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@idcomment", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@idcliente", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@idproducto", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@fuente", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@fecha", NpgsqlDbType.Date);
            cmd.Parameters.Add("@comentario", NpgsqlDbType.Text);

            var count = 0;

            foreach (var r in records)
            {
                cmd.Parameters["@idcomment"].Value = r.IdComment;
                cmd.Parameters["@idcliente"].Value =
                    string.IsNullOrWhiteSpace(r.IdCliente)
                        ? DBNull.Value
                        : r.IdCliente!;
                cmd.Parameters["@idproducto"].Value = r.IdProducto;
                cmd.Parameters["@fuente"].Value = r.Fuente;
                cmd.Parameters["@fecha"].Value = r.Fecha.Date;
                cmd.Parameters["@comentario"].Value =
                    string.IsNullOrWhiteSpace(r.Comentario)
                        ? DBNull.Value
                        : r.Comentario!;

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                count++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Se guardaron {Count} registros en staging.social_comments",
                count
            );
        }
    }
}
