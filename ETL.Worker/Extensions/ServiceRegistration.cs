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
            //Loaders
            services.AddSingleton<ISurveyLoader, SurveyLoader>();
            services.AddSingleton<IWebReviewsLoader, WebReviewsLoader>();
            services.AddSingleton<ISocialCommentsLoader, SocialCommentsLoader>();
            services.AddSingleton<IClientLoader, ClientLoader>();
            services.AddSingleton<IProductLoader, ProductLoader>();
            services.AddSingleton<IFuenteDatosLoader, FuenteDatosLoader>();
            services.AddSingleton<ITimeLoader, TimeLoader>();
            services.AddSingleton<IClasificacionOpinionLoader, ClasificacionOpinionLoader>();        services.AddSingleton<IRedSocialLoader, RedSocialLoader>();
            services.AddSingleton<IRedSocialLoader, RedSocialLoader>();
            services.AddSingleton<IFactOpinionesLoader, FactOpinionesLoader>();


            //Extractors
            services.AddSingleton<IExtractor, CsvSurveyExtractor>();
            services.AddSingleton<IExtractor, CsvWebReviewsExtractor>();
            services.AddSingleton<IExtractor, CsvSocialCommentsExtractor>();
            services.AddSingleton<IExtractor, CsvClientsExtractor>();
            services.AddSingleton<IExtractor, CsvProductsExtractor>();
            services.AddSingleton<IExtractor, CsvFuenteDatosExtractor>();
            services.AddSingleton<IExtractor, TimeGeneratorExtractor>();
            services.AddSingleton<IExtractor, ClasificacionOpinionGeneratorExtractor>();
            services.AddSingleton<IExtractor, RedSocialGeneratorExtractor>();
            services.AddSingleton<IFactOpinionesLoader, FactOpinionesLoader>();


            return services;
        }
    }
}
