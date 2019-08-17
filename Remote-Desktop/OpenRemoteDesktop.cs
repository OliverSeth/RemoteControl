using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Remote_Desktop
{
    class OpenRemoteDesktop
    {
        /// <summary>
        /// 密码因子
        /// </summary>
        static byte[] s_aditionalEntropy = null;

        /// <summary>
        /// 打开远程桌面
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="admin">用户名</param>
        /// <param name="pw">明文密码</param>
        public void Openrdesktop(string ip, string admin = null, string pw = null)
        {
            string password = GetRdpPassWord(pw);

            #region newrdp
            //创建rdp
            string fcRdp = @"screen mode id:i:1 
                           desktopwidth:i:1280 
                           desktopheight:i:750 
                           session bpp:i:24 
                           winposstr:s:2,3,188,8,1062,721 
                           full address:s:MyServer 
                           compression:i:1 
                           keyboardhook:i:2 
                           audiomode:i:0 
                           redirectdrives:i:0 
                           redirectprinters:i:0 
                           redirectcomports:i:0 
                           redirectsmartcards:i:0 
                           displayconnectionbar:i:1 
                           autoreconnection 
                           enabled:i:1 
                           username:s:" + admin + @"
                           domain:s:QCH
                           alternate shell:s: 
                           shell working directory:s: 
                           password 51:b:" + password + @"
                           disable wallpaper:i:1 
                           disable full window drag:i:1 
                           disable menu anims:i:1 
                           disable themes:i:0 
                           disable cursor setting:i:0 
                           bitmapcachepersistenable:i:1";

            string rdpname = "rdesktop.rdp";
            CreationBat(rdpname, fcRdp);
            #endregion

            //创建bat
            string fcBat = @"mstsc rdesktop.rdp /console /v:" + ip + ":3389";
            string batname = "runRdp.bat";
            CreationBat(batname, fcBat);
            //创建vbs

            string vbsname = "runBat.vbs";
            string fcVbs = @"set ws=WScript.CreateObject(""WScript.Shell"")" + "\r\nws.Run\"runRdp.bat\",0";
            CreationBat(vbsname, fcVbs);
            //NewVbs(vbsname);
            ExecuteVbs(vbsname);
        }

        /// <summary>
        /// 获取RDP密码
        /// </summary>
        private string GetRdpPassWord(string pw)
        {
            byte[] secret = Encoding.Unicode.GetBytes(pw);
            byte[] encryptedSecret = Protect(secret);
            string res = string.Empty;
            foreach (byte b in encryptedSecret)
            {
                res += b.ToString("X2"); //转换16进制的一定要用2位 
            }
            return res;
        }

        /// <summary>
        /// 加密方法
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] Protect(byte[] data)
        {
            try
            {
                //调用System.Security.dll
                return ProtectedData.Protect(data, s_aditionalEntropy, DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        //解密方法
        private static byte[] Unprotect(byte[] data)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return ProtectedData.Unprotect(data, s_aditionalEntropy, DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not decrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// 创建bat脚本
        /// </summary>
        /// <param name="batName">文件名</param>
        /// <param name="fileContent">文件内容</param>
        /// <param name="u"></param>
        private void CreationBat(string batName, string fileContent, string u = null)
        {
            string filepath = System.IO.Directory.GetCurrentDirectory();
            string batpath = filepath + @"\" + batName;
            writeBATFile(fileContent, batpath);
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="fileContent">文件内容</param>
        /// <param name="filePath">路径</param>
        private void writeBATFile(string fileContent, string filePath)
        {
            if (!File.Exists(filePath))
            {
                FileStream fs1 = new FileStream(filePath, FileMode.Create, FileAccess.Write);//创建写入文件
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(fileContent);//开始写入值
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(fileContent);//开始写入值
                sr.Close();
                fs.Close();
            }
        }


        /// <summary>
        /// 调用执行bat文件
        /// </summary>
        /// <param name="batName">文件名</param>
        /// <param name="thisbatpath">路径</param>
        private void ExecuteBat(string batName, string thisbatpath)
        {
            Process proc = null;
            try
            {
                string targetDir = string.Format(thisbatpath);//this is where testChange.bat lies
                proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = batName;
                proc.StartInfo.Arguments = string.Format("10");//this is argument
                proc.StartInfo.RedirectStandardError = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }
        }

        /// <summary>
        /// 调用执行vbs文件
        /// </summary>
        /// <param name="vbsName">文件名</param>
        private void ExecuteVbs(string vbsName)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + @"\" + vbsName;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "wscript.exe";
            startInfo.Arguments = path;
            Process.Start(startInfo);
        }


        /// <summary>
        /// 创建vbs文件
        /// </summary>
        /// <param name="vbsName">vbs文件名</param>
        private void NewVbs(string vbsName)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + @"\" + vbsName;
            FileStream fsvbs = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter runBat = new StreamWriter(path);
            runBat.WriteLine("set ws=WScript.CreateObject(\"WScript.Shell\")");
            runBat.WriteLine("ws.Run\"runRdp.bat\",0");
            runBat.Close();
            fsvbs.Close();
        }
    }
}