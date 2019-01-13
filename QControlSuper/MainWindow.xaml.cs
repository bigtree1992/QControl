using QCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace QControlServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private QControlSuper m_Server = new QControlSuper();

        private ClientInfo m_SelectClient;

        private string m_PreSetInfoMachine;

        private int m_IsLogin = 0;

        private string key = "";


        public MainWindow()
        {
            InitializeComponent();

            m_Server.OnConnectResult += (code) =>
            {
                if(code == 100)
                {
                    m_IsLogin = 50;
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoginInfo.Text = "与服务器连接失败，正在重试...";
                    });
                    m_IsLogin = 0;
                }
            };

            m_Server.GetSpuerKey = () => {
                if (m_IsLogin==100)
                {
                    return key;
                }
                else
                {
                    return Dispatcher.Invoke(new Func<string>(() => this.KeyTextBox.Text));
                }
            };

            m_Server.OnLoginResult += (result) =>
            {               
                if (result == 100)
                {
                    m_Server.SendGetClientList(0,100);
                    m_IsLogin = 100;
                    

                    Dispatcher.Invoke(() =>
                    {
                        key = KeyTextBox.Text;
                        KeyTextBox.Text = "";
                        LoginInfo.Text = "";
                    });
                    

                    MessageBox.Show("登录成功.");
                }
                else
                {
                    m_IsLogin = 0;
                    MessageBox.Show("登录失败: " + result);
                }
            };

            m_Server.OnLoginClient += (client) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (MachineList.ItemsSource != null)
                    {
                        var list = MachineList.ItemsSource as IList<ClientInfo>;
                        list.Add(client);
                    }
                    else
                    {
                        var list = new List<ClientInfo>();
                        list.Add(client);
                        MachineList.ItemsSource = list;
                        
                    }
                    MachineList.Items.Refresh();
                });
            };

            m_Server.OnLogoutClient += (client) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (MachineList.ItemsSource != null)
                    {
                        var list = MachineList.ItemsSource as IList<ClientInfo>;

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].machine == client.machine)
                            {
                                list.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    MachineList.Items.Refresh();
                });
            };

            m_Server.OnGetClientList += (clients) =>
            {
                Dispatcher.Invoke(()=> { 
                    MachineList.ItemsSource = clients;
                    MachineList.Items.Refresh();
                });
            };

            m_Server.OnCommandResult += (result) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ClientReturn.Text = result;
                });
            };

            m_Server.OnSetInfoResult += (result) =>
            {
                Dispatcher.Invoke(()=> {

                    if (!string.IsNullOrEmpty(m_PreSetInfoMachine) && 
                        MachineList.ItemsSource != null)
                    {
                        var list = MachineList.ItemsSource as IList<ClientInfo>;

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].machine == m_PreSetInfoMachine)
                            {
                                list[i].info = result;
                                break;
                            }
                        }
                    }
                    MachineList.Items.Refresh();

                    if(m_SelectClient.machine == m_PreSetInfoMachine)
                    {
                        Info.Text = result;
                        m_SelectClient.info = result;
                    }

                    m_PreSetInfoMachine = null;

                    MessageBox.Show("设置结果：" + result);
                });
            };

            m_Server.OnRunTeamViewerResult += (result) =>
            {
                Dispatcher.Invoke(() => {
                    MessageBox.Show("运行结果：" + result);
                });
            };

            m_Server.OnGetTeamViwerKeyResult += (result) =>
            {
                Dispatcher.Invoke(() => {
                    try
                    {
                        var buff = Convert.FromBase64String(result);
                        var ms = new MemoryStream(buff);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        KeyImage.Source = bitmapImage;
                    }
                    catch
                    {
                        MessageBox.Show("获取失败.");
                    }
                });
            };

            new Thread(() => { m_Server.Connect(Config.ServerIP); }).Start();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (m_IsLogin < 100)
            {
                MessageBox.Show("请先登录！");
                return;
            }

            if(m_SelectClient != null && Command.Text.Length > 0)
            {                
                m_Server.SendCommand(m_SelectClient.machine,Command.Text);
            }
        }

        private void MachineList_Selected(object sender, RoutedEventArgs e)
        {
            if (MachineList.SelectedItem != null)
            {
                var item = MachineList.SelectedItem as ClientInfo;
                m_SelectClient = item;
                Machine.Text = item.machine;
                TMachine.Text = item.machine;
                Info.Text = item.info;
            }
            else
            {
                Machine.Text = "无机器";
                TMachine.Text = "无机器";
                m_SelectClient = null;
            }
        }

        private void RunTeamViewer_Click(object sender, RoutedEventArgs e)
        {
            if (m_IsLogin < 100)
            {
                MessageBox.Show("请先登录！");
                return;
            }


            if (m_SelectClient != null )
            {
                m_Server.SendRunTeamViewer(m_SelectClient.machine);
            }
            else
            {
                MessageBox.Show("请先选择机器！");
            }
            
        }

        private void GetTeamViewerKey_Click(object sender, RoutedEventArgs e)
        {
            if (m_IsLogin < 100)
            {
                MessageBox.Show("请先登录！");
                return;
            }

            if (m_SelectClient != null )
            {
                m_Server.SendGetTeamViewerKey(m_SelectClient.machine);
            }
            else
            {
                MessageBox.Show("请先选择机器！");
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if(KeyTextBox.Text.Length == 0)
            {
                MessageBox.Show("密钥不能为空！");
                return;
            }

            if (m_IsLogin < 50)
            {
                return;
            }

            if (m_IsLogin < 100)
            {
                m_IsLogin = 90;
                LoginInfo.Text = "登录中...";
                m_Server.SendLogin();
            }
            else
            {
                MessageBox.Show("已经登录。");
            }          
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(MainTab.SelectedIndex != 3)
            {
                KeyImage.Source = null;

            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (m_IsLogin < 100)
            {
                MessageBox.Show("请先登录！");
                return;
            }

            if (m_SelectClient != null)
            {
                m_PreSetInfoMachine = m_SelectClient.machine;

                var dialog = new SetInfoDialog(m_Server,m_SelectClient.machine,m_SelectClient.info);
                
                dialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("请先选择机器！");
            }
        }
    }
}
