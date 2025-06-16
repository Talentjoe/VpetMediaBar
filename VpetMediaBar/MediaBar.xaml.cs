using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static MediaClient;

namespace VpetMediaBar
{
    public partial class MediaBar : Window 
    {
        MediaClient _client;
        VpetMediaBar _vpetMediaBar;
        public CancellationTokenSource cts;
        
        private ImageSource _coverSource;
        
        public ImageSource CoverSource
        {
            get => _coverSource;
            set
            {
                _coverSource = value;
                OnPropertyChanged(nameof(CoverSource));
            }
        }

        public MediaBar(VpetMediaBar vpetMediaBar )
        {
            _vpetMediaBar = vpetMediaBar;
            InitializeComponent();
            Init();
            
            // CoverSource = new ImageSourceConverter().ConvertFromString("cover.jpg") as ImageSource;
        }

        public async void Init()
        {
            Resources = Application.Current.Resources;
            _client = new MediaClient();
            await _client.WaitForConnectionAsync();
            cts = new CancellationTokenSource();
            _client.StartListeningAsync(cts.Token);
            
            _client.OnMediaInfoReceived += SetMediaInfo;
            
        }
        
        public void SetMediaInfo(MediaInfo mediaInfo)
        {
            if (mediaInfo == null)
                return;

            Dispatcher.Invoke(() => { 
                    Title.Text = mediaInfo.Title.Replace("Title: ","");
                    Info.Text = mediaInfo.Artist.Replace("Artist: ","") +" / "+ mediaInfo.Album.Replace("Album: ","");
                
            if (mediaInfo.ThumbnailSize > 0)
            {
                SetCover(mediaInfo.ThumbnailBase64.Replace("Thumbnail Base64: ",""));
            }}
            );
            
        }
        
        public void SetCover(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                CoverSource = null;
                return;
            }
            try
            {
                byte[] imageBytes = System.Convert.FromBase64String(base64);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    
                    CoverSource = bitmap;
                }
            }
            catch
            {
                CoverSource = null;
            }
        }
        
        private void MediaBar_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            await _client.SentNextAsync();
        }
        
        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            await _client.SentPrevAsync();
        }
        
        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await _client.SentStopOrStartAsync();
        }

        public void End()
        {
            _client?.SentStopAsync();
            _client?.Dispose();
            cts?.Cancel();
            this.Close();
        }
    }
}