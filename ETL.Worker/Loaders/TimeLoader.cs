using CsvHelper;
using ETL.Worker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace ETL.Worker.Loaders
{
    public class TimeLoader : ITimeLoader
    {
        private readonly string _connectionString;
        private readonly ILogger<TimeLoader> _logger;

        public TimeLoader(IConfiguration config, ILogger<TimeLoader> logger)
        {
            _connectionString = config.GetConnectionString("DW")
                ?? throw new InvalidOperationException("ConnectionStrings:DW no configurado");
            _logger = logger;
        }

        public async Task SaveAsync(IEnumerable<TimeRecord> records, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            const string sql = @"
                INSERT INTO dimension.dim_time (
                    time_key, date, year, quarter, month, month_name, day,
                    day_name, week_of_year, is_weekend
                )
                VALUES (
                    @time_key, @date, @year, @quarter, @month, @month_name,
                    @day, @day_name, @week_of_year, @is_weekend
                )
                ON CONFLICT (time_key) DO NOTHING;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@time_key", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@date", NpgsqlDbType.Date);
            cmd.Parameters.Add("@year", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@quarter", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@month", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@month_name", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@day", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@day_name", NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@week_of_year", NpgsqlDbType.Integer);
            cmd.Parameters.Add("@is_weekend", NpgsqlDbType.Boolean);

            var count = 0;

            foreach (var r in records)
            {
                cmd.Parameters["@time_key"].Value = r.TimeKey;
                cmd.Parameters["@date"].Value = r.Date;
                cmd.Parameters["@year"].Value = r.Year;
                cmd.Parameters["@quarter"].Value = r.Quarter;
                cmd.Parameters["@month"].Value = r.Month;
                cmd.Parameters["@month_name"].Value = r.MonthName;
                cmd.Parameters["@day"].Value = r.Day;
                cmd.Parameters["@day_name"].Value = r.DayName;
                cmd.Parameters["@week_of_year"].Value = r.WeekOfYear;
                cmd.Parameters["@is_weekend"].Value = r.IsWeekend;

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                count++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation("Se insertaron {Count} filas en dim_time", count);
        }
    }

}
