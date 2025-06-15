using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace VpetMediaBar
{
    public partial class MediaBar : UserControl, INotifyPropertyChanged
    {
        VpetMediaBar _vpetMediaBar;
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
            InitializeComponent();
            this.DataContext = this;
            _vpetMediaBar = vpetMediaBar;
            

            // CoverSource = new ImageSourceConverter().ConvertFromString("cover.jpg") as ImageSource;
        }

        public void SetCover(string path)
        {
            try
            {
                CoverSource = new ImageSourceConverter().ConvertFromString(path) as ImageSource;
            }
            catch
            {
                CoverSource = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}