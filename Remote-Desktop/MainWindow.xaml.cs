using AxMSTSCLib;
using MSTSCLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using WebSocketProtocol;

namespace Remote_Desktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<device> DeviceList = new ObservableCollection<device>();
        private int cnt=1;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                DeviceList.Clear();
                listView.ItemsSource = DeviceList;
                //DeviceList.Add(new device(1, "10.199.172.62", "未连接", "dhu", "dhu123456"));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region 菜单栏
        /// <summary>
        /// 菜单栏“添加”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddDevice addDevice = new AddDevice();
                addDevice.ShowDialog();
                if (addDevice.DialogResult == true)
                {
                    DeviceList.Add(new device(cnt++, addDevice.textBox1.Text, "未连接", addDevice.username.Text, addDevice.password.Text));
                }
            }
            catch { }
        }

        /// <summary>
        /// 菜单栏“关于”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            Grid2.Visibility = Visibility.Collapsed;
            Grid4.Visibility = Visibility.Collapsed;
            Grid3.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 菜单栏“主页”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Index_Click(object sender, RoutedEventArgs e)
        {
            Grid4.Visibility = Visibility.Collapsed;
            Grid3.Visibility = Visibility.Collapsed;
            Grid2.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 菜单栏“帮助”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Grid2.Visibility = Visibility.Collapsed;
            Grid3.Visibility = Visibility.Collapsed;
            Grid4.Visibility = Visibility.Visible;
        }
        #endregion

        #region 操作机器
        /// <summary>
        /// “管理”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            Client client = new Client();
            client.Title += " - " + ((sender as Button).DataContext as device).Ip;
            client.Show();
        }

        /// <summary>
        /// “连接”按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ip = ((sender as Button).DataContext as device).Ip;
                string username = ((sender as Button).DataContext as device).Username;
                string password = ((sender as Button).DataContext as device).Password;
                var form = new System.Windows.Forms.Form();
                form.Text = ip;
                form.Size = new System.Drawing.Size((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
                var axmstsc = new AxMsTscAxNotSafeForScripting();
                ((System.ComponentModel.ISupportInitialize)(axmstsc)).BeginInit();
                form.Controls.Add(axmstsc);
                ((System.ComponentModel.ISupportInitialize)(axmstsc)).EndInit();
                Desktop desk = new Desktop();
                desk.Title = ip;
                
                //WindowsFormsHost wfh = new WindowsFormsHost();
                axmstsc.Height = (int)SystemParameters.PrimaryScreenHeight;
                axmstsc.Width = (int)SystemParameters.PrimaryScreenWidth;
                
                //wfh.Child = axmstsc;
                
                axmstsc.Server = ip;
                axmstsc.UserName = username;
                IMsTscNonScriptable secured = (IMsTscNonScriptable)axmstsc.GetOcx();
                secured.ClearTextPassword = password;
                axmstsc.Connect();
                form.Show();

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        #endregion

    }

}
