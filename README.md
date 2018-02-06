This is a simple troubleshooter “Kerberos Configuration Manager for IIS” which allows one to do the following tasks:

1. Review the current settings related to Kerberos for any specific website in IIS.
	-> Checks and displays the site properties
	-> Checks and displays Application pool properties like Application pool identity
	-> Checks and displays Anonymous authentication properties
	-> Checks and displays Basic authentication properties	
	-> Checks and displays Digest authentication properties
	-> Checks and displays ASP.NET Impersonation properties
	-> Checks and displays Windows authentication
		-> whether Windows authentication is enabled or disabled
		-> What are the Providers settings
	-> Checks and displays Configuration editor settings for windows authentication
		-> UseAppPoolCredentials settings
		-> UseKernelMode settings
	-> Based on the Application pool identity,
		-> Checks for the existing SPNs for that identity and displays them
		-> Displays the necessary SPNs required for Kerberos to work

2. Configures Kerberos for the affected website:
	-> Disables Anonymous authentication if enabled
	-> Disables Basic authentication if enabled
	-> Disables Digest authentication if enabled
	-> Disables ASP.NET Impersonation if enabled
	-> Enables Windows authentication if disabled
		-> Once the above is enabled, checks whether we have Negotiate on priority or no. If not, Negotiate is moved to the top
	-> Based on the application pool credentials,
		-> Either it will enable useAppPoolCredentials or disables it
		-> Either it will enable useKernelMode or disables it
	-> Based on the Application pool identity,
		-> Checks for the existing SPNs for that identity and displays them
	-> Displays the necessary SPNs required for Kerberos to work
	-> Generates the script for setting the required SPNs in the same directory

3. It also has a provision to revert the changes made just in case there is a requirement.
4. It also has a feature of auditing through a log file which would capture the below details:
	-> Logged in user who used the tool and made changes
	-> Timestamp when the changes were made
	-> Review, Configure and Revert logs (All settings which were added/modified)
	
For documentation follow the below blog: 

