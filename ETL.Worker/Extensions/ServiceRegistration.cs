using ETL.Worker.Config;
using ETL.Worker.Extractors;
using ETL.Worker.Loaders;
using Microsoft.Extensions.Options;

namespace ETL.Worker.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddETLServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SourceSettings>(configuration.GetSection("SourceSettings"));
            services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<SourceSettings>>().Value);

            services.AddSingleton<ISurveyLoader, SurveyLoader>();

            services.AddSingleton<IExtractor, CsvSurveyExtractor>();

            return services;
        }
    }
}
