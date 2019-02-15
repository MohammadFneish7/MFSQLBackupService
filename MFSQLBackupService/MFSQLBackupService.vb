Imports System.IO

Public Class MFSQLBackupService

    Dim Timer1 As Timers.Timer
    Dim backupHourMap As New Dictionary(Of String, Boolean)
    Dim dbNames As New List(Of String)

    Protected Overrides Sub OnStart(ByVal args() As String)
        Try
            If My.Settings.AutoBackupTime.Contains(",") Then
                Dim hours As String() = My.Settings.AutoBackupTime.ToString.Split(",")
                For Each Hour As String In hours
                    If Hour.Trim.Length > 0 Then
                        backupHourMap.Add(Hour.Trim, False)
                    End If
                Next
            Else
                If My.Settings.AutoBackupTime.Trim.Trim.Length > 0 Then
                    backupHourMap.Add(My.Settings.AutoBackupTime.Trim, False)
                End If
            End If

            If My.Settings.DatabaseName.ToString.Contains(",") Then
                For Each dbname As String In My.Settings.DatabaseName.ToString.Split(",")
                    If dbname.Trim.Length > 0 Then
                        dbNames.Add(dbname)
                    End If
                Next
            Else
                dbNames.Add(My.Settings.DatabaseName.ToString.Trim)
            End If

            WriteToFile("Sql automated backup service started with Configs:" & _
                        vbNewLine & "Connection String: " & My.Settings.ConnectionString & _
                        vbNewLine & "Database Name: " & My.Settings.DatabaseName & _
                        vbNewLine & "Backup Directory: " & My.Settings.BackupDirectory & _
                        vbNewLine & "Backup Schedule: " & My.Settings.AutoBackupTime & _
                        vbNewLine & "Auto Delete Delay: " & My.Settings.AutoDeleteDelay)

            Timer1 = New Timers.Timer
            Timer1.Interval = 60000
            AddHandler Timer1.Elapsed, AddressOf Timer1_Tick
            Timer1.Enabled = True

        Catch ex As Exception
            WriteToFile("Service started with errors\n" & ex.StackTrace)
            Me.EventLog.WriteEntry(ex.Message)
        End Try
    End Sub

    Protected Overrides Sub OnStop()
        WriteToFile("Sql automated backup service stopped")
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs)
        Try
            For Each kvp As KeyValuePair(Of String, Boolean) In backupHourMap.ToList
                Dim Hour As String = kvp.Key
                Dim backupTaken As Boolean = kvp.Value

                If Date.Now.Hour.ToString.Equals(Hour) Then
                    If backupTaken = False Then
                        backupHourMap(Hour) = True
                        For Each dbname As String In dbNames
                            TakeBackup(dbname)
                        Next
                    End If
                Else
                    backupHourMap(Hour) = False
                End If
            Next
        Catch ex As Exception
            WriteToFile(ex.StackTrace)
            Me.EventLog.WriteEntry(ex.Message)
        End Try
    End Sub

    Public Sub RemoveOldBackupFiles()
        If My.Settings.AutoDeleteDelay = -1 Then
            Return
        End If

        For Each file As IO.FileInfo In New IO.DirectoryInfo(My.Settings.BackupDirectory).GetFiles
            If file.Extension.Equals(".BAK") AndAlso (Now - file.CreationTime).Days > My.Settings.AutoDeleteDelay Then file.Delete()
        Next

        For Each file As IO.FileInfo In New IO.DirectoryInfo(getTempDir()).GetFiles
            If file.Extension.Equals(".log") AndAlso (Now - file.CreationTime).Days > My.Settings.AutoDeleteDelay Then file.Delete()
        Next
    End Sub

    Public Sub WriteToFile(Message As String)
        Try
            Dim path As String = getTempDir()

            If Directory.Exists(path) = False Then
                Directory.CreateDirectory(path)
            End If

            Dim filepath As String = getTempDir() & "\Log_" & DateTime.Now.Date.ToShortDateString.Replace("/".ToCharArray.GetValue(0), "_".ToCharArray.GetValue(0)) & ".log"

            Dim log As System.IO.StreamWriter

            If File.Exists(filepath) = False Then
                log = File.CreateText(filepath)
                Me.EventLog.WriteEntry(filepath)
            Else
                log = My.Computer.FileSystem.OpenTextFileWriter(filepath, True)
            End If

            Message = "[" & Date.Now.ToLocalTime & "]" & vbNewLine & Message & vbNewLine
            log.WriteLine(Message)
            log.Close()
        Catch ex As Exception
            Me.EventLog.WriteEntry(ex.Message)
        End Try
    End Sub

    Public Function getTempDir() As String
        Dim s As String = AppDomain.CurrentDomain.BaseDirectory
        If s.EndsWith("\") Then
            Return s.Substring(0, s.Length - 1) & "\ServiceLog"
        End If
        Return s & "\ServiceLog"
    End Function

    Private Sub TakeBackup(DatabaseName As String)
        WriteToFile("Backuping database (" & DatabaseName & ")...")
        Dim sqlHelper As SqlDBHelper.Helper = Nothing

        Try
            If Directory.Exists(My.Settings.BackupDirectory) = False Then
                Directory.CreateDirectory(My.Settings.BackupDirectory)
            End If

            sqlHelper = New SqlDBHelper.Helper
            Dim backupQuery As String = "DECLARE @name VARCHAR(50) " & _
                                   " DECLARE @path VARCHAR(256) " & _
                                   " DECLARE @fileName VARCHAR(256) " & _
                                   " DECLARE @fileDate VARCHAR(20) " & _
                                   " " & _
                                   " SET @path = '" & My.Settings.BackupDirectory & "\' " & _
                                   " " & _
                                   " SELECT @fileDate = CONVERT(VARCHAR(20),GETDATE(),112) " & _
                                   " " & _
                                   " DECLARE db_cursor CURSOR READ_ONLY FOR  " & _
                                   " Select Name" & _
                                   " FROM master.dbo.sysdatabases " & _
                                   " WHERE name IN ('" & DatabaseName & "')" & _
                                   " " & _
                                   " OPEN db_cursor   " & _
                                   " FETCH NEXT FROM db_cursor INTO @name   " & _
                                   " " & _
                                   " WHILE @@FETCH_STATUS = 0   " & _
                                   " BEGIN   " & _
                                   "    SET @fileName = @path + @name + '_' + @fileDate + '_" & Date.Now.Hour & "' +'.BAK'  " & _
                                   "    BACKUP DATABASE @name TO DISK = @fileName  " & _
                                   " " & _
                                   "    FETCH NEXT FROM db_cursor INTO @name   " & _
                                   "                 End" & _
                                   " " & _
                                   " " & _
                                   " CLOSE db_cursor " & _
                                   " DEALLOCATE db_cursor"

            sqlHelper.Execute(backupQuery)
            WriteToFile("Backup taken successfully")

        Catch ex As Exception
            WriteToFile(ex.StackTrace)
        Finally
            If Not sqlHelper Is Nothing Then
                Try
                    sqlHelper.cn.Close()
                Catch ex As Exception

                End Try
            End If
        End Try
    End Sub

End Class
