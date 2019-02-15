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
            <setting name="ConnectionString" serializeAs="String">
                <value>Server=DESKTOP-O3TSL14;Database=EEMS;Persist Security Info=True;Integrated Security=true;</value>
            </setting>
            <setting name="DatabaseName" serializeAs="String">
                <value>EEMS, ClientDB</value>
            </setting>
            <setting name="AutoDeleteDelay" serializeAs="String">
                <value>10</value>
            </setting>
            <setting name="AutoBackupTime" serializeAs="String">
                <value>1,6,15,21</value>
            </setting>
```
  * **AutoBackupTime**: A String of numbers splitted by "," between 0 and 23 that represents the time as Hour to take the backup
  * **BackupDirectory**: A String that represents the backup directory
  * **ConnectionString**: A String that represents the SQL Server instance connection string
  * **DatabaseName**:  A String of databases' names to backup splitted by ","
  * **AutoDeleteDelay**:  A Short number that represents the time in days to keep the backup and log files before automatic deletion

4. Run the **install.bat** file

Note: you can stop or start the service by running the **stop.bat** or **start.bat** files respectively.

# Uninstall
To stop and uninstall the service simply run the **uninstall.bat** file, or type ```sc delete MFSQLBackupService``` into cmd with administrative access.

  # License
This project is licensed under [GNU General Public License][1].
    
    
[1]: https://github.com/MohammadFneish7/MFSQLBackupService/blob/master/LICENSE
  
