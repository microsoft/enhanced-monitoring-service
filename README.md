How to install Enhanced Monitoring Service on Windows Server 2012 R2
======
This tools Provides enhanced data monitoring for virtual machines. It allows you to monitor the performance and resource status of the physical server from within the virtual machine.
	
Prerequisites:
------
None
	
Installation:
------
1. Download the EnhancedMonitoring.msi package from here(TODO: add link)
2. Run MSI to install the Enhanced Monitoring Service
![Installer](/doc/installer.png)

Validate the installation
------
1. After installation, confirm you can find the "Enhanced Monitoring Provider Service" service from the services console, and it should be automatically started.	
![Installer](/doc/service.png)

2. If any issue occurs, you can find log file named "monitor.log" under c:\ProgramData\EnhancedMonitoring\log for trouble shooting.
![Installer](/doc/log.png)
