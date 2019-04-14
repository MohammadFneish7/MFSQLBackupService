Imports System.IO
Imports System.Collections.Specialized
Imports System.Data.SqlClient

Public Class MFSQLBackupService

    Dim Timer1 As Timers.Timer
    Dim backupHourMap As New Dictionary(Of String, Boolean)

    Protected Overrides Sub OnStart(ByVal args() As String)
        Try
            For Each Hour As String In My.Settings.AutoBackupTime
                If Hour.Trim.Length > 0 Then
                    backupHourMap.Add(Hour.Trim, False)
                End If
            Next

            WriteToFile("Sql automated backup service started with Configs:" & _
                        vbNewLine & "Connection String: " & printStringCollection(My.Settings.ConnectionString, vbNewLine & vbTab) & _
                        vbNewLine & "Backup Directory: " & My.Settings.BackupDirectory & _
                        vbNewLine & "Backup Schedule: " & printStringCollection(My.Settings.AutoBackupTime, ",") & _
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

    Private Function printStringCollection(ByVal stringcollection As StringCollection, ByVal splitter As String) As String
        Dim s As String = String.Empty
        For Each item As String In stringcollection
            If s.Equals(String.Empty) Then
                s = item
            Else
                s = s & splitter & item
            End If
        Next
        Return s.Trim
    End Function

    Private Sub Timer1_Tick(sender As Object, e As EventArgs)
        Try
            For Each kvp As KeyValuePair(Of String, Boolean) In backupHourMap.ToList
                Dim Hour As String = kvp.Key
                Dim backupTaken As Boolean = kvp.Value

                If Date.Now.Hour.ToString.Equals(Hour) Then
                    If backupTaken = False Then
                        backupHourMap(Hour) = True
                        TakeBackup()
                    End If
                Else
                    backupHourMap(Hour) = False
                End If
            Next
        Catch ex As Exception
            WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
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

    Private Sub TakeBackup()
        WriteToFile("Taking database(s) Backup...")
        Dim sqlHelper As SqlDBHelper.Helper = Nothing


        If Directory.Exists(My.Settings.BackupDirectory) = False Then
            Directory.CreateDirectory(My.Settings.BackupDirectory)
        End If
        For Each constr As String In My.Settings.ConnectionString
            Try
                Dim builder As New SqlConnectionStringBuilder(constr)
                WriteToFile("Backuping database '" & builder.InitialCatalog & "'...")

                sqlHelper = New SqlDBHelper.Helper(constr, Me)

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
                                       " WHERE name IN ('" & builder.InitialCatalog & "')" & _
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
                WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                Me.EventLog.WriteEntry(ex.Message)
            Finally
                If Not sqlHelper Is Nothing Then
                    Try
                        sqlHelper.cn.Close()
                    Catch ex As Exception

                    End Try
                End If
            End Try
        Next
    End Sub

End Class
