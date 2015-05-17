using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
using Windows.ApplicationModel.DataTransfer;  //添加的命名空间 共享会用到
using Windows.Storage;   //使用StorageFile时用到
using Windows.Storage.Pickers;  //用于打开文件选取器，以便用户选择图像和文件
using Windows.Storage.Streams;  //通常在共享图像、文件和自定义格式的数据时使用
using Windows.Graphics.Imaging;   //当在共享图像之前需要对图像进行修改时有用
using Windows.Data.Json;

namespace MvvmLight1
{
    public sealed partial class Content
    {
        private NavigationHelper navi;
        private iTimeViewModel itimeviewmodel;

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
        public Content()
        {
            this.InitializeComponent();
            this.navi = new NavigationHelper(this);
            this.navi.LoadState += navi_LoadState;
            this.navi.SaveState += navi_SaveState;

            //共享文件，截图
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
            back2.Begin();
        }

        //返回主界面
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), itimeviewmodel);
        }

        //进入编辑界面
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewDairy), itimeviewmodel);
        }

        //进入页面的重载函数，用于加载viewmodel以及读取pageitem数据
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //由App.xaml.cs导航过来后提供数据
            navi.OnNavigatedTo(e);
            itimeviewmodel = (iTimeViewModel)e.Parameter;

            //推送数据到UI
            if (itimeviewmodel.PageItem  != null) {
                ContentTitle.Text = itimeviewmodel.PageItem["title"].GetString();
                ContentDate.Text = itimeviewmodel.PageItem["date"].GetString();
                Content1.Text = itimeviewmodel.PageItem["description"].GetString();
                ContentImage.Source = new BitmapImage(new Uri("ms-appdata:///local/" + itimeviewmodel.PageItem["image"].GetString()));
            }
            else
            {
                ContentTitle.Text = "";
                ContentDate.Text = "";
                Content1.Text = "";
            }

            itimeviewmodel.PropertyChanged += (sender, e1) =>
            {
                if (e1.PropertyName == "PageItem")
                {
                    Frame.Navigate(typeof(Content), itimeviewmodel);
                }
            };

            //监听搜索事件
            SearchPane.GetForCurrentView().SuggestionsRequested += searchPane_SuggestionsRequested;
        }

        //离开页面的重载函数
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navi.OnNavigatedFrom(e);
            itimeviewmodel = (iTimeViewModel)e.Parameter;

            //去除监听
            SearchPane.GetForCurrentView().SuggestionsRequested -= searchPane_SuggestionsRequested;
        }

        //搜索事件
        void searchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            foreach (JsonObject temp in itimeviewmodel.DataItems)
            {
                //标题如果有匹配搜索词汇，加入显示结果
                string MaybeRight = temp["title"].GetString();

                if (MaybeRight != null && MaybeRight.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase))
                {
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(MaybeRight);
                }
                if (args.Request.SearchSuggestionCollection.Size > 5)
                {
                    break;
                }
                //日期如果有匹配搜索词汇，加入显示结果
                MaybeRight = temp["date"].GetString();

                if (MaybeRight != null && MaybeRight.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase))
                {
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(MaybeRight);
                }
                if (args.Request.SearchSuggestionCollection.Size > 5)
                {
                    break;
                }
                //详细记录如果有匹配，加入显示结果
                MaybeRight = temp["description"].GetString();

                if (MaybeRight != null && MaybeRight.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase))
                {
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(MaybeRight);
                }
                if (args.Request.SearchSuggestionCollection.Size > 5)
                {
                    break;
                }

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

        //共享操作
        private void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {

            var defl = args.Request.GetDeferral();
            // 设置数据包
            DataPackage dp = new DataPackage();
            dp.Properties.Title = "共享文本";
            dp.Properties.Description = "分享一些内容。";
            dp.SetText("Title:"+ ContentTitle.Text + '\n'+"Date:" + ContentDate.Text + "\nDescription:" + Content1.Text + "\n");
            args.Request.Data = dp;
            // 报告操作完成
            defl.Complete();
        }

        //共享按钮点击事件
        private void Share_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        //删除日志事件
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            //删除原记录成功再跳转到编辑页面
            if (itimeviewmodel.deleteDataItem(itimeviewmodel.PageItem["title"].GetString()) == 1)
            {
                this.Frame.Navigate(typeof(MainPage), itimeviewmodel);
            }
            //不成功提示删除失败
            else
            {
                MessageDialog dlg = new MessageDialog("还没办法删除哦！( ^＾^ ) ", "提示");
                await dlg.ShowAsync();
            }
        }
    }
}


