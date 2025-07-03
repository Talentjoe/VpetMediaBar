// See https://aka.ms/new-console-template for more information

using Windows.Data.Json;
using Windows.Media.Control;


public class MediaControlCore
{
    private GlobalSystemMediaTransportControlsSessionManager _transportControl;
    public GlobalSystemMediaTransportControlsSession CurrentSession;
    public Action<GlobalSystemMediaTransportControlsSessionMediaProperties> MediaPropertiesChanged;
    public Action<GlobalSystemMediaTransportControlsSessionPlaybackInfo> PlaybackInfoChanged;
    public Action<GlobalSystemMediaTransportControlsSessionTimelineProperties> TimelinePropertiesChanged;
    

    public GlobalSystemMediaTransportControlsSessionMediaProperties GetCurrentMediaProperties()
    {
        var session = _transportControl.GetCurrentSession();
        if (session == null)
        {
            Console.WriteLine("No media session is currently active.");
            return null;
        }
        return session.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();
    }
    
    public MediaControlCore()
    {
        Init();
    }
    
    private async void Init()
    {
        _transportControl = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        if (_transportControl != null)
        {
            _transportControl.SessionsChanged += UpdateSession;
            var session = _transportControl.GetCurrentSession();
            CurrentSession = session;
            if (session == null)
            {
                Console.WriteLine("No media session is currently active.");
                return;
            }
            session.MediaPropertiesChanged += UpdateMediaPropertiesSub;
            var mediaProperties = await session.TryGetMediaPropertiesAsync();
            UpdateMediaProperty(mediaProperties);
        }
    }
    
    private async void UpdateMediaPropertiesSub(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs e)
    {
        var mediaProperties = await sender.TryGetMediaPropertiesAsync();
        UpdateMediaProperty(mediaProperties);
    }
    private void UpdateMediaProperty( GlobalSystemMediaTransportControlsSessionMediaProperties? mediaProperties)
    {
        if (mediaProperties != null)
        {
            MediaPropertiesChanged?.Invoke(mediaProperties);
            Console.WriteLine("Initial media properties:");
            Console.WriteLine($"Title: {mediaProperties.Title}, Artist: {mediaProperties.Artist}, Album: {mediaProperties.AlbumTitle}");
            //Console.WriteLine( mediaProperties);
        }
    }

    private async void UpdatePlaybackInfosSub(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs e)
    {
        var playbackInfo =  sender.GetPlaybackInfo();
        UpdatePlayBackInfo(playbackInfo);
    }    
    private void UpdatePlayBackInfo( GlobalSystemMediaTransportControlsSessionPlaybackInfo? playBackInfo)
    {
        if (playBackInfo != null)
        {
            PlaybackInfoChanged?.Invoke(playBackInfo);
            Console.WriteLine("Play Back Info:");
            Console.WriteLine($"Playback Status: {playBackInfo.PlaybackStatus}, Rate: {playBackInfo.PlaybackRate}, Type: {playBackInfo.PlaybackType}");

        }
    }
    
    private async void UpdateTimelinePropertiesSub(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs e)
    {
        var timelineProperties = sender.GetTimelineProperties();
        UpdateTimelineProperties(timelineProperties);
    }
    
    private void UpdateTimelineProperties(GlobalSystemMediaTransportControlsSessionTimelineProperties? timelineProperties)
    {
        if (timelineProperties != null)
        {
            TimelinePropertiesChanged?.Invoke(timelineProperties);
            Console.WriteLine("Timeline Properties:");
            Console.WriteLine($"Start Time: {timelineProperties.StartTime}, End Time: {timelineProperties.EndTime}, Position: {timelineProperties.Position}");
        }
    }
    
    private async void UpdateSession(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs e)
    {
        var session = _transportControl.GetCurrentSession();
        if (session == null) return;
        CurrentSession = session;
        session.MediaPropertiesChanged -= UpdateMediaPropertiesSub;
        session.MediaPropertiesChanged += UpdateMediaPropertiesSub;
        session.PlaybackInfoChanged -= UpdatePlaybackInfosSub;
        session.PlaybackInfoChanged += UpdatePlaybackInfosSub;
        session.TimelinePropertiesChanged -= UpdateTimelinePropertiesSub;
        session.TimelinePropertiesChanged += UpdateTimelinePropertiesSub;
        try
        {
            var mediaProperties = await session.TryGetMediaPropertiesAsync();
            UpdateMediaProperty(mediaProperties);
            Console.WriteLine("session changed");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error retrieving media properties: " + ex.Message);
        }
    }
    

}