namespace CloudSynkr.Models;

public class File
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string ParentId { get; set; }
    public string MimeType { get; set; }
    public string Id { get; set; }
    public DateTime LastModified { get; set; }
}