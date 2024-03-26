namespace CloudSynkr.Models.Extensions;

public static class GoogleFileExtensions
{
    public static File MapFile(this Google.Apis.Drive.v3.Data.File file)
    {
        return new File()
        {
            Name = file.Name,
            Id = file.Id,
            ParentId = file.Parents[0] ?? null,
            LastModified = file.ModifiedTimeDateTimeOffset,
            MimeType = file.MimeType
        };
    }
    
    public static Folder MapFolder(this Google.Apis.Drive.v3.Data.File folder)
    {
        return new Folder()
        {
            Name = folder.Name,
            Id = folder.Id,
            ParentId = folder.Parents[0],
            Type = FileType.Folder
        };
    }
}