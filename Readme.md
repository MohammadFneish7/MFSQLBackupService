<h1> <img src="https://github.com/MohammadFneish7/MFSQLBackupService/blob/master/icon_x36.png"
  style="float:left;">
MFSQLBackupService</h1>

An Automated SQL Server backup service for windows (coded via VB.NET) that workes on taking a full database backup on a schedule.

# Installation

1. Preform: git clone https://github.com/MohammadFneish7/MFSQLBackupService.git
2. After cloning, navigate to **~/MFSQLBackupService/bin/Release/**
3. Edit the settings file **MFSQLBackupService.exe.config** to fit your case:
```
            <setting name="BackupDirectory" serializeAs="String">
                <value>C:\Backup</value>
            </setting>
            <setting name="ConnectionString" serializeAs="Xml">
                <value>
                    <ArrayOfString xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                        <string>Server=DESKTOP-O3TSL14;Database=EEMS;Persist Security Info=True;Integrated Security=true;</string>
                    </ArrayOfString>
                </value>
            </setting>
            <setting name="DatabaseName" serializeAs="String">
                <value>EEMS, ClientDB</value>
            </setting>
            <setting name="AutoDeleteDelay" serializeAs="String">
                <value>10</value>
            </setting>
            <setting name="AutoBackupTime" serializeAs="Xml">
                <value>
                    <ArrayOfString xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                        <string>0</string>
                        <string>12</string>
                        <string>21</string>
                    </ArrayOfString>
                </value>
            </setting>
```
  * **BackupDirectory**: A String that represents the backup directory
  * **ConnectionString**: A String Collection that represents the SQL Server instances connection strings that will be scheduled for the backup
  * **AutoDeleteDelay**:  A Short number that represents the time in days to keep the backup and log files before automatic deletion
  * **AutoBackupTime**: A String Collection of numbers in range of 0 to 23 that represents the time in Hours to take the backup within

4. Run the **install.bat** file

Note: you can stop or start the service by running the **stop.bat** or **start.bat** files respectively.

# Uninstall
To stop and uninstall the service simply run the **uninstall.bat** file, or type ```sc delete MFSQLBackupService``` into cmd with administrative access.

  # License
This project is licensed under [GNU General Public License][1].
    
    
[1]: https://github.com/MohammadFneish7/MFSQLBackupService/blob/master/LICENSE
  
