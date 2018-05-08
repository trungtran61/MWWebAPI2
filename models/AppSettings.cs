public class AppSettings
{
    public string MWConnectionString { get; set; }
    public string SecurityConnectionString { get; set; }
    public string JWTKey { get; set; }
    public string JWTIssuer { get; set; }
    public string JWTAudience { get; set; }
    public int JWTMinutesToExpiration { get; set; }
    public string ImageUrl { get; set; }
    public string ImageLibrary { get; set; }
    public string UnprovenProgramsPath { get; set; }
    public string ProvenProgramsPath { get; set; }
}
