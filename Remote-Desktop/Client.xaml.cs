using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using WebSocketProtocol;

namespace Remote_Desktop
{
    /// <summary>
    /// Client.xaml 的交互逻辑
    /// </summary>
    public partial class Client : Window
    {
        public Client()
        {
            InitializeComponent();
            socket.OnlineMessageReceived += OnOnlineMessageReceived;
            socket.StartService();
            //label1.Content += "dddddd\n";
        }

        private bool type=true;  //聊天方式默认true为聊天，false为操控命令行

        public SocketClient socket = new SocketClient("wss://www.nwsar.top:8080/connect", "server", "server");

        /// <summary>
        /// 发送聊天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string message = textBox1.Text;
            var tuple = new Tuple<string, object>("","");
            if (type)
                tuple = Tuple.Create<string, object>("message", message);
            else
                tuple = Tuple.Create<string, object>("cmd", message);
            if (socket.Open)
            {
                //socket.Send("client", message);
                socket.Send("client", tuple);
                textBox1.Text = string.Empty;
                Dispatcher.Invoke(delegate
                {
                    label1.Text += message + "\n";
                });
            }
            else
            {
                MessageBox.Show("Socket未连接成功,请稍后重试");
                socket.StartService();
            }
        }

        /// <summary>
        /// socket消息接收处理
        /// </summary>
        /// <param name="e"></param>
        void OnOnlineMessageReceived(MessageReceivedEventArgs e)
        {
            var tuple0 = (Tuple<string, object>)e.Protocol.Content;
            switch (tuple0.Item1)
            {
                case "message":
                    Dispatcher.Invoke(delegate
                    {
                        label1.Text += (string)tuple0.Item2 + "\n";
                        label1.ScrollToEnd();
                    });
                    break;
                case "file_return":
                    switch ((string)tuple0.Item2)
                    {
                        case "succeed":
                            MessageBox.Show("上传文件成功");
                            break;
                        case "File not Exits":
                            MessageBox.Show("未找到文件");
                            break;
                        default:
                            MessageBox.Show("失败");
                            break;
                    }
                    break;
                case "cmd_return":
                    if ((string)tuple0.Item2 == "success")
                    {
                        type = false;
                        Dispatcher.Invoke(delegate
                        {
                            label1.Text += "成功切换至命令行模式\n";
                            label1.ScrollToEnd();
                        });
                    }
                    break;
                case "chat_return":
                    if ((string)tuple0.Item2 == "success")
                    {
                        type = false;
                        Dispatcher.Invoke(delegate
                        {
                            label1.Text += "成功切换至聊天模式\n";
                            label1.ScrollToEnd();
                        });
                    }
                    break;
                case "cmd_text":
                    Dispatcher.Invoke(delegate
                    {
                        label1.Text += (string)tuple0.Item2 + "\n";
                        label1.ScrollToEnd();
                    });
                    break;
                default:
                    var tuple = (byte[])tuple0.Item2;
                    string path = (string)tuple0.Item1;
                    var filestream = new FileStream(path, FileMode.Create);
                    filestream.Write(tuple, 0, tuple.Length);
                    filestream.Close();
                    MessageBox.Show("下载文件成功");
                    break;
            }
            
            
        }        

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 打开文件夹选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                if (dialog.ShowDialog() == true)
                {
                    upload.Text = dialog.FileName;
                }
            }
            catch { }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = upload.Text;
                string[] fileName = path.Split('\\');
                if(!File.Exists(path))
                {
                    MessageBox.Show("未找到该文件");
                    return;
                }
                var fs = new FileStream(path, FileMode.Open);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                var tuple = Tuple.Create<string, object>(fileName[fileName.Length-1], bytes);
                fs.Close();
                socket.Send("client", tuple);
                upload.Text = string.Empty;
            }
            catch { }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            var message = download.Text;
            var tuple = Tuple.Create<string, object>("file_request", message);
            if (socket.Open)
            {
                //socket.Send("client", message);
                socket.Send("client", tuple);
                download.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Socket未连接成功,请稍后重试");
                socket.StartService();
            }
        }

        /// <summary>
        /// 关机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tuple = Tuple.Create<string, object>("control", "shutdown");
                if (socket.Open)
                {
                    socket.Send("client", tuple);
                }
                else
                {
                    MessageBox.Show("Socket未连接成功,请稍后重试");
                    socket.StartService();
                }
            }
            catch { }
        }

        /// <summary>
        /// 重启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tuple = Tuple.Create<string, object>("control", "restart");
                if (socket.Open)
                {
                    socket.Send("client", tuple);
                }
                else
                {
                    MessageBox.Show("Socket未连接成功,请稍后重试");
                    socket.StartService();
                }
            }
            catch { }
        }

        
    }
}
