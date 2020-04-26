Many of us find troubleshooting Kerberos quite a tedious task since it involves multiple levels of troubleshooting. Today, inorder to configure kerberos on IIS server we need to go through the set of steps which is really time consuming and of high complexity.

<b>Why is Kerberos painful at times?</b>

<ul>
<li>First, understanding Kerberos is quite tricky.</li>
<li>Configuration takes a lot of time as we need to look at configuration of IIS server, Domain controller and Client side.</li>
<li>Once a site breaks because of some configuration related issues, it’s really difficult to identify the exact cause because of the complexity.</li>
</ul>

To address these issues, I have created the “Kerberos Configuration Manager for IIS”. This tool configures Kerberos single hop on any site on IIS by reading and modifying the Configration files. This reduces both time spent.This tool allows one to do the following tasks:

<ul>
<li>Review the current settings related to Kerberos for any specific website in IIS.</li>
<li>Configures Kerberos for the affected website</li>
<li>It also has a provision to revert the changes made just in case there is a requirement.</li>
<li>It also has a feature of auditing through a log file.</li>
</ul>

**Let’s see what exactly happens “Under the Hood”:**

At a high level, the below steps needs to be followed to configure Kerberos for a website:

**On IIS Server:**

1.	Disable all the authentication methods except windows authentication
2.	In windows authentication section, in Providers we should see negotiate should be a priority
3.	Based on the Application pool credentials,
  - useAppPoolCredentials to true if we are using a custom account
  - useAppPoolCredentials to false and useKernelMode to true if we are using a machine account

**On Domain Controller:**

  - Based on the Application pool credentials on the IIS we set Service Principle names on the DC.
    - If we use a Machine account, set SPNs on Machine account
  - If we use a custom account, set SPNs on custom account
    - The above also depends on whether we are using a hostname or machine to browse the website

**On Client Browser(Internet Explorer):**

- Based on whether we use hostname or not, we need to add the host/machine name to Trusted sites/Local Intranet Zone.

**You can find more information regarding Configuration of Kerberos in the below blogs:**

- [All-about-kerberos-the-three-headed-dog-with-respect-to-iis-and-sql](https://blogs.msdn.microsoft.com/chiranth/2013/09/20/all-about-kerberos-the-three-headed-dog-with-respect-to-iis-and-sql/)
- [Setting-up-kerberos-authentication-for-a-website-in-iis](https://blogs.msdn.microsoft.com/chiranth/2014/04/17/setting-up-kerberos-authentication-for-a-website-in-iis/)


Now just imagine if we can automate the above process through a nifty application which can help us troubleshoot/configure Kerberos in just a few minutes – Is it possible? The good news is that NOW IT IS POSSIBLE 

I have developed a simple troubleshooter “Kerberos Configuration Manager for IIS” which allows one to do the following tasks:

1. Review the current settings related to Kerberos for any specific website in IIS. 
      - Checks and displays the site properties
      - Checks and displays Application pool properties like Application pool identity
      - Checks and displays Anonymous authentication properties
      - Checks and displays Basic authentication properties
      - Checks and displays Digest authentication properties
      - Checks and displays ASP.NET Impersonation properties
      - Checks and displays Windows authentication
        * whether Windows authentication is enabled or disabled
        * What are the Providers settings
      - Checks and displays Configuration editor settings for windows authentication
        * UseAppPoolCredentials settings 
        * UseKernelMode settings
      - Based on the Application pool identity, 
        * Checks for the existing SPNs for that identity and displays them
        * Displays the necessary SPNs required for Kerberos to work 

2.	Configures Kerberos for the affected website:
a.	Disables Anonymous authentication if enabled
b.	Disables Basic authentication if enabled
c.	Disables Digest authentication if enabled
d.	Disables ASP.NET Impersonation if enabled
e.	Enables Windows authentication if disabled
i.	Once the above is enabled, checks whether we have Negotiate on priority or no. If not, Negotiate is moved to the top
f.	Based on the application pool credentials,
i.	Either it will enable useAppPoolCredentials or disables it
ii.	Either it will enable useKernelMode or disables it
g.	Based on the Application pool identity, 
i.	Checks for the existing SPNs for that identity and displays them
ii.	Displays the necessary SPNs required for Kerberos to work 
h.	Generates the script for setting the required SPNs in the same directory

3.	It also has a provision to revert the changes made just in case there is a requirement.

4.	It also has a feature of auditing through a log file which would capture the below details:
a.	Logged in user who used the tool and made changes
b.	Timestamp when the changes were made
c.	Review, Configure and Revert logs (All settings which were added/modified)

The good news is that we have released the Kerberos Configuration Manager v2.0 which supports reviewing and configuring the Kerberos Pass-through authentication also (Kerberos Double Hop).

**Whats new in Kerberos Configuration Manager v2.0 ?**

- Double Hop Support - Delegation and Impersonation
- UI changes
- Scroll view for the text area
- Application can run as administrator without any intervention
- Bug fixes

**Why should I use tool?:**

- Troubleshooting Kerberos just becomes much simpler with this tool and it optimizes the time taken to troubleshoot from few hours to few minutes.
- You can review the Kerberos Configuration for any of your web sites and share the generated log files with support to save precious troubleshooting time.
- No need to install the tool -  it’s a standalone executable.
- Disk space utilization is minimal.
- Open source, free to download and modify.
- Auditing support which makes troubleshooting Kerberos easier.

**Where do I get it from and how do I use it?**

The tool can be downloaded from the open source github repo:

**Latest release:**  https://github.com/SurajDixit/KerberosConfigMgrIIS/releases/download/v2.1/KerberosConfigMgrIIS.exe

**All releases:**  https://github.com/SurajDixit/KerberosConfigMgrIIS/releases

The GUI has a fairly simple layout with the options to Review, Configure, Generate Script and Revert the Kerberos related configuration settings.

**Instructions for use:**

https://docs.microsoft.com/en-us/archive/blogs/surajdixit/kerberos-configuration-manager-for-internet-information-services-server
