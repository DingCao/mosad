using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 为了保证文件读取的正常运行 请加入以下库
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.ApplicationModel;
using Windows.Data.Json;              // 使用json数据格式处理
namespace MvvmLight1
{
    class iTimeViewModel : INotifyPropertyChanged
    {

        //数据格式直接用封装好的jsonobject，没有自己创建Model下的数据类

        private JsonArray dataitems;   //所有数据记录列表
        private JsonArray newestitems;    //最新的三个记录列表
        private JsonObject pageitem;    //newdiary和content页面读取的日志记录
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;  //存储程序数据和图像的文件夹
        StorageFolder dataFolder = null; 
        StorageFile dataFile = null;    //存储用到的数ToString("yyyy-MM-dd hh:mm:ss")据文件

        //初始化iTimeViewModel,用于处理数据
        public iTimeViewModel()
        {
            dataitems = new JsonArray();

            pageitem = new JsonObject();
            newestitems = new JsonArray();
            //程序开始运行时用户还没建立自己的日志，先显示前天，昨天和今天
            newestitems.Add(createnewitem("还没创建哦", DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd hh:mm:ss"), "", ""));
            newestitems.Add(createnewitem("还没创建哦", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd hh:mm:ss"), "", ""));
            newestitems.Add(createnewitem("还没创建哦", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), "", ""));

            //在程序运行中一直异步打开文件夹读取最新数据
            readFile();

        }

        //用于界面读取封装好的所有数据列表
        public JsonArray DataItems
        {
            get
            {
                return dataitems;
            }
        }

        //用于界面读取封装好的最新建立的三个日志
        public JsonArray NewestItems
        {
            get
            {
                return newestitems;
            }
        }

        //用于存储界面正在查看或编辑的日志，方便写入数据列表
        public JsonObject PageItem
        {
            get
            {
                return pageitem;
            }
        }

        // 读取文件函数
        async public void readFile()
        {
            try
            {
                // 在应用存储文件夹中创建数据。如果文件夹不存在，就新建文件夹。
                if (dataFolder == null)
                    dataFolder = await localFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);

                // 打开数据文本，如果不存在文本则新建文本。
                if (dataFile == null)
                    dataFile = await dataFolder.CreateFileAsync("data.json", CreationCollisionOption.OpenIfExists);

                // 从文件中读取字符串。
                IList<string> dataString = await FileIO.ReadLinesAsync(dataFile);

                // 将字符串转换成json数据的格式。
                foreach (string str in dataString)
                {
                    JsonObject temp = JsonObject.Parse(str);
                    dataitems.Add(temp);
                }
            }
            catch(Exception)
            {
            }
        }

        // 写入文件函数
        async public void writeFile()
        {
            try
            {
                // 在应用存储文件夹中创建数据。如果文件夹不存在，就新建文件夹。
                if (dataFolder == null)
                    dataFolder = await localFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);

                // 打开数据文本，如果不存在文本则新建文本。
                if (dataFile == null)
                    dataFile = await dataFolder.CreateFileAsync("data.json", CreationCollisionOption.OpenIfExists);

                // 转成字符串格式
                IList<string> dataString = new List<string>();
                foreach (JsonObject dait in dataitems)
                {
                    dataString.Add(dait.Stringify());
                }

                // 写入文件
                await FileIO.WriteLinesAsync(dataFile, dataString);
            }
            catch(Exception)
            {
            }
        }

        //事件属性改变，主要用于实现搜索
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(str));
            }
        }

        // 项目查找函数，主要用于搜索返回对应的页面
        public void Search(string searchTerm)
        {
            foreach (JsonObject temp in dataitems)
            {
                if (temp["title"].GetString().ToLower() == searchTerm.ToLower() ||
                       temp["description"].GetString().ToLower() == searchTerm.ToLower() ||
                           temp["date"].GetString().ToLower() == searchTerm.ToLower())
                {
                    pageitem = temp;
                    NotifyPropertyChanged("PageItem");
                }
            }
        }

        // CRUD

        //创建新的item，可用于pageitem或dataitem
        public JsonObject createnewitem(string title, string date, string desri, string image)
        {
            JsonObject temp = new JsonObject();
            temp.Add("title", JsonValue.CreateStringValue(title));
            temp.Add("date", JsonValue.CreateStringValue(date));
            temp.Add("description", JsonValue.CreateStringValue(desri));
            temp.Add("image", JsonValue.CreateStringValue(image));
            return temp;
        }

        //创建新的日志时页面暂时显示为空
        public void emptyPageItem() 
        {
            pageitem = createnewitem("", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), "", "");
        }

        // 项目插入函数
        // 返回值：0表示成功插入，1表示不成功插入
        public int addDataItem(string title, string date, string descri, string img)
        {
            // 检查是否存在相同标题的项目，如果存在，则返回异常
            foreach (JsonObject each in dataitems)
            {
                if (each["title"].GetString() == title)
                {
                    return 0;
                }
            }
            // 新建新项目
            JsonObject temp = createnewitem(title, date, descri, img);

            dataitems.Add(temp);  // 插入项目

            //一旦插入记录，写到文件中
            writeFile();
            return 1;
        }

        //项目编辑函数，修改已有项目
        public int editDataItem(string title, string date, string descri, string img)
        {
            JsonObject find = searchItem(pageitem["title"].GetString());
            if (find != null)
            {
                find["title"] = JsonValue.CreateStringValue(title);
                find["date"] =  JsonValue.CreateStringValue(date);
                find["description"] =  JsonValue.CreateStringValue(descri);
                find["image"] =  JsonValue.CreateStringValue(img);

                //一旦修改记录，写到文件中
                writeFile();
                return 1;
            }
            else 
            {
                return 0;
            }
        }

        // 通过标题来检索的函数, 仅可以返回一个
        public JsonObject searchItem(string title)
        {
            foreach (JsonObject temp in dataitems)
            {
                if (temp["title"].GetString() == title)
                {
                    return temp;
                }
            }
            return null;
        }
        // 项目删除函数
        // 把标题唯一化

        public int deleteDataItem(string title) {
            foreach (JsonObject temp in dataitems) {
                if (temp["title"].GetString() == title)
                {
                    dataitems.Remove(temp);

                    //一旦删除记录，写到文件中
                    writeFile();
                    return 1;
                }
            }
            return 0;
        }

        //更新最新的三个日志
        public void updateNewestItems()
        {
            //用户当前只有一条记录,自动补全
            if (dataitems.Count == 1)
            {
                newestitems[2] = dataitems[0];
                newestitems[1] = createnewitem("还没创建哦", Convert.ToDateTime((dataitems[0] as JsonObject)["date"].GetString()).AddDays(-1).ToString("yyyy-MM-dd hh:mm:ss"), "", "");
                newestitems[0] = createnewitem("还没创建哦", Convert.ToDateTime((dataitems[0] as JsonObject)["date"].GetString()).AddDays(-2).ToString("yyyy-MM-dd hh:mm:ss"), "", "");
            }
            //用户有两条记录，自动补全
            else if (dataitems.Count == 2)
            {
                if (Convert.ToDateTime((dataitems[0] as JsonObject)["date"].GetString()) < Convert.ToDateTime((dataitems[1] as JsonObject)["date"].GetString()))
                {
                    newestitems[2] = dataitems[1];
                    newestitems[1] = dataitems[0];
                }
                else
                {
                    newestitems[2] = dataitems[0];
                    newestitems[1] = dataitems[1];
                }
                newestitems[0] = createnewitem("还没创建哦", Convert.ToDateTime((dataitems[0] as JsonObject)["date"].GetString()).AddDays(-1).ToString("yyyy-MM-dd hh:mm:ss"), "", "");
            }
            else if (dataitems.Count >= 3)
            {
                //有时间可以优化比较
                JsonObject lowest = new JsonObject();
                lowest.Add("date", JsonValue.CreateStringValue("1900-01-01 00:00:00"));
                newestitems[2] = newestitems[1] = newestitems[0] = lowest;
                for (int i = 0; i < dataitems.Count; i++)
                {
                    if (Convert.ToDateTime((newestitems[2] as JsonObject)["date"].GetString()) < Convert.ToDateTime((dataitems[i] as JsonObject)["date"].GetString()))
                    {
                        newestitems[2] = dataitems[i];
                    }
                }
                for (int i = 0; i < dataitems.Count; i++)
                {
                    if (Convert.ToDateTime((newestitems[1] as JsonObject)["date"].GetString()) < Convert.ToDateTime((dataitems[i] as JsonObject)["date"].GetString())
                        && Convert.ToDateTime((newestitems[1] as JsonObject)["date"].GetString()) < Convert.ToDateTime((newestitems[2] as JsonObject)["date"].GetString())
                        && newestitems[2] != dataitems[i])
                    {
                        newestitems[1] = dataitems[i];
                    }
                }
                for (int i = 0; i < dataitems.Count; i++)
                {
                    if (Convert.ToDateTime((newestitems[0] as JsonObject)["date"].GetString()) < Convert.ToDateTime((dataitems[i] as JsonObject)["date"].GetString())
                        && Convert.ToDateTime((newestitems[0] as JsonObject)["date"].GetString()) < Convert.ToDateTime((newestitems[1] as JsonObject)["date"].GetString())
                        && newestitems[2] != dataitems[i] && newestitems[1] != dataitems[i])
                    {
                        newestitems[0] = dataitems[i];
                    }
                }
            }
        }

    }
}