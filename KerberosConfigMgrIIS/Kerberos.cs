using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.Administration;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Collections;
using System.Threading;
using System.IO;
using Serilog;
using Serilog.Core;

namespace KerberosConfigMgr
{
    public partial class Kerberos : Form
    {
        static string onPriority = null;
        static int count = 0;
        List<string> spnValue = new List<string>();
        bool isAppPoolSetGlobal = false;
        string UserGlobal;
        string UserGlobalDeleg;
        bool isUsingCustomAppPoolDelegation = false;
        static string day = DateTime.Today.Date.Day.ToString();
        static string month = DateTime.Today.Date.Month.ToString();
        static string year = DateTime.Today.Date.Year.ToString();
        static string filename = "k_log" + "_" + day + "_" + month + "_" + year + ".log";
        bool delegation = false;
        ToolTip toolTip2 = new ToolTip();
        Logger log = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt")
                .CreateLogger();


        [Flags()]
        public enum UserAccountControl : int
        {
            TRUSTED_FOR_DELEGATION = 0x00080000,
            TRUSTED_TO_AUTH_FOR_DELEGATION = 0x1000000,
        }
        public Kerberos()
        {
            InitializeComponent();
            log.Information("========= Program Start =========");
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            log.Information("Combobox created");
            var serverMgr = new ServerManager();
            SiteCollection s = serverMgr.Sites;
            foreach (Site site in serverMgr.Sites)
            {
                comboBox2.Items.Add(site.Name);
                foreach(Microsoft.Web.Administration.Application app in site.Applications)
                {
                    if (app.Path != "/")
                        comboBox2.Items.Add(site.Name + app.Path);                        
                }
            }
            log.Information("Sites loaded");
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.WordWrap = true;
            button4.Enabled = false;
            button1.Enabled = false;
            button3.Enabled = false;
            button2.Enabled = false;
            comboBox2.SelectedIndex = 0;
            StreamWriter w = new StreamWriter(filename, append: true);
            string date = DateTime.Now.ToString();
            w.WriteLine("Start :" + date + "\r\n----------------------------------------------------------\r\n");
            w.Close();
            
        }
        string app2;
        string selectedSite2;
        bool isAnonymousChanged = false;
        bool isBasicChanged = false;
        bool isAspnetImpersonationChanged = false;
        bool isDigestChanged = false;
        bool isWindowsChanged = false;
        bool isNegotiateChanged = false;
        bool isUseAppPoolChanged = false;
        bool isUseKernelChanged = false;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
                button1.Enabled = false;
                DialogResult result2 = MessageBox.Show("Are you sure you want to configure Kerberos for this website?", "Alert!", MessageBoxButtons.YesNo);
                if (result2.ToString().Equals("Yes"))
                {
                    comboBox2.Enabled = false;
                    button4.Enabled = false;
                    bool isUsingPoolIdentity = false;
                    String UName;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    var serverMgr = new ServerManager();
                    string selectedSite = this.comboBox2.GetItemText(this.comboBox2.SelectedItem);
                    if (selectedSite == "")
                    {
                        MessageBox.Show("No site is selected!", "Error!");
                    }
                    else
                    {
                        //<summary>
                        //To check if user wants to query DC for delegation settings
                        //based on this one more function is written for delegation query
                        //</summary>

                        bool DelegationQuery = false;

                        if (radioButton2.Checked == true)
                        {
                            DelegationQuery = delegation = true;
                        }
                        else if (radioButton1.Checked == true)
                        {
                            DelegationQuery = delegation = false;
                        }
                        else
                        {
                            MessageBox.Show("Please select one of the Radio buttons for Single Hop or Pass Through Authentication!", "Alert!");
                            return;
                        }
                        bool isAnApplication = false;
                        textBox1.Text = "=========Configure=========\r\n===============================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        textBox1.Text += "==> Selected Site : " + selectedSite + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        if (selectedSite.Contains("/"))
                        {
                            isAnApplication = true;
                            string[] app = selectedSite.Split('/');
                            selectedSite2 = selectedSite;
                            int appNum = app.Length;
                            selectedSite = app[0];
                            for (int i = 1; i < appNum; i++)
                            {
                                if (i == 1)
                                    app2 += app[i];
                                else
                                    app2 += "/" + app[i];
                            }
                        }

                        Site site = serverMgr.Sites[selectedSite];

                        textBox1.Text += "==> Sitename : " + site.Name + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 10;

                        String getPool;
                        if (isAnApplication == true)
                        {
                            textBox1.Text += "==> Application name : " + app1 + "\r\n";
                            Microsoft.Web.Administration.Application application = site.Applications["/" + app1];
                            getPool = application.ApplicationPoolName;
                            selectedSite = selectedSite2;
                        }
                        else
                        {
                            Microsoft.Web.Administration.Application application = site.Applications["/"];
                            getPool = application.ApplicationPoolName;
                        }

                        textBox1.Text += "==> Application Pool : " + getPool + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 20;

                        Configuration config = serverMgr.GetApplicationHostConfiguration();
                        ConfigurationSection anonymousAuthenticationSection = config.GetSection("system.webServer/security/authentication/anonymousAuthentication", selectedSite);
                        bool AnonymousBool = (bool)anonymousAuthenticationSection["enabled"];
                        if (AnonymousBool == true)
                        {
                            anonymousAuthenticationSection["enabled"] = false;
                            textBox1.Text += "==> Anonymous authentication is disabled..(MODIFIED)\r\n";
                            isAnonymousChanged = true;
                        }
                        else
                        {
                            textBox1.Text += "==> Anonymous authentication is already disabled...(NOT MODIFIED)\r\n";
                            isAnonymousChanged = false;
                        }


                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 30;

                        ConfigurationSection basicAuthenticationSection = config.GetSection("system.webServer/security/authentication/basicAuthentication", selectedSite);
                        bool basicBool = (bool)basicAuthenticationSection["enabled"];
                        if (basicBool == true)
                        {
                            basicAuthenticationSection["enabled"] = false;
                            textBox1.Text += "==> Basic authentication is disabled..(MODIFIED)\r\n";
                            isBasicChanged = true;
                        }
                        else
                        {
                            textBox1.Text += "==> Basic authentication is already disabled...(NOT MODIFIED)\r\n";
                            isBasicChanged = false;
                        }


                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 30;

                        ConfigurationSection digestAuthenticationSection = config.GetSection("system.webServer/security/authentication/digestAuthentication", selectedSite);
                        bool digestBool = (bool)digestAuthenticationSection["enabled"];
                        if (digestBool == true)
                        {
                            digestAuthenticationSection["enabled"] = false;
                            textBox1.Text += "==> Digest authentication is disabled..(MODIFIED)\r\n";
                            isDigestChanged = true;
                        }
                        else
                        {
                            textBox1.Text += "==> Digest authentication is already disabled...(NOT MODIFIED)\r\n";
                            isDigestChanged = false;
                        }


                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 30;



                        Configuration config1 = serverMgr.GetWebConfiguration(selectedSite);
                        ConfigurationSection identitySection = config1.GetSection("system.web/identity");
                        bool aspnetimpersonation = (bool)identitySection["impersonate"];

                        //<summary>
                        //Validating ASP.NET Impersonation settings
                        //</summary>

                        if (DelegationQuery == false)
                        {
                            if (aspnetimpersonation == true)
                            {
                                identitySection["impersonate"] = false;
                                textBox1.Text += "==> ASP.NET Impersonation is disabled..(MODIFIED)\r\n";
                                isAspnetImpersonationChanged = true;
                            }
                            else
                            {
                                textBox1.Text += "==> ASP.NET Impersonation is already disabled...(NOT MODIFIED)\r\n";
                                isAspnetImpersonationChanged = false;
                            }
                        }
                        else
                        {
                            if (aspnetimpersonation == true)
                            {
                                textBox1.Text += "==> ASP.NET Impersonation is already enabled..(NOT MODIFIED)\r\n";
                                isAspnetImpersonationChanged = false;
                            }
                            else
                            {
                                identitySection["impersonate"] = true;
                                textBox1.Text += "==> ASP.NET Impersonation is enabled...(MODIFIED)\r\n";
                                isAspnetImpersonationChanged = true;
                            }
                        }

                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        ConfigurationSection windowsAuthenticationSection = config.GetSection("system.webServer/security/authentication/windowsAuthentication", selectedSite);
                        bool windowsBool = (bool)windowsAuthenticationSection["enabled"];
                        if (windowsBool == false)
                        {
                            windowsAuthenticationSection["enabled"] = true;
                            textBox1.Text += "==> Windows authentication is enabled..(MODIFIED)\r\n";
                            isWindowsChanged = true;
                        }
                        else
                        {
                            textBox1.Text += "==> Windows authentication is already enabled...(NOT MODIFIED)\r\n";
                            isWindowsChanged = false;
                        }

                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 40;

                        ConfigurationElementCollection providersCollection = windowsAuthenticationSection.GetCollection("providers");
                        ConfigurationElement searchNegotiate = FindElement(providersCollection, "add", "value", @"Negotiate");
                        if (searchNegotiate == null) throw new InvalidOperationException("Element not found!");
                        bool isNegotiateOnPriority = false;
                        if (count == 1)
                        {
                            isNegotiateOnPriority = true;
                            textBox1.Text += "==> Negotiate is on top priority...\r\n";
                        }
                        else if (count != 1)
                        {
                            isNegotiateOnPriority = false;
                            textBox1.Text += "==> Negotiate is not a priority! Current Priority is " + onPriority + " (X)\r\n";
                        }

                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 50;

                        count = 0;

                        if (isNegotiateOnPriority == false)
                        {
                            textBox1.Text += "==> Making Negotiate the priority...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            ConfigurationElement searchOnPriority = FindElement(providersCollection, "add", "value", onPriority);
                            if (count == 1)
                            {
                                searchOnPriority["value"] = "Negotiate";
                                searchNegotiate["value"] = onPriority;
                                textBox1.Text += "==> Negotiate is the priority..(MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                            isNegotiateChanged = true;
                        }
                        else
                        {
                            isNegotiateChanged = false;
                        }

                        progressBar1.Value = 60;

                        ApplicationPool pool = serverMgr.ApplicationPools[getPool];
                        String poolIdentity = pool.ProcessModel.IdentityType.ToString();
                        String poolUser;

                        if (poolIdentity == "SpecificUser")
                        {
                            isUsingPoolIdentity = true;
                            UName = poolUser = pool.ProcessModel.UserName;
                            textBox1.Text += "==> You are using a custom identity : " + UName + "..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            isUsingPoolIdentity = false;
                            poolUser = poolUser = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                            textBox1.Text += "==> You are using a builtin account : " + poolIdentity + "..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        if (isUsingPoolIdentity == true)
                        {
                            isUseAppPoolChanged = (bool)windowsAuthenticationSection["useAppPoolCredentials"];
                            isUseKernelChanged = (bool)windowsAuthenticationSection["useKernelMode"];
                            if (isUseAppPoolChanged == true)
                            {
                                isUseAppPoolChanged = false;
                                textBox1.Text += "==> useAppPoolCredentials Already set to true...(NOT MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                            else
                            {
                                textBox1.Text += "==> Setting useAppPoolCredentials to true..\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                windowsAuthenticationSection["useAppPoolCredentials"] = true;
                                textBox1.Text += "==> useAppPoolCredentials set to true..(MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                isUseAppPoolChanged = true;
                            }
                            if (isUseKernelChanged == true)
                            {
                                isUseKernelChanged = false;
                                textBox1.Text += "==> useKernelMode Already set to true...(NOT MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                            else
                            {
                                textBox1.Text += "==> Setting useKernelMode to true..\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                windowsAuthenticationSection["useKernelMode"] = true;
                                textBox1.Text += "==> useKernelMode set to true..(MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                isUseKernelChanged = true;
                            }

                        }
                        else if (isUsingPoolIdentity == false)
                        {
                            isUseAppPoolChanged = (bool)windowsAuthenticationSection["useAppPoolCredentials"];
                            isUseKernelChanged = (bool)windowsAuthenticationSection["useKernelMode"];
                            if (isUseAppPoolChanged == false)
                            {
                                isUseAppPoolChanged = false;
                                textBox1.Text += "==> useAppPoolCredentials Already set to false...(NOT MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                            else
                            {
                                textBox1.Text += "==> Setting useAppPoolCredentials to false..\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                windowsAuthenticationSection["useAppPoolCredentials"] = false;
                                textBox1.Text += "==> useAppPoolCredentials set to false..(MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                isUseAppPoolChanged = true;
                            }
                            if (isUseKernelChanged == true)
                            {
                                //change
                                isUseKernelChanged = false;
                                textBox1.Text += "==> useKernelMode Already set to true..(NOT MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                            else
                            {
                                textBox1.Text += "==> Setting useKernelMode to true..\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                windowsAuthenticationSection["useKernelMode"] = true;
                                textBox1.Text += "==> useKernelMode set to true..(MODIFIED)\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                isUseKernelChanged = true;
                            }
                        }

                        textBox1.Text += "===============================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        //<summary>
                        //Adding the condition to check if user wants to see the available SPNs
                        //This will query the DC for SPNs
                        //<summary>

                        DialogResult QuerySPNDialogueConfigure = MessageBox.Show("Are you sure you want to fetch the currently available SPNs for the service?", "Alert!", MessageBoxButtons.YesNo);
                        if (QuerySPNDialogueConfigure.ToString().Equals("Yes"))
                        {
                            textBox1.Text += "\r\nFetching SPNs for the account set for Application pool identity..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            if (isUsingPoolIdentity == true)
                            {
                                isAppPoolSetGlobal = isUsingPoolIdentity;
                                bool isSetSPNsSmall = false;
                                UName = pool.ProcessModel.UserName;
                                bool isSetSPNsCaps = false;
                                textBox1.Text += "\r\n=========SPNs set for the Custom account: " + UName + "=========\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                UName = pool.ProcessModel.UserName;
                                UserGlobal = UName;
                                string[] split = UName.Split('\\');
                                UserGlobalDeleg = split[1];
                                isUsingCustomAppPoolDelegation = true;
                                foreach (string value in ListSPN("HTTP", isUsingPoolIdentity, split[1]))
                                {
                                    if (value != null)
                                    {
                                        textBox1.Text += value + "\r\n";
                                        Thread.Sleep(100);
                                        System.Windows.Forms.Application.DoEvents();
                                        isSetSPNsSmall = true;
                                    }
                                    else
                                    {
                                        isSetSPNsSmall = false;
                                    }

                                }

                                foreach (string value in ListSPN("http", isUsingPoolIdentity, split[1]))
                                {
                                    if (value != null)
                                    {
                                        textBox1.Text += value + "\r\n";
                                        Thread.Sleep(100);
                                        System.Windows.Forms.Application.DoEvents();
                                        isSetSPNsCaps = true;
                                    }
                                    else
                                    {
                                        isSetSPNsCaps = false;
                                    }

                                }

                                if (isSetSPNsCaps == false && isSetSPNsSmall == false)
                                {
                                    textBox1.Text += "No SPNs set for this account (X)\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();
                                }

                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }

                            else
                            {
                                bool isSetSPNsSmall = false;
                                bool isSetSPNsCaps = false;
                                string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                                isAppPoolSetGlobal = false;
                                UserGlobal = computerName;
                                UserGlobalDeleg = computerName;
                                isUsingCustomAppPoolDelegation = false;
                                textBox1.Text += "\r\n=========SPNs set for the " + computerName + " account=========\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                foreach (string value in ListSPN("HOST", isUsingPoolIdentity, computerName))
                                {
                                    if (value != null)
                                    {
                                        textBox1.Text += value + "\r\n";
                                        Thread.Sleep(100);
                                        System.Windows.Forms.Application.DoEvents();
                                        isSetSPNsSmall = true;
                                    }
                                    else
                                    {
                                        isSetSPNsSmall = false;
                                    }

                                }

                                foreach (string value in ListSPN("host", isUsingPoolIdentity, computerName))
                                {
                                    if (value != null)
                                    {
                                        textBox1.Text += value + "\r\n";
                                        Thread.Sleep(100);
                                        System.Windows.Forms.Application.DoEvents();
                                        isSetSPNsCaps = true;
                                    }
                                    else
                                        isSetSPNsCaps = false;
                                }

                                if (isSetSPNsCaps == false && isSetSPNsSmall == false)
                                {
                                    textBox1.Text += "No SPNs set for this account (X)\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();
                                }

                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                        }

                        progressBar1.Value = 70;

                        //<summary>
                        //Query Domain controller for Delegation settings for the application pool credentials (machine account or custom account)
                        //</summary>

                        bool isDelegationSet = false;
                        bool isConstrainedDelegationSet = false;

                        try
                        {
                            if (DelegationQuery == true)
                            {
                                textBox1.Text += "\r\n=========Delegation Settings=========\r\n";
                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                using (Domain domain = Domain.GetCurrentDomain())
                                {
                                    DirectoryEntry ouDn = new DirectoryEntry();
                                    DirectorySearcher search = new DirectorySearcher(ouDn);
                                    if (isUsingPoolIdentity == true)
                                    {
                                        string[] User = poolUser.Split('\\');
                                        search.Filter = "(sAMAccountName=" + User[1] + ")";
                                    }
                                    else
                                    {
                                        search.Filter = "(sAMAccountName=" + poolUser + "$)";
                                    }
                                    search.PropertiesToLoad.Add("displayName");
                                    search.PropertiesToLoad.Add("userAccountControl");

                                    SearchResult result_dc = search.FindOne();
                                    if (result_dc != null)
                                    {
                                        DirectoryEntry entry = result_dc.GetDirectoryEntry();

                                        int userAccountControlFlags = (int)entry.Properties["userAccountControl"].Value;

                                        object[] arr = (object[])entry.Properties["msDS-AllowedToDelegateTo"].Value;

                                        if ((userAccountControlFlags & (int)UserAccountControl.TRUSTED_FOR_DELEGATION) == (int)UserAccountControl.TRUSTED_FOR_DELEGATION)
                                        {
                                            textBox1.Text += "This user is trusted for delegation to any service(Kerberos only)..\r\n";
                                            isDelegationSet = true;
                                            isConstrainedDelegationSet = false;
                                        }
                                        else if (arr != null)
                                        {

                                            textBox1.Text += "This user is trusted for delegation to specified services only..\r\n";
                                            Thread.Sleep(100);
                                            System.Windows.Forms.Application.DoEvents();

                                            if ((userAccountControlFlags & (int)UserAccountControl.TRUSTED_TO_AUTH_FOR_DELEGATION) == (int)UserAccountControl.TRUSTED_TO_AUTH_FOR_DELEGATION)
                                            {
                                                textBox1.Text += "\r\n'Use any authentication protocol' is set..\r\n";
                                            }

                                            else
                                            {
                                                textBox1.Text += "\r\n'Use Kerberos only' is set..\r\n";
                                            }
                                            Thread.Sleep(100);
                                            System.Windows.Forms.Application.DoEvents();

                                            textBox1.Text += "\r\nServices to which this account can present delegated credentials:\r\n\r\n";
                                            Thread.Sleep(100);
                                            System.Windows.Forms.Application.DoEvents();

                                            foreach (object o in arr)
                                            {
                                                textBox1.Text += o + "\r\n";
                                                Thread.Sleep(100);
                                                System.Windows.Forms.Application.DoEvents();
                                            }

                                            isDelegationSet = true;
                                            isConstrainedDelegationSet = true;
                                        }
                                        else
                                        {
                                            textBox1.Text += "This user is not trusted for delegation..\r\n";
                                            Thread.Sleep(100);
                                            System.Windows.Forms.Application.DoEvents();

                                            isDelegationSet = false;
                                            isConstrainedDelegationSet = false;
                                        }

                                    }
                                }

                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                        }
                        catch (Exception e2)
                        {
                            textBox1.Text += "\r\nError\r\n=======\r\n" + e2 + "\r\n\r\n";
                            MessageBox.Show("" + e2, "Fatal Error!");
                        }

                        progressBar1.Value = 80;

                        DialogResult result1 = MessageBox.Show("Are you using hostname for the website?", "Alert!", MessageBoxButtons.YesNo);

                        Label: if (result1.ToString().Equals("Yes"))
                        {
                            string customHostName = "";
                            while (customHostName == "")
                            {
                                customHostName = Prompt.ShowDialog("Enter the hostname:", "Important!");
                                if (customHostName == "")
                                {
                                    MessageBox.Show("No hostname is entered!", "Error!");
                                    result1 = MessageBox.Show("Are you using hostname for the website?", "Alert!", MessageBoxButtons.YesNo);
                                    goto Label;
                                }

                            }

                            textBox1.Text += "\r\n==> The hostname you entered is : " + customHostName + "\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "\r\n========SPNs needed for kerberos to work========\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            if (isUsingPoolIdentity == true)
                            {
                                try
                                {
                                    UName = pool.ProcessModel.UserName;
                                    string fqdn = System.Net.Dns.GetHostEntry(customHostName).HostName;

                                    textBox1.Text += "Below SPNs should be on account: " + UName + "\r\n\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();

                                    textBox1.Text += "HTTP/" + customHostName + "\r\n";
                                    spnValue.Add("HTTP/" + customHostName);
                                    if (fqdn != customHostName)
                                    {
                                        textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                        spnValue.Add("HTTP/" + fqdn);
                                    }
                                    
                                }
                                catch (Exception e4)
                                {
                                    textBox1.Text += "\r\nError\r\n=======\r\n" + e4 + "\r\n\r\n";
                                    MessageBox.Show("" + e4, "Fatal Error!");
                                }

                            }
                            else
                            {
                                try
                                {
                                    string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

                                    textBox1.Text += "Below SPNs should be on account: " + computerName + "\r\n\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();

                                    textBox1.Text += "HTTP/" + customHostName + "\r\n";
                                    spnValue.Add("HTTP/" + customHostName);
                                    string fqdn = System.Net.Dns.GetHostEntry(customHostName).HostName;
                                    if (fqdn != customHostName)
                                    {
                                        textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                        spnValue.Add("HTTP/" + fqdn);
                                    }
                                    
                                }
                                catch (Exception e1)
                                {
                                    textBox1.Text += "\r\nError\r\n=======\r\n" + e1 + "\r\n\r\n";
                                    MessageBox.Show("" + e1, "Fatal Error!");
                                }
                            }

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();


                        }
                        else
                        {
                            textBox1.Text += "\r\n==> You are not using any hostname.\r\n\r\n========SPNs needed for kerberos to work========\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            progressBar1.Value = 90;

                            if (isUsingPoolIdentity == true)
                            {
                                try
                                {
                                    UName = pool.ProcessModel.UserName;

                                    textBox1.Text += "Below SPNs should be on account: " + UName + "\r\n\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();

                                    string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                                    textBox1.Text += "HTTP/" + computerName + "\r\n";
                                    spnValue.Add("HTTP/" + computerName);
                                    string fqdn = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                                    textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                    spnValue.Add("HTTP/" + fqdn);
                                    
                                }
                                catch (Exception e2)
                                {
                                    textBox1.Text += "\r\nError\r\n=======\r\n" + e2 + "\r\n\r\n";
                                    MessageBox.Show("" + e2, "Fatal Error!");
                                }

                            }
                            else
                            {
                                try
                                {
                                    string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

                                    textBox1.Text += "Below SPNs should be on account: " + computerName + "\r\n\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();

                                    textBox1.Text += "HOST/" + computerName + "\r\n";
                                    spnValue.Add("HOST/" + computerName);
                                    string fqdn = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                                    textBox1.Text += "HOST/" + fqdn + "\r\n";
                                    spnValue.Add("HOST/" + fqdn);
                                    
                                }
                                catch (Exception e3)
                                {
                                    textBox1.Text += "\r\nError\r\n=======\r\n" + e3 + "\r\n\r\n";
                                    MessageBox.Show("" + e3, "Fatal Error!");
                                }

                            }

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                        }

                        progressBar1.Value = 95;

                        //bool isDelegationSetConf = false;
                        //bool isConstrainedDelegationSetConf = false;
                        if (DelegationQuery == true)
                        {
                            textBox1.Text += "\r\n========Required Delegation Settings========\r\n";
                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            if (isDelegationSet == true)
                            {
                                if (isConstrainedDelegationSet == true)
                                    textBox1.Text += "Delegation is already set but you need to verify the services!\r\n";
                                else
                                    textBox1.Text += "Delegation is already set!\r\n";
                            }
                            else
                            {
                                textBox1.Text += "==> Delegation is not set.\r\n==> We need to configure Either Constrained or Unconstrained Delegation.\r\n";
                            }
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        if (DelegationQuery == true)
                        {
                            textBox1.Text += "\r\n==> You can generate cmdlet and powershell script to set SPNs and configure delegation for the application pool user on DC\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "==> Note that the powershell script which gets generated will only configure Unconstrained Delegation.\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "==> Click the below button to save the cmdlet and ps1 files to current directory.\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "\r\n==> You can generate cmdlet to set SPNs for application pool user on DC\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "==> Click the below button to save the cmdlet to current directory.\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        button4.Enabled = true;
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 100;
                        serverMgr.CommitChanges();
                        button2.Enabled = false;
                        button1.Enabled = false;
                        button3.Enabled = true;
                        StreamWriter w = new StreamWriter(filename, append: true);
                        w.WriteLine(DateTime.Now.ToString() + "\r\n----------------------------------------------------------\r\n");
                        string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                        w.WriteLine("Currently Logged-in User : " + userName + "\r\n");
                        w.WriteLine(textBox1.Text);
                        w.Close();
                        MessageBox.Show("Site has been configured with Kerberos successfully!", "Success!");
                    }
                }

                else
                {
                    button1.Enabled = true;
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                    return;
                }
            }
            catch(Exception ex)
            {
                textBox1.Text += "\r\nError\r\n=======\r\n" + ex + "\r\n\r\n";
                MessageBox.Show("" + ex, "Fatal Error!");
            }
        }

        private void InputBox()
        {
            throw new NotImplementedException();
        }

        ArrayList ListSPN(string ServiceType, bool isUsingAppPool , string AccountName = "")
        {
            
            ArrayList SPNs = new ArrayList();
            string spnfilter = "(servicePrincipalName={0}/*)";
            spnfilter = String.Format(spnfilter, ServiceType);
            System.DirectoryServices.DirectoryEntry domain = new DirectoryEntry();
            System.DirectoryServices.DirectorySearcher searcher = new DirectorySearcher();
            searcher.SearchRoot = domain;
            searcher.PageSize = 1000;
            //searcher.Filter = spnfilter;

            //<summary>
            //Add-on for v2.1
            //-checking if the account is a machine account or a user account
            //-then adding this filter to search the account in the directory
            //-Using this filter we can just fetch one account details rather than enumerating through each account
            //</summary>

            if(isUsingAppPool == false)
                searcher.Filter ="(sAMAccountName=" + AccountName + "$)";
            else
                searcher.Filter = "(sAMAccountName=" + AccountName + ")";
            SearchResult result;
            try {
                result = searcher.FindOne();
                //foreach (SearchResult result in results)
                {
                    DirectoryEntry account = result.GetDirectoryEntry();
                   // if (account.Properties["sAMAccountName"].Value.ToString().Contains(AccountName))
                   // {
                        foreach (string spn in account.Properties["servicePrincipalName"])
                        {
                            if (spn.Contains(ServiceType))
                            {
                                SPNs.Add(spn);
                            }
                        }
                    }
               // }
                
            }

            catch(Exception e)
            {
                textBox1.Text += "\r\nError\r\n=======\r\n" + e + "\r\n\r\n";
                MessageBox.Show("" + e, "Fatal Error!");
            }

            return SPNs;
        }


        private static ConfigurationElement FindElement(ConfigurationElementCollection collection, string elementTagName, params string[] keyValues)
        {
            foreach (ConfigurationElement element in collection)
            {
                if (String.Equals(element.ElementTagName, elementTagName, StringComparison.OrdinalIgnoreCase))
                {
                    bool matches = true;

                    for (int i = 0; i < keyValues.Length; i += 2)
                    {
                        object o = element.GetAttributeValue(keyValues[i]);
                        string value = null;
                        if (o != null)
                        {
                            value = o.ToString();
                        }

                        if (!String.Equals(value, keyValues[i + 1], StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                            if (count == 1)
                            {
                                onPriority = value;
                            }
                            matches = false;
                            break;
                        }
                    }
                    if (matches)
                    {
                        count++;
                        return (element);
                    }
                }
            }
            return null;
        }
        
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            log.Information("Default Selected Site: "+ comboBox2.SelectedItem);
            button2.Enabled = true;
            button1.Enabled = false;
            button3.Enabled = false;
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            //comboBox2.Enabled = false;
        }

        
        void comboBox2_Leave(object sender, EventArgs e)
        {
            toolTip2.Hide(comboBox2);
        }

        void comboBox2_DropDownClosed(object sender, EventArgs e)
        {
            toolTip2.Hide(comboBox2);
        }

        void comboBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; }
            string text = comboBox2.GetItemText(comboBox2.Items[e.Index]);
            e.DrawBackground();
            using (SolidBrush br = new SolidBrush(e.ForeColor))
            { 
                e.Graphics.DrawString(text, e.Font, br, e.Bounds); 
            }
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            { 
                toolTip2.Show(text, comboBox2, e.Bounds.Right, e.Bounds.Bottom); 
            }
            e.DrawFocusRectangle();
        }


        private void Kerberos_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.button2, "Reviews all the current settings for the selected site.");
            ToolTip1.SetToolTip(this.button1, "Configures Kerberos for the selected site.");
            ToolTip1.SetToolTip(this.button3, "Reverts all the changes which were made in previous click for the selected site.");
            ToolTip1.SetToolTip(this.button4, "Generates the script for setting up SPNs required for the selected site to make kerberos work.");
            ToolTip1.SetToolTip(this.radioButton1, "Reviews/Configures the Kerberos Single Hop Authentication.");
            ToolTip1.SetToolTip(this.radioButton2, "Reviews/Configures the Kerberos Double Hop Authentication(Pass Through Authentication).");
            ToolTip1.SetToolTip(this.comboBox2, "You can select the website/Web application of your choice for review/configuring Kerberos.");
            
            //<summary>
            //ToolTip for each combobox item on hover
            //</summary>
            comboBox2.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox2.DrawItem += new DrawItemEventHandler(comboBox2_DrawItem);
            comboBox2.DropDownClosed += new EventHandler(comboBox2_DropDownClosed);
            comboBox2.Leave += new EventHandler(comboBox2_Leave);
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (File.Exists("spn.cmd"))
            {
                File.Delete("spn.cmd");
            }

            if (delegation == true)
            {
                if (File.Exists("delegation.ps1"))
                {
                    File.Delete("delegation.ps1");
                }
            }

            using (StreamWriter fs = File.CreateText("spn.cmd"))
            {
                foreach (string spn in spnValue)
                {
                    fs.WriteLine("setspn -s " + spn + " " + UserGlobal);
                }

                fs.Close();
            }

            if (delegation == true)
            {
                using (StreamWriter fs = File.CreateText("delegation.ps1"))
                {


                    fs.WriteLine("Import-Module ActiveDirectory");
                    if (isUsingCustomAppPoolDelegation == true)
                        fs.WriteLine("Set-ADAccountControl -Identity '" + UserGlobalDeleg + "' -TrustedForDelegation $true");
                    else
                        fs.WriteLine("Set-ADAccountControl -Identity '" + UserGlobalDeleg + "$' -TrustedForDelegation $true");
                    fs.Close();
                }
            }
            spnValue.Clear();
            if(delegation == true)
                MessageBox.Show("Scripts Generated and Saved to current directory for adding SPNs and Configuring Delegation!", "Success!");
            else
                MessageBox.Show("Script Generated and Saved to current directory for adding SPNs!", "Success!");
            button4.Enabled = false;
        }
        string app1;
        string selectedSite1;
        private void button2_Click(object sender, EventArgs e)
        {
            try
            { 
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
                button2.Enabled = false;
                DialogResult result = MessageBox.Show("Do you want to review the current settings for this website?", "Alert!", MessageBoxButtons.YesNo);
                if (result.ToString().Equals("Yes"))
                {
                    button4.Enabled = false;
                    bool isUsingPoolIdentity = false;
                    String UName;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    var serverMgr = new ServerManager();
                    string selectedSite = this.comboBox2.GetItemText(this.comboBox2.SelectedItem);
                    if (selectedSite == "")
                    {
                        MessageBox.Show("No site is selected!", "Error!");
                    }
                    else
                    {
                        bool isAnApplication = false;

                        //<summary>
                        //To check if user wants to query DC for delegation settings
                        //based on this one more function is written for delegation query
                        //</summary>

                        bool DelegationQuery = false;

                        if (radioButton2.Checked == true)
                        {
                            DelegationQuery = true;
                        }
                        else if (radioButton1.Checked == true)
                        {
                            DelegationQuery = false;
                        }
                        else
                        {
                            MessageBox.Show("Please select one of the Radio buttons for Single Hop or Pass Through Authentication!", "Alert!");
                            return;
                        }
                        textBox1.Text = "=========Review=========\r\n===============================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        textBox1.Text += "==> Selected Site : " + selectedSite + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        if (selectedSite.Contains("/"))
                        {
                            isAnApplication = true;
                            string[] app = selectedSite.Split('/');
                            selectedSite1 = selectedSite;
                            int appNum = app.Length;
                            selectedSite = app[0];
                            for (int i = 1; i < appNum; i++)
                            {
                                if (i == 1)
                                    app1 += app[i];
                                else
                                    app1 += "/" + app[i];
                            }
                        }

                        Site site = serverMgr.Sites[selectedSite];

                        textBox1.Text += "==> Sitename : " + site.Name + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 10;
                        String getPool;
                        if (isAnApplication == true)
                        {
                            textBox1.Text += "==> Application name : " + app1 + "\r\n";
                            Microsoft.Web.Administration.Application application = site.Applications["/" + app1];
                            getPool = application.ApplicationPoolName;
                            selectedSite = selectedSite1;
                        }
                        else
                        {
                            Microsoft.Web.Administration.Application application = site.Applications["/"];
                            getPool = application.ApplicationPoolName;
                        }
                        textBox1.Text += "==> Application Pool : " + getPool + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 20;

                        Configuration config = serverMgr.GetApplicationHostConfiguration();
                        ConfigurationSection anonymousAuthenticationSection = config.GetSection("system.webServer/security/authentication/anonymousAuthentication", selectedSite);
                        bool anonymous = (bool)anonymousAuthenticationSection["enabled"];
                        if (anonymous == true)
                            textBox1.Text += "==> Anonymous authentication is enabled (X)\r\n";
                        else
                            textBox1.Text += "==> Anonymous authentication is disabled (\u2714)\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        ConfigurationSection basicAuthenticationSection = config.GetSection("system.webServer/security/authentication/basicAuthentication", selectedSite);
                        bool basic = (bool)basicAuthenticationSection["enabled"];
                        if (basic == true)
                            textBox1.Text += "==> Basic authentication is enabled (X)\r\n";
                        else
                            textBox1.Text += "==> Basic authentication is disabled (\u2714)\r\n";

                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        ConfigurationSection digestAuthenticationSection = config.GetSection("system.webServer/security/authentication/digestAuthentication", selectedSite);
                        bool digest = (bool)digestAuthenticationSection["enabled"];
                        if (digest == true)
                            textBox1.Text += "==> Digest authentication is enabled (X)\r\n";
                        else
                            textBox1.Text += "==> Digest authentication is disabled (\u2714)\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        Configuration config1 = serverMgr.GetWebConfiguration(selectedSite);
                        ConfigurationSection identitySection = config1.GetSection("system.web/identity");
                        bool aspnetimpersonation = (bool)identitySection["impersonate"];

                        //<summary>
                        //Validating ASP.NET Impersonation settings
                        //</summary>

                        if (DelegationQuery == false)
                        {
                            if (aspnetimpersonation == true)
                                textBox1.Text += "==> ASP.NET Impersonation is enabled (X)\r\n";
                            else
                                textBox1.Text += "==> ASP.NET Impersonation is disabled (\u2714)\r\n";
                        }
                        else
                        {
                            if (aspnetimpersonation == true)
                                textBox1.Text += "==> ASP.NET Impersonation is enabled (\u2714)\r\n";
                            else
                                textBox1.Text += "==> ASP.NET Impersonation is disabled (X)\r\n";
                        }

                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 30;

                        ConfigurationSection windowsAuthenticationSection = config.GetSection("system.webServer/security/authentication/windowsAuthentication", selectedSite);
                        bool win = (bool)windowsAuthenticationSection["enabled"];

                        if (win == false)
                            textBox1.Text += "==> Windows authentication is disabled (X)\r\n";
                        else
                            textBox1.Text += "==> Windows authentication is enabled (\u2714)\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 40;

                        ConfigurationElementCollection providersCollection = windowsAuthenticationSection.GetCollection("providers");
                        ConfigurationElement searchNegotiate = FindElement(providersCollection, "add", "value", @"Negotiate");
                        if (searchNegotiate == null) throw new InvalidOperationException("Element not found!");
                        bool isNegotiateOnPriority = false;
                        if (count == 1)
                        {
                            isNegotiateOnPriority = true;
                            textBox1.Text += "==> Negotiate is on priority (\u2714)\r\n";
                        }
                        else if (count != 1)
                        {
                            isNegotiateOnPriority = false;
                            textBox1.Text += "==> Negotiate is not a priority! Current Priority is " + onPriority + " (X)\r\n";
                            textBox1.Text += "==> Negotiate should be a top priority..\r\n";
                        }

                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        progressBar1.Value = 50;
                        count = 0;
                        progressBar1.Value = 60;

                        ApplicationPool pool = serverMgr.ApplicationPools[getPool];
                        String poolIdentity = pool.ProcessModel.IdentityType.ToString();
                        String poolUser;
                        if (poolIdentity == "SpecificUser")
                        {
                            isUsingPoolIdentity = true;
                            UName = poolUser = pool.ProcessModel.UserName;
                            textBox1.Text += "==> You are using a custom identity : " + UName + "...\r\n";
                            textBox1.Text += "==> We should have useAppPoolCredentials set to true...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            isUsingPoolIdentity = false;
                            poolUser = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                            textBox1.Text += "==> You are using a builtin account : " + poolIdentity + "..\r\n";
                            textBox1.Text += "==> We should have useAppPoolCredentials set to false and useKernelMode set to true...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        if (isUsingPoolIdentity == true)
                        {
                            bool useAppPool = (bool)windowsAuthenticationSection["useAppPoolCredentials"];
                            if (useAppPool == true)
                                textBox1.Text += "==> useAppPoolCredentials set to true (\u2714)\r\n";
                            else
                                textBox1.Text += "==> useAppPoolCredentials set to false (X)\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            bool useKernel = (bool)windowsAuthenticationSection["useKernelMode"];
                            if (useKernel == true)
                                textBox1.Text += "==> useKernelMode set to true (\u2714)\r\n";
                            else
                                textBox1.Text += "==> useKernelMode set to false (X)\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else if (isUsingPoolIdentity == false)
                        {
                            bool useAppPool = (bool)windowsAuthenticationSection["useAppPoolCredentials"];
                            if (useAppPool == true)
                                textBox1.Text += "==> useAppPoolCredentials set to true (X)\r\n";
                            else
                                textBox1.Text += "==> useAppPoolCredentials set to false (\u2714)\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            bool useKernel = (bool)windowsAuthenticationSection["useKernelMode"];
                            if (useKernel == true)
                                textBox1.Text += "==> useKernelMode set to true (\u2714)\r\n";
                            else
                                textBox1.Text += "==> useKernelMode set to false (X)\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        textBox1.Text += "===============================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        //<summary>
                        //Adding the condition to check if user wants to see the available SPNs
                        //This will query the DC for SPNs
                        //<summary>

                        DialogResult QuerySPNDialogueReview = MessageBox.Show("Are you sure you want to fetch the available SPNs for the service?", "Alert!", MessageBoxButtons.YesNo);
                        if (QuerySPNDialogueReview.ToString().Equals("Yes"))
                        {
                            textBox1.Text += "\r\n==> Fetching SPNs for the account set for Application pool identity..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            if (isUsingPoolIdentity == true)
                            {
                                isAppPoolSetGlobal = isUsingPoolIdentity;
                                bool isSetSPNsSmall = false;
                                UName = pool.ProcessModel.UserName;
                                bool isSetSPNsCaps = false;
                                textBox1.Text += "\r\n=========SPNs set for the Custom account: " + UName + "=========\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                UName = pool.ProcessModel.UserName;
                                UserGlobal = UName;
                                string[] split = UName.Split('\\');
                                foreach (string value in ListSPN("HTTP",isUsingPoolIdentity, split[1]))
                                {
                                    if (value != null)
                                    {
                                        textBox1.Text += value + "\r\n";
                                        Thread.Sleep(100);
                                        System.Windows.Forms.Application.DoEvents();
                                        isSetSPNsSmall = true;
                                    }
                                    else
                                    {
                                        isSetSPNsSmall = false;
                                    }

                                }

                                foreach (string value in ListSPN("http", isUsingPoolIdentity, split[1]))
                                {
                                    if (value != null)
                                    {
                                        textBox1.Text += value + "\r\n";
                                        Thread.Sleep(100);
                                        System.Windows.Forms.Application.DoEvents();
                                        isSetSPNsCaps = true;
                                    }
                                    else
                                    {
                                        isSetSPNsCaps = false;
                                    }

                                }

                                if (isSetSPNsCaps == false && isSetSPNsSmall == false)
                                {
                                    textBox1.Text += "==> No SPNs set for this account (X)\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();
                                }

                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }

                            else
                            {
                                bool isSetSPNsSmall = false;
                                bool isSetSPNsCaps = false;
                                string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                                isAppPoolSetGlobal = false;
                                UserGlobal = computerName;
                                textBox1.Text += "\r\n=========SPNs set for the " + computerName + " account=========\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                foreach (string value in ListSPN("HOST", isUsingPoolIdentity, computerName))
                                {
                                    if (value != null)
                                    {
                                        textBox1.Text += value + "\r\n";
                                        Thread.Sleep(100);
                                        System.Windows.Forms.Application.DoEvents();
                                        isSetSPNsSmall = true;
                                    }
                                    else
                                    {
                                        isSetSPNsSmall = false;
                                    }

                                }

                                foreach (string value in ListSPN("host", isUsingPoolIdentity, computerName))
                                {
                                    if (value != null)
                                    {
                                        textBox1.Text += value + "\r\n";
                                        Thread.Sleep(100);
                                        System.Windows.Forms.Application.DoEvents();
                                        isSetSPNsCaps = true;
                                    }
                                    else
                                        isSetSPNsCaps = false;
                                }

                                if (isSetSPNsCaps == false && isSetSPNsSmall == false)
                                {
                                    textBox1.Text += "No SPNs set for this account (X)\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();
                                }

                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                        }

                        progressBar1.Value = 70;

                        //<summary>
                        //Query Domain controller for Delegation settings for the application pool credentials (machine account or custom account)
                        //</summary>

                        bool isDelegationSet = false;
                        bool isConstrainedDelegationSet = false;
                        try
                        {
                            if (DelegationQuery == true)
                            {
                                textBox1.Text += "\r\n=========Delegation Settings=========\r\n";
                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                                using (Domain domain = Domain.GetCurrentDomain())
                                {
                                    DirectoryEntry ouDn = new DirectoryEntry();
                                    DirectorySearcher search = new DirectorySearcher(ouDn);
                                    if (isUsingPoolIdentity == true)
                                    {
                                        string[] User = poolUser.Split('\\');
                                        search.Filter = "(sAMAccountName=" + User[1] + ")";
                                    }
                                    else
                                    {
                                        search.Filter = "(sAMAccountName=" + poolUser + "$)";
                                    }
                                    search.PropertiesToLoad.Add("displayName");
                                    search.PropertiesToLoad.Add("userAccountControl");

                                    SearchResult result_dc = search.FindOne();
                                    if (result_dc != null)
                                    {
                                        DirectoryEntry entry = result_dc.GetDirectoryEntry();

                                        int userAccountControlFlags = (int)entry.Properties["userAccountControl"].Value;

                                        object[] arr = (object[])entry.Properties["msDS-AllowedToDelegateTo"].Value;

                                        if ((userAccountControlFlags & (int)UserAccountControl.TRUSTED_FOR_DELEGATION) == (int)UserAccountControl.TRUSTED_FOR_DELEGATION)
                                        {
                                            textBox1.Text += "This user is trusted for delegation to any service(Kerberos only)..\r\n";
                                            isDelegationSet = true;
                                            isConstrainedDelegationSet = false;
                                        }
                                        else if (arr != null)
                                        {

                                            textBox1.Text += "This user is trusted for delegation to specified services only..\r\n";
                                            Thread.Sleep(100);
                                            System.Windows.Forms.Application.DoEvents();

                                            if ((userAccountControlFlags & (int)UserAccountControl.TRUSTED_TO_AUTH_FOR_DELEGATION) == (int)UserAccountControl.TRUSTED_TO_AUTH_FOR_DELEGATION)
                                            {
                                                textBox1.Text += "\r\n'Use any authentication protocol' is set..\r\n";
                                            }

                                            else
                                            {
                                                textBox1.Text += "\r\n'Use Kerberos only' is set..\r\n";
                                            }
                                            Thread.Sleep(100);
                                            System.Windows.Forms.Application.DoEvents();

                                            textBox1.Text += "\r\nServices to which this account can present delegated credentials:\r\n\r\n";
                                            Thread.Sleep(100);
                                            System.Windows.Forms.Application.DoEvents();

                                            foreach (object o in arr)
                                            {
                                                textBox1.Text += o + "\r\n";
                                                Thread.Sleep(100);
                                                System.Windows.Forms.Application.DoEvents();
                                            }

                                            isDelegationSet = true;
                                            isConstrainedDelegationSet = true;
                                        }
                                        else
                                        {
                                            textBox1.Text += "This user is not trusted for delegation..\r\n";
                                            Thread.Sleep(100);
                                            System.Windows.Forms.Application.DoEvents();
                                            isDelegationSet = false;
                                            isConstrainedDelegationSet = false;
                                        }

                                    }
                                }

                                textBox1.Text += "===============================================================\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                        }
                        catch (Exception e2)
                        {
                            textBox1.Text += "\r\nError\r\n=======\r\n" + e2 + "\r\n\r\n";
                            MessageBox.Show("" + e2, "Fatal Error!");
                        }

                        progressBar1.Value = 80;

                        //<summary>
                        //Display required SPNs
                        //</summary>
                        DialogResult result1 = MessageBox.Show("Are you using hostname for the website?", "Alert!", MessageBoxButtons.YesNo);

                        Label: if (result1.ToString().Equals("Yes"))
                        {
                            string customHostName = "";
                            while (customHostName == "")
                            {
                                customHostName = Prompt.ShowDialog("Enter the hostname:", "Important!");
                                if (customHostName == "")
                                {
                                    MessageBox.Show("No hostname is entered!", "Error!");
                                    result1 = MessageBox.Show("Are you using hostname for the website?", "Alert!", MessageBoxButtons.YesNo);
                                    goto Label;
                                }

                            }

                            textBox1.Text += "\r\n==> The hostname you entered is : " + customHostName + "\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            progressBar1.Value = 90;

                            textBox1.Text += "\r\n========SPNs needed for kerberos to work========\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            if (isUsingPoolIdentity == true)
                            {
                                UName = pool.ProcessModel.UserName;
                                try
                                {
                                    string fqdn = System.Net.Dns.GetHostEntry(customHostName).HostName;

                                    textBox1.Text += "Below SPNs should be on account: " + UName + "\r\n\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();

                                    textBox1.Text += "HTTP/" + customHostName + "\r\n";
                                    if (fqdn != customHostName)
                                    {
                                        textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                    }
                                    
                                }
                                catch (Exception e2)
                                {
                                    textBox1.Text += "\r\nError\r\n=======\r\n" + e2 + "\r\n\r\n";
                                    MessageBox.Show("" + e2, "Fatal Error!");
                                }

                            }
                            else
                            {
                                string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");


                                textBox1.Text += "Below SPNs should be on account: " + computerName + "\r\n\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();

                                textBox1.Text += "HTTP/" + customHostName + "\r\n";
                                try
                                {
                                    string fqdn = System.Net.Dns.GetHostEntry(customHostName).HostName;

                                    if (fqdn != customHostName)
                                    {
                                        textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                    }
                                    
                                }

                                catch (Exception e1)
                                {
                                    textBox1.Text += "\r\nError\r\n=======\r\n" + e1 + "\r\n\r\n";
                                    MessageBox.Show("" + e1, "Fatal Error!");
                                }
                            }

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "\r\n==> You are not using any hostname.\r\n\r\n========SPNs needed for kerberos to work========\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            if (isUsingPoolIdentity == true)
                            {
                                UName = pool.ProcessModel.UserName;
                                string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

                                textBox1.Text += "Below SPNs should be on account: " + UName + "\r\n\r\n";

                                textBox1.Text += "HTTP/" + computerName + "\r\n";
                                try
                                {
                                    string fqdn = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;

                                    textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                    
                                    textBox1.Text += "===============================================================\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();
                                }
                                catch (Exception e3)
                                {
                                    textBox1.Text += "\r\nError\r\n=======\r\n" + e3 + "\r\n\r\n";
                                    MessageBox.Show("" + e3, "Fatal Error!");
                                }

                            }
                            else
                            {
                                try
                                {
                                    string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

                                    textBox1.Text += "Below SPNs should be on account: " + computerName + "\r\n\r\n";

                                    textBox1.Text += "HOST/" + computerName + "\r\n";
                                    string fqdn = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                                    textBox1.Text += "HOST/" + fqdn + "\r\n";
                                    
                                    textBox1.Text += "===============================================================\r\n";
                                    Thread.Sleep(100);
                                    System.Windows.Forms.Application.DoEvents();
                                }
                                catch (Exception e4)
                                {
                                    textBox1.Text += "\r\nError\r\n=======\r\n" + e4 + "\r\n\r\n";
                                    MessageBox.Show("" + e4, "Fatal Error!");
                                    textBox1.Text += "===============================================================\r\n";
                                }

                            }
                        }

                        progressBar1.Value = 95;

                        if (DelegationQuery == true)
                        {
                            textBox1.Text += "\r\n========Required Delegation Settings========\r\n";
                            textBox1.Text += "===============================================================\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();

                            if (isDelegationSet == true)
                            {
                                if (isConstrainedDelegationSet == true)
                                    textBox1.Text += "Delegation is already set but you need to verify the services!\r\n";
                                else
                                    textBox1.Text += "Delegation is already set!\r\n";
                            }
                            else
                            {
                                textBox1.Text += "==> Delegation is not set.\r\n==> We need to configure Either Constrained or Unconstrained Delegation.\r\n";
                            }
                            textBox1.Text += "===============================================================\r\n";
                        }
                        progressBar1.Value = 100;
                        serverMgr.CommitChanges();
                        button2.Enabled = false;
                        button1.Enabled = true;
                        StreamWriter w = new StreamWriter(filename, append: true);
                        string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                        w.WriteLine("Currently Logged-in User : " + userName + "\r\n");
                        w.WriteLine(textBox1.Text);
                        w.Close();
                        MessageBox.Show(text: "Review for the selected website completed successfully!", caption: "Success!");
                        radioButton1.Enabled = true;
                        radioButton2.Enabled = true;
                    }
                }

                else
                {
                    button2.Enabled = true;
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                    return;
                }

            }
            catch(Exception ex)
            {
                textBox1.Text += "\r\nError\r\n=======\r\n" + ex + "\r\n\r\n";
                MessageBox.Show("" + ex, "Fatal Error!");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                button3.Enabled = false;
                DialogResult result = MessageBox.Show("Are you sure you want to revert the changes for this website?", "Alert!", MessageBoxButtons.YesNo);
                if (result.ToString().Equals("Yes"))
                {
                    button4.Enabled = false;
                    textBox1.Text = "Reverting changes\r\n======================\r\n";
                    var serverMgr = new ServerManager();
                    Configuration config = serverMgr.GetApplicationHostConfiguration();
                    string selectedSite = this.comboBox2.GetItemText(this.comboBox2.SelectedItem);
                    textBox1.Text += "Selected Site : " + selectedSite + "\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    if (isAnonymousChanged == true)
                    {
                        ConfigurationSection anonymousAuthenticationSection = config.GetSection("system.webServer/security/authentication/anonymousAuthentication", selectedSite);
                        bool anonymous = (bool)anonymousAuthenticationSection["enabled"];
                        if (anonymous == false)
                        {
                            textBox1.Text += "Reverting Anonymous auth...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            anonymousAuthenticationSection["enabled"] = true;
                            textBox1.Text += "Anonymous authentication reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "Reverting Anonymous auth...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            anonymousAuthenticationSection["enabled"] = false;
                            textBox1.Text += "Anonymous authentication reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    else
                    {
                        textBox1.Text += "Did not revert Anonymous authentication as it was not changed...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (isBasicChanged == true)
                    {
                        ConfigurationSection basicAuthenticationSection = config.GetSection("system.webServer/security/authentication/basicAuthentication", selectedSite);
                        bool basic = (bool)basicAuthenticationSection["enabled"];
                        if (basic == false)
                        {
                            textBox1.Text += "Reverting basic auth...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            basicAuthenticationSection["enabled"] = true;
                            textBox1.Text += "Basic authentication reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "Reverting Basic auth...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            basicAuthenticationSection["enabled"] = false;
                            textBox1.Text += "Basic authentication reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    else
                    {
                        textBox1.Text += "Did not revert Basic authentication as it was not changed...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (isDigestChanged == true)
                    {
                        ConfigurationSection digestAuthenticationSection = config.GetSection("system.webServer/security/authentication/digestAuthentication", selectedSite);
                        bool digest = (bool)digestAuthenticationSection["enabled"];
                        if (digest == false)
                        {
                            textBox1.Text += "Reverting digest auth...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            digestAuthenticationSection["enabled"] = true;
                            textBox1.Text += "digest authentication reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "Reverting digest auth...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            digestAuthenticationSection["enabled"] = false;
                            textBox1.Text += "digest authentication reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    else
                    {
                        textBox1.Text += "Did not revert digest authentication as it was not changed...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (isAspnetImpersonationChanged == true)
                    {
                        Configuration config1 = serverMgr.GetWebConfiguration(selectedSite);
                        ConfigurationSection identitySection = config1.GetSection("system.web/identity");
                        bool aspnetimpersonation = (bool)identitySection["impersonate"];
                        if (aspnetimpersonation == false)
                        {
                            textBox1.Text += "Reverting aspnetimpersonation...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            identitySection["impersonate"] = true;
                            textBox1.Text += "aspnetimpersonation reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "Reverting aspnetimpersonation...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            identitySection["impersonate"] = false;
                            textBox1.Text += "aspnetimpersonation reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    else
                    {
                        textBox1.Text += "Did not revert aspnetimpersonation as it was not changed...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    ConfigurationSection windowsAuthenticationSection = config.GetSection("system.webServer/security/authentication/windowsAuthentication", selectedSite);

                    if (isWindowsChanged == true)
                    {

                        bool windows = (bool)windowsAuthenticationSection["enabled"];
                        if (windows == false)
                        {
                            textBox1.Text += "Reverting windows auth...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            windowsAuthenticationSection["enabled"] = true;
                            textBox1.Text += "windows auth reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "Reverting windows auth...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            windowsAuthenticationSection["enabled"] = false;
                            textBox1.Text += "Windows auth reverted...\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    else
                    {
                        textBox1.Text += "Did not revert Windows auth as it was not changed...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (isNegotiateChanged == true)
                    {

                        ConfigurationElementCollection providersCollection = windowsAuthenticationSection.GetCollection("providers");
                        ConfigurationElement searchNegotiate = FindElement(providersCollection, "add", "value", @"Negotiate");
                        ConfigurationElement searchOnPriority = FindElement(providersCollection, "add", "value", onPriority);
                        textBox1.Text += "Reverting Providers...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        searchNegotiate["value"] = onPriority;
                        searchOnPriority["value"] = "Negotiate";
                        textBox1.Text += "Reverted Providers...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        textBox1.Text += "Did not revert Providers as it was not changed...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (isUseAppPoolChanged == true)
                    {
                        textBox1.Text += "Reverting useAppPoolCredentials...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        bool useAppPool = (bool)windowsAuthenticationSection["useAppPoolCredentials"];

                        if (useAppPool == true)
                            windowsAuthenticationSection["useAppPoolCredentials"] = false;
                        else
                            windowsAuthenticationSection["useAppPoolCredentials"] = true;

                        textBox1.Text += "Reverted useAppPoolCredentials...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        textBox1.Text += "Did not revert useAppPoolCredentials as it was not changed..\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (isUseKernelChanged == true)
                    {

                        textBox1.Text += "Reverting useKernelMode...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        bool useKernel = (bool)windowsAuthenticationSection["useKernelMode"];

                        if (useKernel == true)
                            windowsAuthenticationSection["useKernelMode"] = false;
                        else
                            windowsAuthenticationSection["useKernelMode"] = true;

                        textBox1.Text += "Reverted userKernelMode...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        textBox1.Text += "Did not revert useKernelMode as it was not changed...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    textBox1.Text += "=======================================\r\n";

                    button3.Enabled = false;
                    serverMgr.CommitChanges();
                    StreamWriter w = new StreamWriter(filename, append: true);
                    w.WriteLine(DateTime.Now.ToString() + "\r\n----------------------------------------------------------\r\n");
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    w.WriteLine("Currently Logged-in User : " + userName + "\r\n");
                    w.WriteLine(textBox1.Text);
                    w.Close();
                    MessageBox.Show("All the settings have been reverted successfully!", "Success!");
                }
                else
                {
                    button3.Enabled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                textBox1.Text += "\r\nError\r\n=======\r\n" + ex + "\r\n\r\n";
                MessageBox.Show("" + ex, "Fatal Error!");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 250,
                Height = 165,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label()
            {
                Left = 65,
                Top = 25,
                Text = text,
                Width = 150
            };
            TextBox textBox = new TextBox()
            {
                Left = 20,
                Top = 50,
                Width = 200
            };
            Button confirmation = new Button()
            {
                Text = "Ok",
                Left = 90,
                Width = 50,
                Top = 80,
                DialogResult = DialogResult.OK
            };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}