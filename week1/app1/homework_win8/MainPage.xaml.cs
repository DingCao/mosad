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

namespace homework_win8
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        // 声明一个委托和委托事件
        private delegate string AnimalSaying(object sender, RoutedEventArgs e);
        private event AnimalSaying toSay;

        enum animals { PIG, CAT, DOG, NULL };  // 枚举所有的动物
        Random rand = new Random();      // 随机数产生器

        // 三种小动物的接口
        interface Animal
        {
            // 方法
            string saying(object sender, RoutedEventArgs e);
        }

        // 三种小动物
        class pig : Animal
        {
            // 传递所需要调用到控件
            TextBlock Sayings;
            public pig(TextBlock s) { this.Sayings = s; }
            public string saying(object sender, RoutedEventArgs e)
            {
                this.Sayings.Text += "pig:我不是猪\n";
                return "";
            }
        }

        class cat : Animal
        {
            // 传递所需要调用到控件
            TextBlock Sayings;
            public cat(TextBlock s) { this.Sayings = s; }
            public string saying(object sender, RoutedEventArgs e)
            {
                this.Sayings.Text += "cat:我不是猫\n";
                return "";
            }
        }

        class dog : Animal
        {
            // 传递所需要调用到控件
            TextBlock Sayings;
            public dog(TextBlock s) { this.Sayings = s; }
            public string saying(object sender, RoutedEventArgs e)
            {
                this.Sayings.Text += "dog:我不是狗\n";
                return "";
            }
        }

        // 实例化三种小动物
        pig p;
        cat c;
        dog d;

        private void Say_Click(object sender, RoutedEventArgs e)
        {
            // 产生[0, 3)的随机整数来代表要抽取的动物是什么
            animals ani = (animals)rand.Next(0, 3);
            switch (ani)
            {
                case animals.PIG:
                    p = new pig(Sayings);
                    toSay = new AnimalSaying(p.saying);
                    break;
                case animals.CAT:
                    c = new cat(Sayings);
                    toSay = new AnimalSaying(c.saying);
                    break;
                case animals.DOG:
                    d = new dog(Sayings);
                    toSay = new AnimalSaying(d.saying);
                    break;
                default:
                    Sayings.Text = "";
                    return;
            }
            toSay(sender, e);
            Speaker.Text = "";
            Sayings2.ChangeView(null, Sayings.ActualHeight, null);
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            animals ani;
            // 从文本框获取动物类型
            if (Speaker.Text == "pig")
                ani = animals.PIG;
            else if (Speaker.Text == "cat")
                ani = animals.CAT;
            else if (Speaker.Text == "dog")
                ani = animals.DOG;
            else
                ani = animals.NULL;

            switch (ani)
            {
                case animals.PIG:
                    p = new pig(Sayings);
                    toSay = new AnimalSaying(p.saying);
                    break;
                case animals.CAT:
                    c = new cat(Sayings);
                    toSay = new AnimalSaying(c.saying);
                    break;
                case animals.DOG:
                    d = new dog(Sayings);
                    toSay = new AnimalSaying(d.saying);
                    break;
                default:
                    Speaker.Text = "";
                    return;
            }
            toSay(sender, e);
            Speaker.Text = "";
            Sayings2.ChangeView(null, Sayings.ActualHeight, null);
        }

    }
}
