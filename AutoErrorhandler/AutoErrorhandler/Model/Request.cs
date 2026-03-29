namespace AutoErrorhandler.Model
{
    public class Request
    {
        public int Id { get; set; }
        public int fkProjectid { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public string Filename { get; set; }
        public string ProjectName { get; set; }
        public string language { get; set; }
    }
}
