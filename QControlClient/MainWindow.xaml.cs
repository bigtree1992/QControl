using QCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace QCommon
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private QControlClient m_Client = new QControlClient();

        public MainWindow()
        {
            InitializeComponent();

            this.Visibility = Visibility.Hidden;

            Utils.FixedExePath();

            m_Client.Connect(Config.ServerIP);

            m_Client.Log += (info) =>
            {
                Dispatcher.Invoke(() => {
                    Info.Text += info + "\n";
                });
            };

            new Thread(() => {
                string exepath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                Utils.SetAutoStart("QControlClient", exepath);

            }).Start();
        }

        
    }
}
