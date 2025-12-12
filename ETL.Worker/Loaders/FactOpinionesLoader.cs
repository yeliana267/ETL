using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace ETL.Worker.Loaders
{
    public class FactOpinionesLoader : IFactOpinionesLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<FactOpinionesLoader> _logger;

        public FactOpinionesLoader(IConfiguration config, ILogger<FactOpinionesLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task LoadAsync(CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            _logger.LogInformation("Cargando fact.fact_opiniones…");

            await using (var truncate = new NpgsqlCommand("TRUNCATE TABLE fact.fact_opiniones;", conn, tx))
            {
                await truncate.ExecuteNonQueryAsync(cancellationToken);
            }

            string insertSql = @"
                INSERT INTO fact.fact_opiniones
                (time_key, product_key, cliente_key, fuente_key, red_social_key, clasificacion_key,
                 origen, id_origen, puntuacion, comentario, longitud_texto, tiene_texto)
                
                SELECT
                    t.time_key,
                    p.product_key,
                    c.cliente_key,
                    f.fuente_key,
                    rs.red_social_key,
                    co.clasificacion_key,
                    data.origen,
                    data.id_origen,
                    data.puntuacion,
                    data.comentario,
                    LENGTH(COALESCE(data.comentario, '')),
                    CASE WHEN data.comentario IS NULL OR data.comentario = '' THEN FALSE ELSE TRUE END
                FROM (
                    -- Surveys
                    SELECT 
                        'Survey' AS origen,
                        s.idopinion::text AS id_origen,
                        s.idproducto,
                        s.idcliente,
                        s.fuente,
                        s.fecha,
                        s.clasificacion,
                        s.puntajesatisfaccion AS puntuacion,
                        s.comentario
                    FROM staging.surveys_part1 s

                    UNION ALL

                    -- Web Reviews
                    SELECT 
                        'WebReview',
                        w.idreview AS id_origen,
                        w.idproducto,
                        w.idcliente,
                        'Web' AS fuente,
                        w.fecha,
                        NULL AS clasificacion,
                        w.rating AS puntuacion,
                        w.comentario
                    FROM staging.web_reviews w

                    UNION ALL

                    -- Social Comments
                    SELECT 
                        'SocialComment',
                        sc.idcomment AS id_origen,
                        sc.idproducto,
                        sc.idcliente,
                        sc.fuente,
                        sc.fecha,
                        NULL,
                        NULL AS puntuacion,
                        sc.comentario
                    FROM staging.social_comments sc
                ) AS data

                -- TIME DIM
                JOIN dimension.dim_time t ON t.date = data.fecha

                -- PRODUCT DIM
                JOIN dimension.dim_products p ON p.product_id = data.idproducto

                -- CLIENT DIM
                LEFT JOIN dimension.dim_clientes c ON c.nk_cliente = data.idcliente

                -- FUENTE DIM
                JOIN dimension.dim_fuente_datos f ON f.id_fuente = data.fuente

                -- RED SOCIAL DIM (solo aplica si es SocialComment)
                LEFT JOIN dimension.dim_red_social rs ON rs.nombre = data.fuente

                -- CLASIFICACION DIM (solo para Survey)
                LEFT JOIN dimension.dim_clasificacion_opinion co ON co.nombre = data.clasificacion;
            ";

            await using (var cmd = new NpgsqlCommand(insertSql, conn, tx))
            {
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation("Fact cargada correctamente.");
        }
    }
}
