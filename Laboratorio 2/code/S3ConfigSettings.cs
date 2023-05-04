namespace S3Operations
{
    public class S3ConfigSettings
    {
        public string BucketName { get; set; }
        public string ObjectName { get; set; }
        public string SourceFileExtension { get; set; }
        public string SourceContentType { get; set; }
        public string ProcessedFileExtension { get; set; }
        public string ProcessedContentType { get; set; }
        public string MetadataKey { get; set; }
        public string MetadataValue { get; set; }
    }
}
