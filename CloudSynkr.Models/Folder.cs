namespace CloudSynkr.Models;

public class Folder
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Id { get; set; } = "";
    public string ParentId { get; set; } = "";
    public string Type { get; set; } = "";
    public List<Folder> Children { get; set; } = [];
    public List<File> Files { get; set; } = [];
}