// See https://aka.ms/new-console-template for more information

using System.IO.Pipes;
using Windows.Devices.Pwm;
using Windows.Media;
using Windows.Media.Control;


public class MediaControlCore
{
    private GlobalSystemMediaTransportControlsSessionManager _transportControl;
    public GlobalSystemMediaTransportControlsSession CurrentSession;
    public Action<GlobalSystemMediaTransportControlsSessionMediaProperties> MediaPropertiesChanged;
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
            session.MediaPropertiesChanged += UpdateMediaProperties;
            var mediaProperties = await session.TryGetMediaPropertiesAsync();
            UpdateMediaProperty(mediaProperties);
        }
    }
    
    private async void UpdateMediaProperties(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs e)
    {
        var mediaProperties = await sender.TryGetMediaPropertiesAsync();
        UpdateMediaProperty(mediaProperties);
    }
    private async void UpdateSession(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs e)
    {
        var session = _transportControl.GetCurrentSession();
        if (session == null) return;
        CurrentSession = session;
        session.MediaPropertiesChanged += UpdateMediaProperties;
        var mediaProperties = await session.TryGetMediaPropertiesAsync();
        UpdateMediaProperty(mediaProperties);
        Console.WriteLine("session changed");
    }
    
    private void UpdateMediaProperty( GlobalSystemMediaTransportControlsSessionMediaProperties? mediaProperties)
    {
        if (mediaProperties != null)
        {
            MediaPropertiesChanged?.Invoke(mediaProperties);
            Console.WriteLine("Initial media properties:");
            Console.WriteLine($"Title: {mediaProperties.Title}, Artist: {mediaProperties.Artist}, Album: {mediaProperties.AlbumTitle}");

        }
    }

}