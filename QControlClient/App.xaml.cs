using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QCommon
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            #region 判断系统是否已启动
            //获取指定的进程名   
            var myProcesses = Process.GetProcessesByName("QControlClient");
            //如果可以获取到知道的进程名则说明已经启动
            if (myProcesses.Length > 1)
            {
                MessageBox.Show("QControlServer 已经启动！");
                //关闭系统
                System.Environment.Exit(0);
            }


            #endregion
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            System.Environment.Exit(0);
        }
    }
}
