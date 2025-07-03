namespace MediaClientDataInterFace;

public class PlayBackInfoSerializableData
{
    public MediaPlaybackAutoRepeatMode AutoRepeatMode { get; init; }
    public SessionPlaybackControls Controls { get; init; }
    public bool IsShuffleActive { get; init; }
    public double PlaybackRate { get; init; }
    public PlaybackStatusSerializable PlaybackStatus { get; init; }
    public MediaPlaybackType PlaybackType { get; init; }
    
    public enum MediaPlaybackAutoRepeatMode
    {
        /// <summary>No repeating.</summary>
        None,
        /// <summary>Repeat the current track.</summary>
        Track,
        /// <summary>Repeat the current list of tracks.</summary>
        List,
    }
    
    public class SessionPlaybackControls
    {
        public bool IsChannelDownEnabled { get; init; }
        public bool IsChannelUpEnabled { get; init; }
        public bool IsFastForwardEnabled { get; init; }
        public bool IsNextEnabled { get; init; }
        public bool IsPauseEnabled { get; init; }
        public bool IsPlayEnabled { get; init; }
        public bool IsPlayPauseToggleEnabled { get; init; }
        public bool IsPlaybackPositionEnabled { get; init; }
        public bool IsPlaybackRateEnabled { get; init; }
        public bool IsPreviousEnabled { get; init; }
        public bool IsRecordEnabled { get; init; }
        public bool IsRepeatEnabled { get; init; }
        public bool IsRewindEnabled { get; init; }
        public bool IsShuffleEnabled { get; init; }
        public bool IsStopEnabled { get; init; }
    }
    
    public enum PlaybackStatusSerializable
    {
        /// <summary>The media is closed.</summary>
        Closed,
        /// <summary>The media is opened.</summary>
        Opened,
        /// <summary>The media is changing.</summary>
        Changing,
        /// <summary>The media is stopped.</summary>
        Stopped,
        /// <summary>The media is playing.</summary>
        Playing,
        /// <summary>The media is paused.</summary>
        Paused,
    }
    
    public enum MediaPlaybackType
    {
        /// <summary>The media type is unknown.</summary>
        Unknown,
        /// <summary>The media type is audio music.</summary>
        Music,
        /// <summary>The media type is video.</summary>
        Video,
        /// <summary>The media type is an image.</summary>
        Image,
    }
    
    public PlayBackInfoSerializableData() { }
}