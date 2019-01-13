using Cjwdev.WindowsApi;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QCommon
{
    public class Utils
    {
        public static void SetAutoStart(string name, string path)
        {
            var localmachine = Microsoft.Win32.Registry.LocalMachine;
            var Run = localmachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            try
            {
                Run.SetValue(name, path);
                localmachine.Close();
            }
            catch { }
        }

        public static void FixedExePath()
        {
            try
            {
                string directory = Directory.GetCurrentDirectory();

                string filepath = Process.GetCurrentProcess().MainModule.FileName;

                string[] split_result = filepath.Split('\\');
                string exename = split_result[split_result.Length - 1];

                filepath = filepath.Replace(exename, "");

                Directory.SetCurrentDirectory(filepath);
            }
            catch { }
            
        }

        public static string RunScript(string scriptText)
        {
            string result = "";

            try
            {
                // 创建 Powershell runspace
                var runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
                // 创建一个 pipeline，并添加命令脚本
                var pipeline = runspace.CreatePipeline();
                pipeline.Commands.AddScript(scriptText);
                // 格式化输出命令执行结果
                pipeline.Commands.Add("Out-String");
                // 执行脚本
                var results = pipeline.Invoke();
                runspace.Close();
                // 将执行结果输出到StringBuilder
                var sb = new StringBuilder();
                foreach (var obj in results)
                {
                    sb.AppendLine(obj.ToString());
                }

                result = sb.ToString();
            }
            catch (Exception e)
            {
                result = e.ToString();
            }

            return result;
        }

        public static bool RunTeamViewer()
        {
            try
            {
                var key = Registry.LocalMachine;
                var teamviewer = key.OpenSubKey("software\\teamviewer");

                if(teamviewer == null)
                {
                    teamviewer = key.OpenSubKey("software\\wow6432Node\\teamviewer");
                }

                //Log.Debug("RunTeamViewer " + (teamviewer == null));

                var path = teamviewer.GetValue("InstallationDirectory") as string;

                //Log.Debug("RunTeamViewer Path : " + path);
                var program = path + "\\TeamViewer.exe";

                StartApp(program);
                
            }
            catch
            {
                return false;
            }

            return true;
            
        }

        private static void StartApp(string filePath)
        {
            try
            {

                string appStartPath = filePath;
                IntPtr userTokenHandle = IntPtr.Zero;
                ApiDefinitions.WTSQueryUserToken(ApiDefinitions.WTSGetActiveConsoleSessionId(), ref userTokenHandle);

                ApiDefinitions.PROCESS_INFORMATION procInfo = new ApiDefinitions.PROCESS_INFORMATION();
                ApiDefinitions.STARTUPINFO startInfo = new ApiDefinitions.STARTUPINFO();
                startInfo.cb = (uint)Marshal.SizeOf(startInfo);

                ApiDefinitions.CreateProcessAsUser(
                    userTokenHandle,
                    appStartPath,
                    "",
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    0,
                    IntPtr.Zero,
                    null,
                    ref startInfo,
                    out procInfo);

                if (userTokenHandle != IntPtr.Zero)
                    ApiDefinitions.CloseHandle(userTokenHandle);

                int _currentAquariusProcessId = (int)procInfo.dwProcessId;
            }
            catch (Exception ex)
            {
            }
        }

        public static string GetTeamViewerKey()
        {
            return GetPictureByName("TeamViewer");
        }

        public static Bitmap GetBitmapFromResult(string result)
        {
            try
            {
                byte[] buff = Convert.FromBase64String(result);

                var ms1 = new MemoryStream(buff);
                var bm = (Bitmap)Image.FromStream(ms1);
                ms1.Close();

                return bm;
            }
            catch
            {
                return null;
            }
            
        }

        public static string GetPictureByName(string name)
        {
            try
            {
                IntPtr handle = new IntPtr(0);
                foreach (var p in Process.GetProcesses())
                {
                    if (p.ProcessName.Contains(name))
                    {
                        handle = p.MainWindowHandle;
                        if (handle.ToInt64() != 0)
                        {
                            break;
                        }
                    }
                }

                var bitmap = GetWindow(handle);

                var b = KiCut(bitmap, 20,180, 280, 80);
               
                var memstream = new MemoryStream();
                b.Save(memstream, ImageFormat.Png);
                var buff = memstream.GetBuffer();
                string base64String = Convert.ToBase64String(buff);

                return base64String;

            }
            catch
            {
                //MessageBox.Show(ex.ToString());
                return "";
            }

        }

        public static Bitmap GetWindow(IntPtr hWnd)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            var rect = new Rectangle();
            GetWindowRect(hWnd, ref rect);

            if (rect.Width < 0)
            {
                rect.Width = 150;
            }
            
            if(rect.Height < 0)
            {
                rect.Height = 100;
            }
            
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, rect.Width, rect.Height);

            IntPtr hmemdc = CreateCompatibleDC(hscrdc);

            SelectObject(hmemdc, hbitmap);

            PrintWindow(hWnd, hmemdc, 0);

            var bmp = Bitmap.FromHbitmap(hbitmap);

            DeleteDC(hscrdc);//删除用过的对象
            DeleteDC(hmemdc);//删除用过的对象

            return bmp;

        }

        /// <summary>  
        /// 将源图像灰度化，并转化为8位灰度图像。  
        /// </summary>  
        /// <param name="original"> 源图像。 </param>  
        /// <returns> 8位灰度图像。 </returns>  
        public static Bitmap RgbToGrayScale(Bitmap original)
        {
            if (original != null)
            {
                // 将源图像内存区域锁定  
                Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
                BitmapData bmpData = original.LockBits(rect, ImageLockMode.ReadOnly,
                     original.PixelFormat);

                // 获取图像参数  
                int width = bmpData.Width;
                int height = bmpData.Height;
                int stride = bmpData.Stride;  // 扫描线的宽度  
                int offset = stride - width * 3;  // 显示宽度与扫描线宽度的间隙  
                IntPtr ptr = bmpData.Scan0;   // 获取bmpData的内存起始位置  
                int scanBytes = stride * height;  // 用stride宽度，表示这是内存区域的大小  

                // 分别设置两个位置指针，指向源数组和目标数组  
                int posScan = 0, posDst = 0;
                byte[] rgbValues = new byte[scanBytes];  // 为目标数组分配内存  
                Marshal.Copy(ptr, rgbValues, 0, scanBytes);  // 将图像数据拷贝到rgbValues中  
                                                             // 分配灰度数组  
                byte[] grayValues = new byte[width * height]; // 不含未用空间。  
                                                              // 计算灰度数组  
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        double temp = rgbValues[posScan++] * 0.11 +
                            rgbValues[posScan++] * 0.59 +
                            rgbValues[posScan++] * 0.3;
                        grayValues[posDst++] = (byte)temp;
                    }
                    // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel  
                    posScan += offset;
                }

                // 内存解锁  
                Marshal.Copy(rgbValues, 0, ptr, scanBytes);
                original.UnlockBits(bmpData);  // 解锁内存区域  

                // 构建8位灰度位图  
                Bitmap retBitmap = BuiltGrayBitmap(grayValues, width, height);
                return retBitmap;
            }
            else
            {
                return null;
            }
        }

        /// <summary>  
        /// 用灰度数组新建一个8位灰度图像。  
        /// http://www.cnblogs.com/spadeq/archive/2009/03/17/1414428.html  
        /// </summary>  
        /// <param name="rawValues"> 灰度数组(length = width * height)。 </param>  
        /// <param name="width"> 图像宽度。 </param>  
        /// <param name="height"> 图像高度。 </param>  
        /// <returns> 新建的8位灰度位图。 </returns>  
        private static Bitmap BuiltGrayBitmap(byte[] rawValues, int width, int height)
        {
            // 新建一个8位灰度位图，并锁定内存区域操作  
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                 ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            // 计算图像参数  
            int offset = bmpData.Stride - bmpData.Width;        // 计算每行未用空间字节数  
            IntPtr ptr = bmpData.Scan0;                         // 获取首地址  
            int scanBytes = bmpData.Stride * bmpData.Height;    // 图像字节数 = 扫描字节数 * 高度  
            byte[] grayValues = new byte[scanBytes];            // 为图像数据分配内存  

            // 为图像数据赋值  
            int posSrc = 0, posScan = 0;                        // rawValues和grayValues的索引  
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    grayValues[posScan++] = rawValues[posSrc++];
                }
                // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel  
                posScan += offset;
            }

            // 内存解锁  
            Marshal.Copy(grayValues, 0, ptr, scanBytes);
            bitmap.UnlockBits(bmpData);  // 解锁内存区域  

            // 修改生成位图的索引表，从伪彩修改为灰度  
            ColorPalette palette;
            // 获取一个Format8bppIndexed格式图像的Palette对象  
            using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            // 修改生成位图的索引表  
            bitmap.Palette = palette;

            return bitmap;
        }

        public static Bitmap KiCut(Bitmap b, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (b == null)
            {
                return null;
            }

            int w = b.Width;
            int h = b.Height;

            if (StartX >= w || StartY >= h)
            {
                return null;
            }

            if (StartX + iWidth > w)
            {
                iWidth = w - StartX;
            }

            if (StartY + iHeight > h)
            {
                iHeight = h - StartY;
            }

            
            try
            {
                Bitmap bmpOut = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);

                Graphics g = Graphics.FromImage(bmpOut);
                var rect_dest = new Rectangle(0, 0, iWidth, iHeight);
                var rect_src = new Rectangle(StartX, StartY, iWidth, iHeight);
                g.DrawImage(b, rect_dest, rect_src, GraphicsUnit.Pixel);
                g.Dispose();

                return bmpOut;
            }
            catch
            {
                return null;
            }
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        public static extern int GetWindowRect(IntPtr hwnd, ref Rectangle lpRect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(
            string lpszDriver,        // driver name驱动名
            string lpszDevice,        // device name设备名
            string lpszOutput,        // not used; should be NULL
            IntPtr lpInitData  // optional printer data
        );

        [DllImport("gdi32.dll")]
        public static extern int BitBlt(
         IntPtr hdcDest, // handle to destination DC目标设备的句柄
         int nXDest,  // x-coord of destination upper-left corner目标对象的左上角的X坐标       
         int nYDest,  // y-coord of destination upper-left corner目标对象的左上角的Y坐标
         int nWidth,  // width of destination rectangle目标对象的矩形宽度
         int nHeight, // height of destination rectangle目标对象的矩形长度
         IntPtr hdcSrc,  // handle to source DC源设备的句柄
         int nXSrc,   // x-coordinate of source upper-left corner源对象的左上角的X坐标
         int nYSrc,   // y-coordinate of source upper-left corner源对象的左上角的Y坐标
         UInt32 dwRop  // raster operation code光栅的操作值
         );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth,int nHeight );

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc,IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hwnd,IntPtr hdcBlt, UInt32 nFlags);
   
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);
    }
}
