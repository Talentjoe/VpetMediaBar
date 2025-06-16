using System.Windows;
using System.Windows.Controls;
using VPet_Simulator;
using VPet_Simulator.Windows.Interface;

namespace VpetMediaBar;


public class VpetMediaBar : MainPlugin
{
    public override string PluginName => "VpetMediaBar";
    private MenuItem DiyMenuItem;
    //MediaControlCore mediaControlCore = new MediaControlCore();
    public MediaBar MediaBar;
    
    public VpetMediaBar(IMainWindow mainWindow) : base(mainWindow)
    { }

    
    public async override void LoadPlugin()
    {
        Version osVersion = Environment.OSVersion.Version;
        if (osVersion.Major < 10)
            return;
        
        DiyMenuItem = new MenuItem()
        {
            Header = "Vpet Media Bar",
            IsCheckable = true,
            IsChecked = true
        };
        DiyMenuItem.Click += (sender, args) =>
        {
            if (DiyMenuItem.IsChecked)
            {
                MediaBar.Visibility = Visibility.Visible;
            }
            else
            {
                MediaBar.Visibility = Visibility.Collapsed;
            }
        };
        
    }
    
    public override void LoadDIY()
    {
        Version osVersion = Environment.OSVersion.Version;
        if (osVersion.Major < 10)
        {
            MessageBox.Show("VpetMediaBar requires Windows 10 or later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        MediaBar = new MediaBar(this);
        MediaBar.Show();
    //  mediaControlCore = new MediaControlCore();
        MW.Main.ToolBar.MenuDIY.Items.Add(DiyMenuItem);
    }

    public override void EndGame()
    {
        MediaBar.End();
    }
}