namespace ETL.Worker.Extractors
{
    public interface IExtractor
    {
        /// <summary>
        /// Nombre de la fuente (CSV, Database, API, etc.)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Ejecuta el proceso de extracción para esta fuente.
        /// </summary>
        Task ExtractAsync(CancellationToken cancellationToken = default);
    }
}
