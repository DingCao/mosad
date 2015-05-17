using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace homework_win8_week3
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<Picture> mylist;

        public Rect WindowRect = Window.Current.Bounds;

        public MainPage()
        {
            this.InitializeComponent();

            // 在主界面消息循环中添加窗口大小改变事件。
            Window.Current.SizeChanged += sizechanged;

            List<Picture> actorsList = new List<Picture>()
            {
                new Picture("AYANO","Assets/阳炎/AYANO.jpg"),
                new Picture("ENE","Assets/阳炎/ENE.jpg"),
                new Picture("HIBIYA+HIYOLI","Assets/阳炎/HIBIYA_HIYOLI.jpg"),
                new Picture("KANO","Assets/阳炎/KANO.jpg"),
                new Picture("KIDO","Assets/阳炎/KIDO.jpg"),
                new Picture("KONOHA","Assets/阳炎/KONOHA.jpg"),
                new Picture("MARI","Assets/阳炎/MARI.jpg"),
                new Picture("MOMO","Assets/阳炎/MOMO.jpg"),
                new Picture("SETO","Assets/阳炎/SETO.jpg"),
                new Picture("SHINTALO","Assets/阳炎/SHINTALO.jpg"),
            };

            mylist = actorsList;

            this.grid.ItemsSource = mylist;
            this.list.ItemsSource = mylist;
        }

        // 视图切换函数
        public void sizechanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs edge)
        {
            double width = edge.Size.Width;
            double height = edge.Size.Height;

            if (width <= height)
            {
                list.SelectedIndex = grid.SelectedIndex;
                grid.Visibility = Visibility.Collapsed;
                list.Visibility = Visibility.Visible;
            }
            else
            {
                grid.SelectedIndex = list.SelectedIndex;
                grid.Visibility = Visibility.Visible;
                list.Visibility = Visibility.Collapsed;
            }
        }

    }
}
