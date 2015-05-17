using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using MvvmLight1.ViewModel;
using MvvmLight1.Common;

using Windows.ApplicationModel.DataTransfer;       //添加的命名空间 共享会用到
using Windows.Storage;                             //使用StorageFile时用到
using Windows.Storage.Pickers;                     //用于打开文件选取器，以便用户选择图像和文件
using Windows.Storage.Streams;                     //通常在共享图像、文件和自定义格式的数据时使用
using Windows.Graphics.Imaging;                    //当在共享图像之前需要对图像进行修改时有用
using Windows.Data.Json;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace MvvmLight1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewDairy
    {
        private NavigationHelper navi;
        private iTimeViewModel itimeviewmodel;
        IRandomAccessStream inputstream = null;           //读取文件流，用于显示图片和复制图片到应用数据文件夹
        string imagename = "";                            //图片名称
        
        public NavigationHelper Navi
        {
            get
            {
                return navi;
            }
        }
        public MainViewModel Vm
        {
            get
            {
                return (MainViewModel)DataContext;
            }
        }
        public NewDairy()
        {
            this.InitializeComponent();
            this.navi = new NavigationHelper(this);
            this.navi.LoadState += navi_LoadState;
            this.navi.SaveState += navi_SaveState;

            back2.Begin();
        }

        //提交按钮
        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (inputstream != null)
            {
                CopyImage(imagename);
                inputstream = null;
            }

            //新建项目加入数据列表, 成功才会提交，否则停留在原页面
            if (Title.Text != "")
            {
                string s;
                Description.Document.GetText(TextGetOptions.None, out s);
                
                //判断是否为新建项目且是否合法
                if (itimeviewmodel.PageItem["title"].GetString() == "" && itimeviewmodel.addDataItem(Title.Text, Date.Date.ToString("yyyy-MM-dd hh:mm:ss"), s, imagename) == 1)
                {
                    this.Frame.Navigate(typeof(MainPage), itimeviewmodel);
                    return;
                }
                //判断编辑后的日志是否合法
                else if (itimeviewmodel.editDataItem(Title.Text, Date.Date.ToString("yyyy-MM-dd hh:mm:ss"), s, imagename) == 1)
                {
                    this.Frame.Navigate(typeof(MainPage), itimeviewmodel);
                    return;
                }
        }

        //编辑或创建页面不成功，显示提示信息
        MessageDialog dlg = new MessageDialog("标题之前已经建立过或不可为空，请重新命名！＼( ^▽^ )／", "提示");
        await dlg.ShowAsync();
    }

        //返回操作
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), itimeviewmodel);
        }

        //进入页面
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //由App.xaml.cs导航过来后提供数据
            navi.OnNavigatedTo(e);
            itimeviewmodel = (iTimeViewModel)e.Parameter;
            
            //读取选中的记录显示出来
            if (itimeviewmodel.PageItem != null) {
                 //推送pageitem数据到UI
                Title.Text = itimeviewmodel.PageItem["title"].GetString();
                Date.Date = Convert.ToDateTime(itimeviewmodel.PageItem["date"].GetString());
                Description.Document.SetText(Windows.UI.Text.TextSetOptions.None, itimeviewmodel.PageItem["description"].GetString());
                NewImage.Source = new BitmapImage(new Uri("ms-appdata:///local/" + itimeviewmodel.PageItem["image"].GetString()));
                imagename = itimeviewmodel.PageItem["image"].GetString();
            }

            itimeviewmodel.PropertyChanged += (sender, e1) =>
            {
                if (e1.PropertyName == "PageItem")
                {
                    Frame.Navigate(typeof(Content), itimeviewmodel);
                }
            };
        }

        //离开页面
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navi.OnNavigatedFrom(e);
            itimeviewmodel = (iTimeViewModel)e.Parameter;
        }

        //选取图像
        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            ///创建一个WriteableBitmap对象，它的作用是提供了一个可写入并更新的BitmapSource
            WriteableBitmap writeAbleBitmap = new WriteableBitmap(172, 129);
            ///创建一个FileOpenPicker对象
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".tiff");
            ///file被创建来保存我们选择的图片
            StorageFile imgFile = await openPicker.PickSingleFileAsync();
            if (imgFile != null)
            {
                imagename = imgFile.Name;
                ///以只读的方式打开选定的文件
                inputstream = await imgFile.OpenAsync(FileAccessMode.Read);
                ///通过访问流来设置源图像
                await writeAbleBitmap.SetSourceAsync(inputstream);
                NewImage.Source = writeAbleBitmap;
            }
        }

        //复制上传的图像到应用数据文件夹，方便读取
        private async void CopyImage(string filename)
        {
            //读文件

            Stream input = WindowsRuntimeStreamExtensions.AsStreamForRead(inputstream.GetInputStreamAt(0));

            try
            {
                //要写入Assets文件夹的图片和名称
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile outputFile = null;
                if (folder != null)
                {
                    //指定目录下创建文件
                    outputFile = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                    //复制
                    using (IRandomAccessStream outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        Stream output = WindowsRuntimeStreamExtensions.AsStreamForWrite(outputStream.GetOutputStreamAt(0));
                        await input.CopyToAsync(output);
                        output.Dispose();
                        input.Dispose();
                    }
                }
            }
            catch(Exception)
            {
            }
        }
        private void navi_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        private void navi_SaveState(object sender, SaveStateEventArgs e)
        {
        }
        protected override void LoadState(object state)
        {
            var casted = state as MainPageState;

            if (casted != null)
            {
                Vm.Load(casted.LastVisit);
            }
        }

        protected override object SaveState()
        {
            return new MainPageState
            {
                LastVisit = DateTime.Now
            };
        }

    }
}
