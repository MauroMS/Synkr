namespace CloudSynkr.Models;

public record SyncBackup
{
    public List<Mapping> Mappings { get; set; } = [];
}