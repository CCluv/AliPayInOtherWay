using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using mshtml;
using System.Media;

namespace 抓取网页内容
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string rightstr = "</p>";
        public HTMLDocument html = new HTMLDocument();
        public System.Windows.Threading.DispatcherTimer timer;
        public System.Windows.Threading.DispatcherTimer timer_refresh;
        public string Mtime, Mtime1, Mitem, MtradeNo, Mname, Mmoney, Mstatus, s, st, s1;

        public SerialPort Uart;

        // private SoundPlayer player = new SoundPlayer(System.Environment.CurrentDirectory + @"\Audio\warning.wav");
        public MainWindow()
        {
            InitializeComponent();
            this.Web.LoadCompleted += (Web_Navigated);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {


        }

        private void Web_Loaded(object sender, RoutedEventArgs e)
        {
            Web.Navigate(new Uri("https://consumeprod.alipay.com/record/advanced.htm"));

        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        { 
            if (this.ports.SelectedItem != null)
            {


                Uart.PortName = this.ports.SelectedItem.ToString();
                Uart.BaudRate = 4800;
                Uart.Parity = 0;//No Parity-Checking
                Uart.DataBits = 8;
                Uart.StopBits = StopBits.One;
               // Uart.Handshake = SetPortHandshake(Uart.Handshake); //No Protocol
                // Set the read/write timeouts
                Uart.ReadTimeout = 500;
                Uart.WriteTimeout = 500;
                Uart.Open();
                System.Windows.MessageBox.Show("Uart Initialized Success!");
            }
            else
            {
                System.Windows.MessageBox.Show("No available serialport !");
                this.ports.ItemsSource = SerialPort.GetPortNames();
           return;
            }
            Web.Visibility = System.Windows.Visibility.Collapsed;

            html = (HTMLDocument)this.Web.Document;
            if (html == null)
            {
                System.Windows.MessageBox.Show("null");
                return;
            }
            st = html.getElementById("J-item-1").innerHTML;
            timer_refresh = new System.Windows.Threading.DispatcherTimer();
            timer_refresh.Interval = new TimeSpan(0, 0, 20);   //间隔20秒
            timer_refresh.Tick += new EventHandler(refresh);
            timer_refresh.Start();

        }
        //显示web
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Web.Visibility = System.Windows.Visibility.Visible;
            //Mmoney = "+0.01";
            //this.asd.Text = float.Parse(Mmoney).ToString();

        }

        //日期:2015.06.26
        //时间:20:57
        //交易号:20150626200040011100610025461887
        //姓名:闫帅伟
        //状态:交易成功
        //金额:+100.00
        //项目:转账
        //系统时间:22:24:07
        //longtime:22:24:10
        //shorttime:22:24
        //shortdate:6月26日 星期五
        //longdate:2015年6月26日
        void timer_Tick(object sender, EventArgs e)
        {
            html = (HTMLDocument)this.Web.Document;
            st = s1;
            if (html.getElementById("J-item-1") == null)
                return;
            s1 = html.getElementById("J-item-1").innerHTML;

            if (s1 == st)
            {
                Web.Refresh();
                return;
            }
            //Web.Visibility = System.Windows.Visibility.Collapsed;
            s = s1.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace(" ", "").Replace("\"", "").ToLower();//去空格回车制表符
            Mtime = Between(s, "time-d>", rightstr);//s.Substring(i, j - i);//date
            Mtime1 = Between(s, "hft-gray>", rightstr);//time
            // MtradeNo = Between(s, "<p>流水号:", rightstr);




            if (Convert.ToDateTime(Mtime.Replace(".", "-") + " " + (Mtime1 + ":00")) > DateTime.Now.AddMinutes(-1))//.Replace("年", ".").Replace("月", ".").Replace("日", "."))
            {
                Mitem = Between(s, "racker=on>", "</a>");
                if (Mitem == "转账" || Mitem == "账户码-转账")
                {
                    Mstatus = Between(s, "status><p>", rightstr);
                    if ("交易成功" == Mstatus)
                    {
                        Mmoney = Between(s, "amount-pay-in>", "</span>");
                        if (float.Parse(Mmoney) >=1f)
                        {
                            Mname = Between(s, "<pclass=name>", rightstr);
                            CoinOut();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Only for Test:Success!\r\n"+Mmoney);
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;

            }

        }
        /// <summary>
        /// 出币函数
        /// </summary>
        private void CoinOut()
        {
            System.Windows.MessageBox.Show("name:" + Mname + " \r\n money:" + float.Parse(Mmoney)+"\r\nCoins Will Fly~");
            Uart_W(Convert.ToString(float.Parse(Mmoney)));
            timer.Stop();
        }
        /// <summary>
        /// 开始工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void start_Click(object sender, RoutedEventArgs e)
        {
            start.Visibility = System.Windows.Visibility.Hidden;
            ports.Visibility = Visibility.Hidden;
            init.Visibility = Visibility.Hidden;
            disp.Visibility = Visibility.Hidden;
            exit.Visibility = Visibility.Hidden;
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 4);   //间隔2秒
            timer.Tick += new EventHandler(timer_Tick);
            test.IsEnabled = true;
            change10.IsEnabled = true;
            change1.IsEnabled = true;
            change2.IsEnabled = true;
            change5.IsEnabled = true;
            init.IsEnabled = false;
            disp.IsEnabled = false;
            start.IsEnabled = false;

        }
        private void refresh(object sender, EventArgs e)
        {
            Web.Refresh();
        }
        private void Web_Navigated(object sender, NavigationEventArgs e)
        {

            //System.Windows.MessageBox.Show(html.getElementById("site_nav_top").innerText);
        }


        /// <summary>  
        /// 取文本左边内容  
        /// </summary>  
        /// <param name="str">文本</param>  
        /// <param name="s">标识符</param>  
        /// <returns>左边内容</returns>  
        public static string GetLeft(string str, string s)
        {
            string temp = str.Substring(0, str.IndexOf(s));
            return temp;
        }


        /// <summary>  
        /// 取文本右边内容  
        /// </summary>  
        /// <param name="str">文本</param>  
        /// <param name="s">标识符</param>  
        /// <returns>右边内容</returns>  
        public static string GetRight(string str, string s)
        {
            string temp = str.Substring(str.IndexOf(s), str.Length - str.Substring(0, str.IndexOf(s)).Length);
            return temp;
        }

        /// <summary>  
        /// 取文本中间内容  
        /// </summary>  
        /// <param name="str">原文本</param>  
        /// <param name="leftstr">左边文本</param>  
        /// <param name="rightstr">右边文本</param>  
        /// <returns>返回中间文本内容</returns>  
        public static string Between(string str, string leftstr, string rightstr)
        {
            int i = str.IndexOf(leftstr) + leftstr.Length;
            int j = str.IndexOf(rightstr, i);
            string temp = str.Substring(i, str.IndexOf(rightstr, i) - i);
            return temp;
        }


        /// <summary>  
        /// 取文本中间到List集合  
        /// </summary>  
        /// <param name="str">文本字符串</param>  
        /// <param name="leftstr">左边文本</param>  
        /// <param name="rightstr">右边文本</param>  
        /// <returns>List集合</returns>  
        public List<string> BetweenArr(string str, string leftstr, string rightstr)
        {
            List<string> list = new List<string>();
            int leftIndex = str.IndexOf(leftstr);//左文本起始位置  
            int leftlength = leftstr.Length;//左文本长度  
            int rightIndex = 0;
            string temp = "";
            while (leftIndex != -1)
            {
                rightIndex = str.IndexOf(rightstr, leftIndex + leftlength);
                if (rightIndex == -1)
                {
                    break;
                }
                temp = str.Substring(leftIndex + leftlength, rightIndex - leftIndex - leftlength);
                list.Add(temp);
                leftIndex = str.IndexOf(leftstr, rightIndex + 1);
            }
            return list;
        }


        /// <summary>  
        /// 指定文本倒序  
        /// </summary>  
        /// <param name="str">文本</param>  
        /// <returns>倒序文本</returns>  
        public static string StrReverse(string str)
        {
            char[] chars = str.ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0;i < chars.Length;i++)
            {
                sb.Append(chars[chars.Length - 1 - i]);
            }
            return sb.ToString();
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            testimg.Visibility = System.Windows.Visibility.Visible;
            testimg.Source = new BitmapImage(new Uri(@"0.01.jpg", UriKind.Relative));
            timer.Start();

        }

        private void Change1_Click(object sender, RoutedEventArgs e)
        {
            testimg.Visibility = System.Windows.Visibility.Visible;
            testimg.Source = new BitmapImage(new Uri(@"1.jpg", UriKind.Relative));
            timer.Start();
        }

        private void change2_Click(object sender, RoutedEventArgs e)
        {
            testimg.Visibility = System.Windows.Visibility.Visible;
            testimg.Source = new BitmapImage(new Uri(@"2.jpg", UriKind.Relative));
            timer.Start();
        }

        private void change10_Click(object sender, RoutedEventArgs e)
        {

            testimg.Visibility = System.Windows.Visibility.Visible;
            testimg.Source = new BitmapImage(new Uri(@"10.jpg", UriKind.Relative));
            timer.Start();
        }

        private void change5_Click_1(object sender, RoutedEventArgs e)
        {
            testimg.Visibility = System.Windows.Visibility.Visible;
            testimg.Source = new BitmapImage(new Uri(@"5.jpg", UriKind.Relative));
            timer.Start();

        }

        private void mainwin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Uart.IsOpen == true)
            {
                Uart.Close();
            }
        }

        private void mainwin_Loaded(object sender, RoutedEventArgs e)
        {
            string[] pn = SerialPort.GetPortNames();
            this.ports.ItemsSource = pn;
            Uart = new SerialPort();
            this.ports.SelectedIndex = 0;
            //player.Load();
        }
        /// <summary>
        /// 串口发命令
        /// </summary>
        /// <param name="Data">"1元","2元","5元","10元"</param>
        public void Uart_W(string Data)
        {
            Byte[] Command = { 0x1, 0x2, 0x5, 0x0A };
            switch (Data)
            {
                case "1":
                    try
                    {
                        Uart.Write(Command, 0, 1);
                    }
                    catch (System.TimeoutException)
                    {
                        System.Windows.MessageBox.Show("Error! SerialPoart write time out. ");
                        Uart.Close();
                    }
                    break;
                case "2":
                    try
                    {
                        Uart.Write(Command, 1, 1);
                    }
                    catch (System.TimeoutException)
                    {
                        System.Windows.MessageBox.Show("Error! SerialPoart write time out. ");
                        Uart.Close();
                    }
                    break;
                case "5":
                    try
                    {
                        Uart.Write(Command, 2, 1);
                    }
                    catch (System.TimeoutException)
                    {
                        System.Windows.MessageBox.Show("Error! SerialPoart write time out. ");
                        Uart.Close();
                    }
                    break;
                case "10":
                    try
                    {
                        Uart.Write(Command, 3, 1);
                    }
                    catch (System.TimeoutException)
                    {
                        System.Windows.MessageBox.Show("Error! SerialPoart write time out. ");
                        Uart.Close();
                    }
                    break;
                default:
                    System.Windows.MessageBox.Show("Error! Illegal Amount. ");
                    break;
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (timer != null)
                timer.Stop();
            if (timer_refresh != null)
                timer_refresh.Stop();
            Close();
        }


        private void fuwei(object sender, MouseButtonEventArgs e)
        {
            start.Visibility = System.Windows.Visibility.Visible;
            ports.Visibility = Visibility.Visible;
            init.Visibility = Visibility.Visible;
            disp.Visibility = Visibility.Visible;
            exit.Visibility = Visibility.Visible;
        }
    }
}
