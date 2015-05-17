using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// using Windows.Data.Json;
// 由于淘宝接口返回的并不是标准的json字符串，故只能用文本截取

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace homework_win8_week7 {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class MainPage: Page {

    // Url 访问
    private string getUrl = "http://tcc.taobao.com/cc/json/mobile_tel_segment.htm?tel={0}";


    public MainPage() {
      this.InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      // 清空内容
      phoneNumber.Text = "";
      From.Text = "";
      // 获取内容
      Get(Input.Text);
    }

    private async void Get(string phoneNum) {
      try {
        if (Input.Text.Length != 11) {
          status.Text = "请输入正确的电话号码";
          return;
        }
        status.Text = "Waiting for response ...";
        HttpClient httpClient = new HttpClient();
        var headers = httpClient.DefaultRequestHeaders;
        // 添加HTTP头
        headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.2; WOW64; rv:25.0) Gecko/20100101 Firefox/25.0");
        // 根据用户输入的phoneNum来进行字符串格式化，变量getUrl在上面可以找到
        string url = string.Format(getUrl, phoneNum);
        // 使用get方法请求url
        HttpResponseMessage response = await httpClient.GetAsync(url);
        // 响应失败将抛出异常
        response.EnsureSuccessStatusCode();
        status.Text = response.StatusCode + " " + response.ReasonPhrase + Environment.NewLine;

        // 获取返回内容
        string data = await response.Content.ReadAsStringAsync();

        // 获取手机号
        string phone = "telString";
        int phoneInedex = data.IndexOf(phone);
        for (int i = phoneInedex + phone.Length + 2; data[i] != '\''; i++) {
          phoneNumber.Text += data[i];
        }
        // 获取归属地
        string from = "carrier";
        int fromIndex = data.IndexOf(from);
        for (int i = fromIndex + from.Length + 2; data[i] != '\''; i++) {
          From.Text += data[i];
        }
      } catch (HttpRequestException hre) {
        status.Text = hre.ToString();
      } catch (Exception ex) {
        status.Text = ex.ToString();
      }
    }
  }
}
