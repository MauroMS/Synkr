namespace CloudSynkr.Models;

public class SyncBackup
{
    public SyncBackup()
    {
        Mappings = new List<Mapping>();
    }

    public List<Mapping> Mappings { get; set; }
}