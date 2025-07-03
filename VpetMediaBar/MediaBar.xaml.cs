using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MediaClientDataInterFace;

namespace VpetMediaBar
{
    public partial class MediaBar : INotifyPropertyChanged
    {
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
            this.DataContext = this;
            InitializeComponent();
            
            //MediaClient.MediaClient.StartServer(fullPath);

            //MessageBox.Show("FullPath: " + fullPath, "Media Server Started", MessageBoxButton.OK, MessageBoxImage.Information);

            Init();
            
            // CoverSource = new ImageSourceConverter().ConvertFromString("cover.jpg") as ImageSource;
        }

        public async void Init()
        {
            string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string fullPath = Path.Combine(dllDir, "MediaServer","MediaServer.exe");
            Resources = Application.Current.Resources;
            _vpetMediaBar._client = new MediaClient.MediaClient(fullPath);
            
            _vpetMediaBar.MW.DynamicResources.Add("MediaInfo",_vpetMediaBar._client );
            await  _vpetMediaBar._client.WaitForConnectionAsync();
            cts = new CancellationTokenSource();
             _vpetMediaBar._client.StartListeningAsync(cts.Token);
            
             _vpetMediaBar._client.OnMediaInfoReceived += SetMediaInfo;
        }
        
        public void SetMediaInfo(MediaPropertiesSerializableData mediaInfo)
        {
            if (mediaInfo == null)
                return;
            
            Dispatcher.Invoke(() => { 
                    Title.Text = mediaInfo.Title;
                    Info.Text = mediaInfo.Subtitle+" / "+ mediaInfo.Artist;
                    Program.Text = mediaInfo.SourceUserModeId;

                    if (mediaInfo.ThumbnailBase64 != "" )
                    {
                        SetCover(mediaInfo.ThumbnailBase64);
                    }
                }
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
            await  _vpetMediaBar._client.SentNextAsync();
        }
        
        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            await  _vpetMediaBar._client.SentPrevAsync();
        }
        
        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await  _vpetMediaBar._client.SentStopOrStartAsync();
        }

        public void End()
        {
             _vpetMediaBar._client?.SentStopAsync();
             _vpetMediaBar._client?.Dispose();
            cts?.Cancel();
            this.Close();
        }
    }
}