namespace DynamoDBOperations
{
    public class DynamoDBConfigSettings
    {
        public string TableName { get; set; }
        public string PartitionKey { get; set; }
        public string SortKey { get; set; }
        public long ReadCapacity { get; set; }
        public long WriteCapacity { get; set; }
        public string Sourcenotes { get; set; }
        public string QueryUserId { get; set; }
        public int PageSize { get; set; }
        public int QueryNoteId { get; set; }
        public string NotePrefix { get; set; }
    }
}
