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
using System.Windows.Shapes;

namespace QControlServer
{
    /// <summary>
    /// SetInfoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SetInfoDialog : Window
    {
        private QControlSuper m_Server;
        private string m_Machine;
        public SetInfoDialog(QControlSuper server, string machine, string preInfo)
        {
            InitializeComponent();

            MainBox.Text = preInfo;
            m_Server = server;
            m_Machine = machine;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            m_Server.SendSendInfo(m_Machine,MainBox.Text);
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
