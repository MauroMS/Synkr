namespace CloudSynkr.Models;

public abstract class Mapping
{
    public string CloudFolderParentId { get; set; } = "";
    public string CloudFolderParentName { get; set; } = "";
    public string LocalFolder { get; set; } = "";
    public string CloudFolder { get; set; } = "";
    public BackupActionType ActionType { get; set; }
    public List<string> FilesToSync { get; set; } = [];
}