using Windows.Media.Control;
using Windows.Storage.Streams;

namespace MediaServer;

public class MediaPropertiesSerializableDataGenerator : MediaPropertiesSerializableData
{
    
    public MediaPropertiesSerializableDataGenerator(GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties,
        String sourceUserModeId = "")
    {
        SourceUserModeId = sourceUserModeId;
        Title = mediaProperties.Title;
        Subtitle = mediaProperties.Subtitle;
        AlbumTitle = mediaProperties.AlbumTitle;
        AlbumArtist = mediaProperties.AlbumArtist;
        AlbumTrackCount = mediaProperties.AlbumTrackCount;
        Artist = mediaProperties.Artist;
        Genres = mediaProperties.Genres.ToArray();
        PlaybackType = mediaProperties.PlaybackType?.ToString() ?? "Unknown";
        TrackNumber = mediaProperties.TrackNumber;

        if (mediaProperties.Thumbnail == null)
        {
            HaveThumbnail = false;
            ThumbnailBase64 = "";
            return;
        }

        try
        {
            var thumbnailStream = mediaProperties.Thumbnail.OpenReadAsync().GetAwaiter().GetResult();
            if (thumbnailStream == null)
            {
                HaveThumbnail = false;
                ThumbnailBase64 = "";
                return;
            }

            using var dataReader = new DataReader(thumbnailStream);
            uint size = (uint)thumbnailStream.Size;

            dataReader.LoadAsync(size).GetAwaiter();
            byte[] buffer = new byte[size];
            dataReader.ReadBytes(buffer);

            HaveThumbnail = true;
            ThumbnailBase64 = Convert.ToBase64String(buffer);
        }
        catch (Exception ex)
        {
            HaveThumbnail = false;
            ThumbnailBase64 = "";
            Console.WriteLine("[Thumbnail error] " + ex.Message);
        }
    }
}