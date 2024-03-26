namespace CloudSynkr.Models;

public class Mapping
{
    public string CloudFolderParentId { get; set; }
    public string CloudFolderParentName { get; set; }
    public string LocalFolder { get; set; }
    public string CloudFolder { get; set; }
    public BackupActionType ActionType { get; set; }
}