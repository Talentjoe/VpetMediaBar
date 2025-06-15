using Windows.Devices.Pwm;
using Windows.Media;
using Windows.Media.Control;


namespace VpetMediaBar;

public class MediaControlCore
{
    private GlobalSystemMediaTransportControlsSessionManager _transportControl;
    private  GlobalSystemMediaTransportControlsSessionMediaProperties _currentMediaProperties;
    public GlobalSystemMediaTransportControlsSessionMediaProperties CurrentMediaProperties 
    {
        get => _currentMediaProperties;
    }
    public Action<GlobalSystemMediaTransportControlsSessionMediaProperties> MediaPropertiesChanged;
    
    public MediaControlCore()
    {
        Init();
    }
    
    private async void Init()
    {
        _transportControl = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        _transportControl.SessionsChanged += UpdateSession;
        var session = _transportControl.GetCurrentSession();
        session.MediaPropertiesChanged += UpdateMediaProperties;
    }
    
    private async void UpdateMediaProperties(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs e)
    {
        var mediaProperties = await sender.TryGetMediaPropertiesAsync();
        _currentMediaProperties = mediaProperties;
        MediaPropertiesChanged?.Invoke(mediaProperties);
        Console.WriteLine("media properties changed");
        Console.WriteLine($"Title: {mediaProperties.Title}, Artist: {mediaProperties.Artist}, Album: {mediaProperties.AlbumTitle}");
    }
    private async void UpdateSession(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs e)
    {
        var session = _transportControl.GetCurrentSession();
        session.MediaPropertiesChanged += UpdateMediaProperties;
        var mediaProperties = await session.TryGetMediaPropertiesAsync();
        _currentMediaProperties = mediaProperties;
        Console.WriteLine("session changed");
    }

}