using MediaClientDataInterFace;
using Windows.Media.Control;

namespace MediaServer;

public class PlayBackInfoSerializableDataGenerator : PlayBackInfoSerializableData
{
    PlayBackInfoSerializableDataGenerator(GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
    {
        var autoRepeatMode =
            playbackInfo.AutoRepeatMode.GetValueOrDefault(Windows.Media.MediaPlaybackAutoRepeatMode.None);
        AutoRepeatMode =
            (autoRepeatMode == Windows.Media.MediaPlaybackAutoRepeatMode.None)
                ? PlayBackInfoSerializableData.MediaPlaybackAutoRepeatMode.None
                : (autoRepeatMode == Windows.Media.MediaPlaybackAutoRepeatMode.Track)
                    ? PlayBackInfoSerializableData.MediaPlaybackAutoRepeatMode.Track
                    : PlayBackInfoSerializableData.MediaPlaybackAutoRepeatMode.List;


        Controls = new SessionPlaybackControlsGenerator(playbackInfo.Controls);
        IsShuffleActive = playbackInfo.IsShuffleActive.GetValueOrDefault(false);
        PlaybackRate = playbackInfo.PlaybackRate.GetValueOrDefault(1.0);
        
        var playbackStatus = playbackInfo.PlaybackStatus;
        switch (playbackStatus)
        {
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed:
                PlaybackStatus = PlayBackInfoSerializableData.PlaybackStatusSerializable.Closed;
                break;
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Opened:
                PlaybackStatus = PlayBackInfoSerializableData.PlaybackStatusSerializable.Opened;
                break;
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing:
                PlaybackStatus = PlayBackInfoSerializableData.PlaybackStatusSerializable.Changing;
                break;
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped:
                PlaybackStatus = PlayBackInfoSerializableData.PlaybackStatusSerializable.Stopped;
                break;
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                PlaybackStatus = PlayBackInfoSerializableData.PlaybackStatusSerializable.Playing;
                break;
            case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
                PlaybackStatus = PlayBackInfoSerializableData.PlaybackStatusSerializable.Paused;
                break;
            default:
                PlaybackStatus = PlayBackInfoSerializableData.PlaybackStatusSerializable.Closed; // Default case
                break;
        }
        
        var playbackType = playbackInfo.PlaybackType;
        switch (playbackType)
        {
            case Windows.Media.MediaPlaybackType.Unknown:
                PlaybackType = PlayBackInfoSerializableData.MediaPlaybackType.Unknown;
                break;
            case Windows.Media.MediaPlaybackType.Music:
                PlaybackType = PlayBackInfoSerializableData.MediaPlaybackType.Music;
                break;
            case Windows.Media.MediaPlaybackType.Video:
                PlaybackType = PlayBackInfoSerializableData.MediaPlaybackType.Video;
                break;
            case Windows.Media.MediaPlaybackType.Image:
                PlaybackType = PlayBackInfoSerializableData.MediaPlaybackType.Image;
                break;
            default:
                PlaybackType = PlayBackInfoSerializableData.MediaPlaybackType.Unknown;
                break;
        }
    }
}

public class SessionPlaybackControlsGenerator : PlayBackInfoSerializableData.SessionPlaybackControls
{
    public SessionPlaybackControlsGenerator( GlobalSystemMediaTransportControlsSessionPlaybackControls controls)
    {
        IsChannelDownEnabled = controls.IsChannelDownEnabled;
        IsChannelUpEnabled = controls.IsChannelUpEnabled;
        IsFastForwardEnabled = controls.IsFastForwardEnabled;
        IsNextEnabled = controls.IsNextEnabled;
        IsPauseEnabled = controls.IsPauseEnabled;
        IsPlayEnabled = controls.IsPlayEnabled;
        IsPlayPauseToggleEnabled = controls.IsPlayPauseToggleEnabled;
        IsPlaybackPositionEnabled = controls.IsPlaybackPositionEnabled;
        IsPlaybackRateEnabled = controls.IsPlaybackRateEnabled;
        IsPreviousEnabled = controls.IsPreviousEnabled;
        IsRecordEnabled = controls.IsRecordEnabled;
        IsRepeatEnabled = controls.IsRepeatEnabled;
        IsRewindEnabled = controls.IsRewindEnabled;
        IsShuffleEnabled = controls.IsShuffleEnabled;
        IsStopEnabled = controls.IsStopEnabled;
    }
}