using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using ETL.Worker.Models;

namespace ETL.Worker.Loaders
{
    public class SurveyLoader : ISurveyLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<SurveyLoader> _logger;

        public SurveyLoader(IConfiguration config, ILogger<SurveyLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<SurveyRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string sql = @"
                INSERT INTO staging.surveys_part1
                (
                    idopinion,
                    idcliente,
                    idproducto,
                    fecha,
                    comentario,
                    clasificacion,
                    puntajesatisfaccion,
                    fuente
                )
                VALUES
                (
                    @idopinion,
                    @idcliente,
                    @idproducto,
                    @fecha,
                    @comentario,
                    @clasificacion,
                    @puntajesatisfaccion,
                    @fuente
                );";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@idopinion", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@idcliente", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@idproducto", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@fecha", NpgsqlDbType.Date);
            cmd.Parameters.Add("@comentario", NpgsqlDbType.Text);
            cmd.Parameters.Add("@clasificacion", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@puntajesatisfaccion", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@fuente", NpgsqlDbType.Varchar);

            var count = 0;

            foreach (var r in records)
            {
                cmd.Parameters["@idopinion"].Value = r.IdOpinion;
                cmd.Parameters["@idcliente"].Value = r.IdCliente;
                cmd.Parameters["@idproducto"].Value = r.IdProducto;
                cmd.Parameters["@fecha"].Value = r.Fecha.Date;
                cmd.Parameters["@comentario"].Value =
                    string.IsNullOrWhiteSpace(r.Comentario)
                        ? DBNull.Value
                        : r.Comentario;

                cmd.Parameters["@clasificacion"].Value =
                    string.IsNullOrWhiteSpace(r.Clasificacion)
                        ? DBNull.Value                    
                        : r.Clasificacion;

                cmd.Parameters["@puntajesatisfaccion"].Value = r.PuntajeSatisfaccion;
                cmd.Parameters["@fuente"].Value = r.Fuente;

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                count++;
            }

            await tx.CommitAsync(cancellationToken);
            _logger.LogInformation(
                "Se guardaron {Count} encuestas en staging.surveys_part1",
                count
            );
        }
    }
}
