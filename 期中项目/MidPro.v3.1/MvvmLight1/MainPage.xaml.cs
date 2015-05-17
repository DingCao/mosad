using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Search;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;       //添加的命名空间 共享会用到
using Windows.Storage;                             //使用StorageFile时用到
using Windows.Storage.Pickers;                     //用于打开文件选取器，以便用户选择图像和文件
using Windows.Storage.Streams;                     //通常在共享图像、文件和自定义格式的数据时使用
using Windows.Graphics.Imaging;                    //当在共享图像之前需要对图像进行修改时有用
using MvvmLight1.ViewModel;
using MvvmLight1.Common;

namespace MvvmLight1
{
    public sealed partial class MainPage
    {
        /// <summary>
        /// Gets the view's ViewModel.
        /// </summary>

        private NavigationHelper navi;
        private iTimeViewModel itimeviewmodel;
        private SearchPane searchpane;

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

        //MainPage页面初始化
        public MainPage()
        {
            InitializeComponent();
            this.navi = new NavigationHelper(this);
            this.navi.LoadState += navi_LoadState;
            this.navi.SaveState += navi_SaveState;

            //共享文件，截图
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;

           
            back.Begin();
            start.Begin();
        }

        //加载页面，读取itimeviewmodel数据
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //由APP导航过来的数据
            navi.OnNavigatedTo(e);
            itimeviewmodel = (iTimeViewModel)e.Parameter;

            //页面元素先设为空
            itimeviewmodel.emptyPageItem();
            //刷新最新的三个记录
            itimeviewmodel.updateNewestItems();

            //对应设置3个结果显示
            Day1.Content = (itimeviewmodel.NewestItems[0] as JsonObject)["date"].GetString();
            Day2.Content = (itimeviewmodel.NewestItems[1] as JsonObject)["date"].GetString();
            Day3.Content = (itimeviewmodel.NewestItems[2] as JsonObject)["date"].GetString();

            FirstTitle.Text = (itimeviewmodel.NewestItems[0] as JsonObject)["title"].GetString();
            SecondTitle.Text = (itimeviewmodel.NewestItems[1] as JsonObject)["title"].GetString();
            LastTitle.Text = (itimeviewmodel.NewestItems[2] as JsonObject)["title"].GetString();

            if(FirstTitle.Text == "还没创建哦"){
                FirstImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
            }
            else
            {
                FirstImage.Source = new BitmapImage(new Uri("ms-appdata:///local/" + (itimeviewmodel.NewestItems[0] as JsonObject)["image"].GetString()));
            }

            if (SecondTitle.Text == "还没创建哦")
            {
               SecondImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
            }
            else
            {
                SecondImage.Source = new BitmapImage(new Uri("ms-appdata:///local/" + (itimeviewmodel.NewestItems[0] as JsonObject)["image"].GetString()));
            }

            if (LastTitle.Text == "还没创建哦")
            {
                LastImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/background.jpg"));
            }
            else
            {
                LastImage.Source = new BitmapImage(new Uri("ms-appdata:///local/" + (itimeviewmodel.NewestItems[2] as JsonObject)["image"].GetString()));
            }
            
          

            //有符合的结果，跳转至content页面
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

        //离开页面时把数据传回viewmodel
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
                string MaybeRight = temp["title"].GetString();

                if (MaybeRight != null && MaybeRight.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase))
                {
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(MaybeRight);
                }
                if (args.Request.SearchSuggestionCollection.Size > 5)
                {
                    break;
                }

                MaybeRight = temp["date"].GetString();

                if (MaybeRight != null && MaybeRight.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase))
                {
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(MaybeRight);
                }
                if (args.Request.SearchSuggestionCollection.Size > 5)
                {
                    break;
                }

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

        //保存状态
        private void navi_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        //修改状态
        private void navi_SaveState(object sender, SaveStateEventArgs e)
        {
        }
        //加载状态
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
            args.Request.Data = dp;
            // 报告操作完成
            defl.Complete();
        }

        //共享按钮点击事件
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        //新建日志
        private void Newday_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewDairy), itimeviewmodel);
        }

        //搜索按钮点击事件,显示搜索面板
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            searchpane = SearchPane.GetForCurrentView();
            searchpane.Show("");
        }

        //按钮点击事件
        private void Day1_Click(object sender, RoutedEventArgs e)
        {
            //如果是已创建的日志，跳转至content页面
            if (itimeviewmodel.searchItem(FirstTitle.Text) != null)
            {
                itimeviewmodel.Search(FirstTitle.Text);
                this.Frame.Navigate(typeof(Content), itimeviewmodel);
            }
            //未创建，跳转至newdiary页面
            else
            {
                this.Frame.Navigate(typeof(NewDairy), itimeviewmodel);
            }
        }

        private void Day2_Click(object sender, RoutedEventArgs e)
        {
            if (itimeviewmodel.searchItem(SecondTitle.Text) != null)
            {
                itimeviewmodel.Search(SecondTitle.Text);
                this.Frame.Navigate(typeof(Content), itimeviewmodel);
            }
            else
            {
                this.Frame.Navigate(typeof(Content), itimeviewmodel);
            }
        }

        private void Day3_Click(object sender, RoutedEventArgs e)
        {
            if (itimeviewmodel.searchItem(LastTitle.Text) != null)
            {
                itimeviewmodel.Search(LastTitle.Text);
                this.Frame.Navigate(typeof(Content), itimeviewmodel);
            }
            else
            {
                this.Frame.Navigate(typeof(Content), itimeviewmodel);
            }
        }
    }

    //用于搜索，保存上次搜索结果
    public class MainPageState
    {
        public DateTime LastVisit
        {
            get;
            set;
        }
    }
}
