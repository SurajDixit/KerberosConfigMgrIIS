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

namespace KerberosConfigMgr
{
    public partial class Kerberos : Form
    {
        static string onPriority = null;
        static int count = 0;
        List<string> spnValue = new List<string>();
        bool isAppPoolSetGlobal = false;
        string UserGlobal;
        static string day = DateTime.Today.Date.Day.ToString();
        static string month = DateTime.Today.Date.Month.ToString();
        static string year = DateTime.Today.Date.Year.ToString();
        static string filename = "k_log" + "_" + day + "_" + month + "_" + year + ".log";

        public Kerberos()
        {

            InitializeComponent();
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
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
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.WordWrap = true;
            button4.Enabled = false;
            button1.Enabled = false;
            button3.Enabled = false;
            button2.Enabled = false;
            StreamWriter w = new StreamWriter(filename, append: true);
            string date = DateTime.Now.ToString();
            w.WriteLine("Start:" + date + "\r\n----------------------------------------------------------\r\n");
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
            DialogResult result2 = MessageBox.Show("Are you sure you want to configure Kerberos for this website?", "Alert!", MessageBoxButtons.YesNo);
            if (result2.ToString().Equals("Yes"))
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
                    textBox1.Text = "Configure\r\n===================\r\n\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    textBox1.Text += "Selected Site : " + selectedSite + "\r\n";
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

                    textBox1.Text += "Sitename : " + site.Name + "\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    progressBar1.Value = 10;

                    String getPool;
                    if (isAnApplication == true)
                    {
                        textBox1.Text += "Application name : " + app1 + "\r\n";
                        Microsoft.Web.Administration.Application application = site.Applications["/" + app1];
                        getPool = application.ApplicationPoolName;
                        selectedSite = selectedSite2;
                    }
                    else
                    {
                        Microsoft.Web.Administration.Application application = site.Applications["/"];
                        getPool = application.ApplicationPoolName;
                    }

                    textBox1.Text += "Application Pool : " + getPool + "\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    progressBar1.Value = 20;

                    Configuration config = serverMgr.GetApplicationHostConfiguration();
                    ConfigurationSection anonymousAuthenticationSection = config.GetSection("system.webServer/security/authentication/anonymousAuthentication", selectedSite);
                    bool AnonymousBool = (bool)anonymousAuthenticationSection["enabled"];
                    if (AnonymousBool == true)
                    {
                        anonymousAuthenticationSection["enabled"] = false;
                        textBox1.Text += "Anonymous authentication is disabled..\r\n";
                        isAnonymousChanged = true;
                    }
                    else
                    {
                        textBox1.Text += "Anonymous authentication is already disabled..\r\n";
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
                        textBox1.Text += "Basic authentication is disabled..\r\n";
                        isBasicChanged = true;
                    }
                    else
                    {
                        textBox1.Text += "Basic authentication is already disabled..\r\n";
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
                        textBox1.Text += "Digest authentication is disabled..\r\n";
                        isDigestChanged = true;
                    }
                    else
                    {
                        textBox1.Text += "Digest authentication is already disabled..\r\n";
                        isDigestChanged = false;
                    }


                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    progressBar1.Value = 30;

                    Configuration config1 = serverMgr.GetWebConfiguration(selectedSite);
                    ConfigurationSection identitySection = config1.GetSection("system.web/identity");
                    bool aspnetimpersonation = (bool)identitySection["impersonate"];

                    if (aspnetimpersonation == true)
                    {
                        identitySection["impersonate"] = false;
                        textBox1.Text += "ASP.NET Impersonation is disabled..\r\n";
                        isAspnetImpersonationChanged = true;
                    }
                    else
                    {
                        textBox1.Text += "ASP.NET Impersonation is already disabled..\r\n";
                        isAspnetImpersonationChanged = false;
                    }


                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    ConfigurationSection windowsAuthenticationSection = config.GetSection("system.webServer/security/authentication/windowsAuthentication", selectedSite);
                    bool windowsBool = (bool)windowsAuthenticationSection["enabled"];
                    if (windowsBool == false)
                    {
                        windowsAuthenticationSection["enabled"] = true;
                        textBox1.Text += "Windows authentication is enabled..\r\n";
                        isWindowsChanged = true;
                    }
                    else
                    {
                        textBox1.Text += "Windows authentication is already enabled..\r\n";
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
                        textBox1.Text += "Negotiate is on top..\r\n";
                    }
                    else if (count != 1)
                    {
                        isNegotiateOnPriority = false;
                        textBox1.Text += "Negotiate is not a priority! Current Priority is " + onPriority + "..\r\n";
                    }

                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    progressBar1.Value = 50;

                    count = 0;

                    if (isNegotiateOnPriority == false)
                    {
                        textBox1.Text += "Making Negotiate the priority...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        ConfigurationElement searchOnPriority = FindElement(providersCollection, "add", "value", onPriority);
                        if (count == 1)
                        {
                            searchOnPriority["value"] = "Negotiate";
                            searchNegotiate["value"] = onPriority;
                            textBox1.Text += "Negotiate is the priority..\r\n";
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


                    if (poolIdentity == "SpecificUser")
                    {
                        isUsingPoolIdentity = true;
                        UName = pool.ProcessModel.UserName;
                        textBox1.Text += "You are using a custom identity : " + UName + "..\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        isUsingPoolIdentity = false;
                        textBox1.Text += "You are using a builtin account : " + poolIdentity + "..\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (isUsingPoolIdentity == true)
                    {
                        isUseAppPoolChanged = (bool)windowsAuthenticationSection["useAppPoolCredentials"];
                        isUseKernelChanged = false;
                        if (isUseAppPoolChanged == true)
                        {
                            isUseAppPoolChanged = false;
                            textBox1.Text += "useAppPoolCredentials Already set to true..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "Setting useAppPoolCredentials to true..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            windowsAuthenticationSection["useAppPoolCredentials"] = true;
                            textBox1.Text += "useAppPoolCredentials set to true..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            isUseAppPoolChanged = true;
                        }

                    }
                    else if (isUsingPoolIdentity == false)
                    {
                        isUseAppPoolChanged = (bool)windowsAuthenticationSection["useAppPoolCredentials"];
                        isUseKernelChanged = (bool)windowsAuthenticationSection["useKernelMode"];
                        if (isUseAppPoolChanged == false)
                        {
                            isUseAppPoolChanged = false;
                            textBox1.Text += "useAppPoolCredentials Already set to false..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "Setting useAppPoolCredentials to false..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            windowsAuthenticationSection["useAppPoolCredentials"] = false;
                            textBox1.Text += "useAppPoolCredentials set to false..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            isUseAppPoolChanged = true;
                        }
                        if (isUseKernelChanged == true)
                        {
                            isUseAppPoolChanged = false;
                            textBox1.Text += "useKernelMode Already set to true..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                        {
                            textBox1.Text += "Setting useKernelMode to true..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            windowsAuthenticationSection["useKernelMode"] = true;
                            textBox1.Text += "useKernelMode set to true..\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                            isUseKernelChanged = true;
                        }
                    }

                    textBox1.Text += "Fetching SPNs for the account set for Application pool identity..\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    if (isUsingPoolIdentity == true)
                    {
                        isAppPoolSetGlobal = isUsingPoolIdentity;
                        bool isSetSPNsSmall = false;
                        UName = pool.ProcessModel.UserName;
                        bool isSetSPNsCaps = false;
                        textBox1.Text += "\r\nBelow are the SPNs set for the Custom account: " + UName + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        UName = pool.ProcessModel.UserName;
                        UserGlobal = UName;
                        string[] split = UName.Split('\\');
                        foreach (string value in ListSPN("HTTP", split[1]))
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

                        foreach (string value in ListSPN("http", split[1]))
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
                            textBox1.Text += "No SPNs set for this account\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        textBox1.Text += "==================================================\r\n";
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
                        textBox1.Text += "\r\nBelow are the SPNs set for the " + computerName + " account:\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        foreach (string value in ListSPN("HOST", computerName))
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

                        foreach (string value in ListSPN("host", computerName))
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
                            textBox1.Text += "No SPNs set for this account\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    progressBar1.Value = 70;

                    DialogResult result1 = MessageBox.Show("Are you using hostname for the website?", "Alert!", MessageBoxButtons.YesNo);

                    if (result1.ToString().Equals("Yes"))
                    {
                        string customHostName = "";
                        while (customHostName == "")
                        {
                            customHostName = Prompt.ShowDialog("Enter the hostname:", "Important!");
                            if (customHostName == "")
                                MessageBox.Show("No hostname is entered!", "Error!");
                        }

                        textBox1.Text += "\r\nThe hostname you entered is : " + customHostName + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        textBox1.Text += "\r\nSPNs needed for kerberos to work:\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        if (isUsingPoolIdentity == true)
                        {
                            try
                            {
                                UName = pool.ProcessModel.UserName;
                                string fqdn = System.Net.Dns.GetHostEntry(customHostName).HostName;
                                textBox1.Text += "HTTP/" + customHostName + "\r\n";
                                spnValue.Add("HTTP/" + customHostName);
                                if (fqdn != customHostName)
                                {
                                    textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                    spnValue.Add("HTTP/" + fqdn);
                                }
                                textBox1.Text += "\r\nSPNs should be on account:" + UName + "\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
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
                                textBox1.Text += "HTTP/" + customHostName + "\r\n";
                                spnValue.Add("HTTP/" + customHostName);
                                string fqdn = System.Net.Dns.GetHostEntry(customHostName).HostName;
                                if (fqdn != customHostName)
                                {
                                    textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                    spnValue.Add("HTTP/" + fqdn);
                                }
                                textBox1.Text += "\r\nSPNs should be on account:" + computerName + "\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                            catch (Exception e1)
                            {
                                textBox1.Text += "\r\nError\r\n=======\r\n" + e1 + "\r\n\r\n";
                                MessageBox.Show("" + e1, "Fatal Error!");
                            }
                        }

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();


                    }
                    else
                    {
                        textBox1.Text += "\r\nYou are not using any hostname.\r\n\r\nSPNs needed for kerberos to work: \r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        if (isUsingPoolIdentity == true)
                        {
                            try
                            {
                                UName = pool.ProcessModel.UserName;
                                string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                                textBox1.Text += "HTTP/" + computerName + "\r\n";
                                spnValue.Add("HTTP/" + computerName);
                                string fqdn = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                                textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                spnValue.Add("HTTP/" + fqdn);
                                textBox1.Text += "\r\nSPNs should be on account:" + UName + "\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
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
                                textBox1.Text += "HOST/" + computerName + "\r\n";
                                spnValue.Add("HOST/" + computerName);
                                string fqdn = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                                textBox1.Text += "HOST/" + fqdn + "\r\n";
                                spnValue.Add("HOST/" + fqdn);
                                textBox1.Text += "\r\nSPNs should be on account:" + computerName + "\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                            catch (Exception e3)
                            {
                                textBox1.Text += "\r\nError\r\n=======\r\n" + e3 + "\r\n\r\n";
                                MessageBox.Show("" + e3, "Fatal Error!");
                            }

                        }

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                    }

                    progressBar1.Value = 80;
                    textBox1.Text += "\r\nYou can download cmdlet by clicking the below button to set SPNs\r\n";
                    textBox1.Text += "=====================================================\r\n";
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
                }
            }
        }

        private void InputBox()
        {
            throw new NotImplementedException();
        }

        ArrayList ListSPN(string ServiceType, string AccountName = "")
        {
            ArrayList SPNs = new ArrayList();
            string spnfilter = "(servicePrincipalName={0}/*)";
            spnfilter = String.Format(spnfilter, ServiceType);
            System.DirectoryServices.DirectoryEntry domain = new DirectoryEntry();
            System.DirectoryServices.DirectorySearcher searcher = new DirectorySearcher();
            searcher.SearchRoot = domain;
            searcher.PageSize = 1000;
            searcher.Filter = spnfilter;
            SearchResultCollection results;
            try {
                results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    DirectoryEntry account = result.GetDirectoryEntry();
                    if (account.Properties["sAMAccountName"].Value.ToString().Contains(AccountName))
                    {
                        foreach (string spn in account.Properties["servicePrincipalName"])
                        {
                            if (spn.Contains(ServiceType))
                            {
                                SPNs.Add(spn);
                            }
                        }
                    }
                }
                
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
            button2.Enabled = true;
        }

        private void Kerberos_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (File.Exists("spn.cmd"))
            {
                File.Delete("spn.cmd");
            }

            using (StreamWriter fs = File.CreateText("spn.cmd"))
            {
                foreach (string spn in spnValue)
                {
                    fs.WriteLine("setspn -s " + spn + " " + UserGlobal);
                }

                fs.Close();
            }
            spnValue.Clear();
            MessageBox.Show("File saved to current directory...", "Success!");
            button4.Enabled = false;
        }
        string app1;
        string selectedSite1;
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to Review the current settings for this website?", "Alert!", MessageBoxButtons.YesNo);
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

                    textBox1.Text = "Review\r\n===================\r\n\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    textBox1.Text += "Selected Site : " + selectedSite + "\r\n";
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

                    textBox1.Text += "Sitename : " + site.Name + "\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    progressBar1.Value = 10;
                    String getPool;
                    if (isAnApplication == true)
                    {
                        textBox1.Text += "Application name : " + app1 + "\r\n";
                        Microsoft.Web.Administration.Application application = site.Applications["/" + app1];
                        getPool = application.ApplicationPoolName;
                        selectedSite = selectedSite1;
                    }
                    else
                    {
                        Microsoft.Web.Administration.Application application = site.Applications["/"];
                        getPool = application.ApplicationPoolName;
                    }
                    textBox1.Text += "Application Pool : " + getPool + "\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    progressBar1.Value = 20;

                    Configuration config = serverMgr.GetApplicationHostConfiguration();
                    ConfigurationSection anonymousAuthenticationSection = config.GetSection("system.webServer/security/authentication/anonymousAuthentication", selectedSite);
                    bool anonymous = (bool)anonymousAuthenticationSection["enabled"];
                    if (anonymous == true)
                        textBox1.Text += "Anonymous authentication is enabled. It should be disabled for Kerberos Authentication...\r\n";
                    else
                        textBox1.Text += "Anonymous authentication is disabled..\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    ConfigurationSection basicAuthenticationSection = config.GetSection("system.webServer/security/authentication/basicAuthentication", selectedSite);
                    bool basic = (bool)basicAuthenticationSection["enabled"];
                    if (basic == true)
                        textBox1.Text += "Basic authentication is enabled. It should be disabled for Kerberos Authentication...\r\n";
                    else
                        textBox1.Text += "Basic authentication is disabled..\r\n";

                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    ConfigurationSection digestAuthenticationSection = config.GetSection("system.webServer/security/authentication/digestAuthentication", selectedSite);
                    bool digest = (bool)digestAuthenticationSection["enabled"];
                    if (digest == true)
                        textBox1.Text += "Digest authentication is enabled. It should be disabled for Kerberos Authentication...\r\n";
                    else
                        textBox1.Text += "Digest authentication is disabled..\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();

                    Configuration config1 = serverMgr.GetWebConfiguration(selectedSite);
                    ConfigurationSection identitySection = config1.GetSection("system.web/identity");

                    bool aspnetimpersonation = (bool)identitySection["impersonate"];

                    if (aspnetimpersonation == true)
                        textBox1.Text += "ASP.NET Impersonation is enabled..\r\n";
                    else
                        textBox1.Text += "ASP.NET Impersonation is disabled..\r\n";

                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    progressBar1.Value = 30;

                    ConfigurationSection windowsAuthenticationSection = config.GetSection("system.webServer/security/authentication/windowsAuthentication", selectedSite);
                    bool win = (bool)windowsAuthenticationSection["enabled"];

                    if (win == false)
                        textBox1.Text += "Windows authentication is disabled. It should be enabled for Kerberos Authentication...\r\n";
                    else
                        textBox1.Text += "Windows authentication is enabled. It should be disabled for Kerberos Authentication...\r\n";
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
                        textBox1.Text += "Negotiate is on priority..\r\n";
                    }
                    else if (count != 1)
                    {
                        isNegotiateOnPriority = false;
                        textBox1.Text += "Negotiate is not a priority! Current Priority is " + onPriority + "..\r\n";
                        textBox1.Text += "Negotiate should be a top priority..\r\n";
                    }

                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    progressBar1.Value = 50;
                    count = 0;
                    progressBar1.Value = 60;

                    ApplicationPool pool = serverMgr.ApplicationPools[getPool];
                    String poolIdentity = pool.ProcessModel.IdentityType.ToString();

                    if (poolIdentity == "SpecificUser")
                    {
                        isUsingPoolIdentity = true;
                        UName = pool.ProcessModel.UserName;
                        textBox1.Text += "You are using a custom identity : " + UName + "...\r\n";
                        textBox1.Text += "We should have useAppPoolCredentials set to true...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        isUsingPoolIdentity = false;
                        textBox1.Text += "You are using a builtin account : " + poolIdentity + "..\r\n";
                        textBox1.Text += "We should have useAppPoolCredentials set to false and useKernelMode set to true...\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (isUsingPoolIdentity == true)
                    {
                        bool useAppPool = (bool)windowsAuthenticationSection["useAppPoolCredentials"];
                        if (useAppPool == true)
                            textBox1.Text += "useAppPoolCredentials set to true..\r\n";
                        else
                            textBox1.Text += "useAppPoolCredentials set to false..\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        bool useKernel = (bool)windowsAuthenticationSection["useKernelMode"];
                        if (useKernel == true)
                            textBox1.Text += "useKernelMode set to true..\r\n";
                        else
                            textBox1.Text += "useKernelMode set to false..\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else if (isUsingPoolIdentity == false)
                    {
                        bool useAppPool = (bool)windowsAuthenticationSection["useAppPoolCredentials"];
                        if (useAppPool == true)
                            textBox1.Text += "useAppPoolCredentials set to true..\r\n";
                        else
                            textBox1.Text += "useAppPoolCredentials set to false..\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        bool useKernel = (bool)windowsAuthenticationSection["useKernelMode"];
                        if (useKernel == true)
                            textBox1.Text += "useKernelMode set to true..\r\n";
                        else
                            textBox1.Text += "useKernelMode set to false..\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    textBox1.Text += "Fetching SPNs for the account set for Application pool identity..\r\n";
                    Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                    if (isUsingPoolIdentity == true)
                    {
                        isAppPoolSetGlobal = isUsingPoolIdentity;
                        bool isSetSPNsSmall = false;
                        UName = pool.ProcessModel.UserName;
                        bool isSetSPNsCaps = false;
                        textBox1.Text += "\r\nBelow are the SPNs set for the Custom account: " + UName + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        UName = pool.ProcessModel.UserName;
                        UserGlobal = UName;
                        string[] split = UName.Split('\\');
                        foreach (string value in ListSPN("HTTP", split[1]))
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

                        foreach (string value in ListSPN("http", split[1]))
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
                            textBox1.Text += "No SPNs set for this account\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        textBox1.Text += "==================================================\r\n";
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
                        textBox1.Text += "\r\nBelow are the SPNs set for the " + computerName + " account:\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                        foreach (string value in ListSPN("HOST", computerName))
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

                        foreach (string value in ListSPN("host", computerName))
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
                            textBox1.Text += "No SPNs set for this account\r\n";
                            Thread.Sleep(100);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }

                    progressBar1.Value = 70;

                    DialogResult result1 = MessageBox.Show("Are you using hostname for the website?", "Alert!", MessageBoxButtons.YesNo);

                    if (result1.ToString().Equals("Yes"))
                    {
                        string customHostName = "";
                        while (customHostName == "")
                        {
                            customHostName = Prompt.ShowDialog("Enter the hostname:", "Important!");
                            if (customHostName == "")
                                MessageBox.Show("No hostname is entered!", "Error!");
                        }

                        textBox1.Text += "\r\nThe hostname you entered is : " + customHostName + "\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        textBox1.Text += "\r\nSPNs needed for kerberos to work:\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        if (isUsingPoolIdentity == true)
                        {
                            UName = pool.ProcessModel.UserName;
                            try
                            {
                                string fqdn = System.Net.Dns.GetHostEntry(customHostName).HostName;
                                textBox1.Text += "HTTP/" + customHostName + "\r\n";
                                if (fqdn != customHostName)
                                {
                                    textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                }
                                textBox1.Text += "\r\nSPNs should be on account:" + UName + "\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
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
                            textBox1.Text += "HTTP/" + customHostName + "\r\n";
                            try
                            {
                                string fqdn = System.Net.Dns.GetHostEntry(customHostName).HostName;
                                if (fqdn != customHostName)
                                {
                                    textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                }
                                textBox1.Text += "\r\nSPNs should be on account:" + computerName + "\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }

                            catch (Exception e1)
                            {
                                textBox1.Text += "\r\nError\r\n=======\r\n" + e1 + "\r\n\r\n";
                                MessageBox.Show("" + e1, "Fatal Error!");
                            }
                        }

                        textBox1.Text += "=======================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        textBox1.Text += "\r\nYou are not using any hostname.\r\n\r\nSPNs needed for kerberos to work: \r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                        if (isUsingPoolIdentity == true)
                        {
                            UName = pool.ProcessModel.UserName;
                            string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                            textBox1.Text += "HTTP/" + computerName + "\r\n";
                            try
                            {
                                string fqdn = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                                textBox1.Text += "HTTP/" + fqdn + "\r\n";
                                textBox1.Text += "\r\nSPNs should be on account:" + UName + "\r\n";
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
                                textBox1.Text += "HOST/" + computerName + "\r\n";
                                string fqdn = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName;
                                textBox1.Text += "HOST/" + fqdn + "\r\n";
                                textBox1.Text += "\r\nSPNs should be on account:" + computerName + "\r\n";
                                Thread.Sleep(100);
                                System.Windows.Forms.Application.DoEvents();
                            }
                            catch (Exception e4)
                            {
                                textBox1.Text += "\r\nError\r\n=======\r\n" + e4 + "\r\n\r\n";
                                MessageBox.Show("" + e4, "Fatal Error!");
                            }

                        }

                        textBox1.Text += "==================================================\r\n";
                        Thread.Sleep(100);
                        System.Windows.Forms.Application.DoEvents();

                    }
                    progressBar1.Value = 80;
                    progressBar1.Value = 100;
                    serverMgr.CommitChanges();
                    button2.Enabled = false;
                    button1.Enabled = true;
                    StreamWriter w = new StreamWriter(filename, append: true);
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    w.WriteLine("Currently Logged-in User : " + userName + "\r\n");
                    w.WriteLine(textBox1.Text);
                    w.Close();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to Revert the changes for this website?", "Alert!", MessageBoxButtons.YesNo);
            if (result.ToString().Equals("Yes"))
            {
                button4.Enabled = false;
                textBox1.Text = "Reverting changes\r\n======================\r\n\r\n";
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
                    windowsAuthenticationSection["useAppPoolCredentials"] = false;
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
                    windowsAuthenticationSection["useKernelMode"] = false;
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

                textBox1.Text += "\r\n=======================================\r\n";

                button3.Enabled = false;
                serverMgr.CommitChanges();
                StreamWriter w = new StreamWriter(filename, append: true);
                w.WriteLine(DateTime.Now.ToString() + "\r\n----------------------------------------------------------\r\n");
                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                w.WriteLine("Currently Logged-in User : " + userName + "\r\n");
                w.WriteLine(textBox1.Text);
                w.Close();
            }
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