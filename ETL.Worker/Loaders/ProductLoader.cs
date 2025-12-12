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
    public class ProductLoader : IProductLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<ProductLoader> _logger;

        public ProductLoader(IConfiguration config, ILogger<ProductLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<ProductRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string sql = @"
                INSERT INTO dimension.dim_products (
                    product_id,
                    nombre,
                    category_name
                )
                VALUES (
                    @product_id,
                    ARRAY[@nombre],
                    ARRAY[@category_name]
                )
                ON CONFLICT (product_id) DO UPDATE
                SET nombre = EXCLUDED.nombre,
                    category_name = EXCLUDED.category_name;";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@product_id", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@nombre", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@category_name", NpgsqlDbType.Varchar);

            var count = 0;

            foreach (var r in records)
            {
                cmd.Parameters["@product_id"].Value = r.IdProducto;
                cmd.Parameters["@nombre"].Value =
                    string.IsNullOrWhiteSpace(r.Nombre) ? DBNull.Value : r.Nombre;
                cmd.Parameters["@category_name"].Value =
                    string.IsNullOrWhiteSpace(r.Categoria) ? DBNull.Value : r.Categoria;

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                count++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Se guardaron/actualizaron {Count} productos en dimension.dim_products",
                count
            );
        }
    }
}
