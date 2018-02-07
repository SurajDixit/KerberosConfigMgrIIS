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

<b>Why should I use tool?</b>

<ul>
<li>Troubleshooting Kerberos just becomes much simpler with this tool and it optimizes the time taken to troubleshoot from few hours to few minutes.</li>
<li>You can review the Kerberos Configuration for any of your web sites and share the generated log files with support to save precious troubleshooting time.</li>
<li>No need to install the tool -  it’s a standalone executable.</li>
<li>Disk space utilization is minimal.</li>
<li>Open source, free to download and modify.</li>
<li>Auditing support which makes troubleshooting Kerberos easier.</li>
</ul>	

For documentation on how to use, follow the below blog: 

https://blogs.msdn.microsoft.com/surajdixit/2018/02/07/kerberos-configuration-manager-for-internet-information-services-server/
