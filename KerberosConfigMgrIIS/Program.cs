using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace KerberosConfigMgr
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            try {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Kerberos());
                
            }
            catch(Exception e)
            {
                StreamWriter log;
                String _filename = "kerberos" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                if (!File.Exists(_filename))
                {
                    log = new StreamWriter(_filename);
                }
                else
                {
                    log = File.AppendText(_filename);
                }
                MessageBox.Show(e+"" , "--Fatal Error--");
                log.WriteLine(DateTime.Now + ": " + e + "", "--Fatal Error--");
                log.Dispose();
            }
            
        }
    }
}
