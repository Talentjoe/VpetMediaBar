using System.Windows;
using System.Windows.Controls;
using VPet_Simulator;
using VPet_Simulator.Windows.Interface;

namespace VpetMediaBar;


public class VpetMediaBar : MainPlugin
{
    public override string PluginName => "VpetMediaBar";
    private MenuItem DiyMenuItem;
    MediaControlCore mediaControlCore = new MediaControlCore();
    private MediaBar mediaBar;
    
    public VpetMediaBar(IMainWindow mainWindow) : base(mainWindow)
    {
    }

    
    public async override void LoadPlugin()
    {
        DiyMenuItem = new MenuItem()
        {
            Header = "Vpet Media Bar",
            IsCheckable = true,
            IsChecked = true
        };
        DiyMenuItem.Click += (s, e) =>
        {
            if (DiyMenuItem.IsChecked)
            {
                mediaBar.Visibility = Visibility.Visible;
            }
            else
            {
                mediaBar.Visibility = Visibility.Collapsed;
            }
        };
    }
    
    public override void LoadDIY()
    {
        mediaBar = new MediaBar(this);
        mediaControlCore = new MediaControlCore();
        MW.Main.ToolBar.MenuDIY.Items.Add(DiyMenuItem);
    }
    
}