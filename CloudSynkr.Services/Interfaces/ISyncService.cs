namespace CloudSynkr.Services.Interfaces;

public interface ISyncService
{
    Task<bool> Run(CancellationToken cancellationToken);
    
}