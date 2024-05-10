namespace TestRunnerWebApi
{
    public class TestImportInfo
    {
        public string TmsUrl { get; set; }
        public string ProjectId { get; set; }
        public string ConfigurationId { get; set; }
        public string TestRunName { get; set; }
        public string ResultsDirectory { get; set; }
        public string PrivateToken { get; set; }
    }
}
