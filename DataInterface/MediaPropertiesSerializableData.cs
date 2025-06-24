namespace DataInterface;

public interface MediaPropertiesSerializableData
{
    public string SourceUserModeId { get; init; }
    public string Title { get; init;  }
    public string Subtitle { get; init;  }
    public string AlbumTitle { get; init;  }
    public string AlbumArtist { get; init;  }
    public int AlbumTrackCount { get; init;  }
    public string Artist { get; init;  }
    public string[] Genres { get;  init; }
    public string PlaybackType { get; init;  }
    public int TrackNumber { get; init;  }

    public bool HaveThumbnail { get; init;  }
    public string ThumbnailBase64 { get; init;  }
    
}