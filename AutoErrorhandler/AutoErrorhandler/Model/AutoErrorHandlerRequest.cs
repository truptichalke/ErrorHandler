namespace AutoErrorhandler.Model
{
    public class AutoErrorHandlerRequest
    {
        public int Id { get; set; }
        public int fkProjectid { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public string Filename { get; set; }
        public string ProjectName { get; set; }
        public List<ErrorDetail> Error { get; set; }
        //public string? Understand { get; set; }
        //public string? Solution { get; set; }
        public string language { get; set; }
    }
}
