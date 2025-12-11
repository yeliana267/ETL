namespace ETL.Worker.Config;

public class SourceSettings
{
    public string BasePath { get; set; } = "Data";
    public string SurveysCsv { get; set; } = "surveys_part1.csv";
    public string WebReviewsCsv { get; set; } = "web_reviews.csv";
    public string SocialCommentsCsv { get; set; } = "social_comments.csv";
    public string ProductsCsv { get; set; } = "products.csv";
    public string ClientsCsv { get; set; } = "clients.csv";
    public string FuenteDatosCsv { get; set; } = "fuente_datos.csv";
}
