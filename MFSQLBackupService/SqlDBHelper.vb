Imports System.Text
Imports System.Security.Cryptography

Namespace SqlDBHelper

    Public Class Helper

        Public cn As New SqlClient.SqlConnection
        Public ds As New DataSet
        Dim daSQL As SqlClient.SqlDataAdapter
        Dim constring As String
        Dim MFSQLBackupService As MFSQLBackupService

        Public Sub New(conStr As String, MFSQLBackupService As MFSQLBackupService)
            Try
                Me.MFSQLBackupService = MFSQLBackupService
                constring = conStr.Trim
                SetMsSqlServerConnection()
            Catch ex As Exception
                MFSQLBackupService.WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                MFSQLBackupService.EventLog.WriteEntry(ex.Message)
            End Try
        End Sub

        Public Sub SetMsSqlServerConnection()
            cn = New SqlClient.SqlConnection
            cn.ConnectionString = constring
            cn.Open()
        End Sub

        Public Function Execute(ByVal strSQL As String) As Integer
            Dim cmd As SqlClient.SqlCommand
            Dim id As Integer
            Try

                SetMsSqlServerConnection()
                If cn.State = ConnectionState.Closed Then
                    cn = New SqlClient.SqlConnection
                    cn.ConnectionString = constring
                    cn.Open()
                End If
                cmd = New SqlClient.SqlCommand(strSQL, cn)
                cmd.ExecuteNonQuery()
                Try
                    id = New SqlClient.SqlCommand("SELECT @@IDENTITY;", cn).ExecuteScalar()
                Catch ex As Exception

                End Try

                cmd.Dispose()
                cn.Close()
            Catch ex As Exception
                MFSQLBackupService.WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                MFSQLBackupService.EventLog.WriteEntry(ex.Message)
            End Try
            Return id
        End Function

        Public Function ExecuteTEST(ByVal strSQL As String, count As Int32) As Integer
            Dim cmd As SqlClient.SqlCommand
            Dim id As Integer
            Try

                SetMsSqlServerConnection()
                If cn.State = ConnectionState.Closed Then
                    cn = New SqlClient.SqlConnection
                    cn.ConnectionString = constring
                    cn.Open()
                End If
                cmd = New SqlClient.SqlCommand(strSQL, cn)
                cmd.ExecuteNonQuery()
                Try
                    id = New SqlClient.SqlCommand("SELECT @@IDENTITY;", cn).ExecuteScalar()
                Catch ex As Exception

                End Try

                cmd.Dispose()
                cn.Close()

            Catch ex As Exception
                MFSQLBackupService.WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                MFSQLBackupService.EventLog.WriteEntry(ex.Message)
            End Try
            Return id
        End Function

        Public Sub ExecuteNoReturn(ByVal strSQL As String)
            Dim cmd As SqlClient.SqlCommand
            Try

                SetMsSqlServerConnection()
                If cn.State = ConnectionState.Closed Then
                    cn = New SqlClient.SqlConnection
                    cn.ConnectionString = constring
                    cn.Open()
                End If
                cmd = New SqlClient.SqlCommand(strSQL, cn)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
                cn.Close()
            Catch ex As Exception
                MFSQLBackupService.WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                MFSQLBackupService.EventLog.WriteEntry(ex.Message)
            End Try
        End Sub

        Public Sub ExecuteNoReturn(ByVal cmd As SqlClient.SqlCommand)
            Try

                SetMsSqlServerConnection()
                If cn.State = ConnectionState.Closed Then
                    cn = New SqlClient.SqlConnection
                    cn.ConnectionString = constring
                    cn.Open()
                End If
                cmd.Connection = cn
                cmd.ExecuteNonQuery()
                cmd.Dispose()
                cn.Close()
            Catch ex As Exception
                MFSQLBackupService.WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                MFSQLBackupService.EventLog.WriteEntry(ex.Message)
            End Try
        End Sub

        Public Function ExecuteScalar(ByVal strSQL As String) As Integer
            Dim cmd As SqlClient.SqlCommand
            Dim val As Integer
            Try
                SetMsSqlServerConnection()
                If cn.State = ConnectionState.Closed Then
                    cn = New SqlClient.SqlConnection
                    cn.ConnectionString = constring
                    cn.Open()
                End If
                cmd = New SqlClient.SqlCommand(strSQL, cn)
                cmd.ExecuteNonQuery()
                val = cmd.ExecuteScalar()
                cmd.Dispose()
                cn.Close()
            Catch ex As Exception
                MFSQLBackupService.WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                MFSQLBackupService.EventLog.WriteEntry(ex.Message)
            End Try
            Return val
        End Function

        Public Sub GetData(ByVal strSQL As String, Optional ByVal strDataTableName As String = "dt")
            Try

                SetMsSqlServerConnection()
                If cn.State = ConnectionState.Closed Then
                    cn = New SqlClient.SqlConnection
                    cn.ConnectionString = constring
                    Try
                        cn.Open()
                    Catch ex As Exception

                    End Try
                End If

                Dim da As New SqlClient.SqlDataAdapter

                da.SelectCommand = New SqlClient.SqlCommand(strSQL, cn)
                da.Fill(ds, strDataTableName)

                cn.Close()

            Catch ex As Exception
                MFSQLBackupService.WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                MFSQLBackupService.EventLog.WriteEntry(ex.Message)
            End Try
        End Sub

        Public Sub GetData(ByVal strSQL As String, ByVal strDataTab As DataTable)
            Try

                SetMsSqlServerConnection()
                If cn.State = ConnectionState.Closed Then
                    cn = New SqlClient.SqlConnection
                    cn.ConnectionString = constring
                    Try
                        cn.Open()
                    Catch ex As Exception

                    End Try
                End If

                Dim da As New SqlClient.SqlDataAdapter

                da.SelectCommand = New SqlClient.SqlCommand(strSQL, cn)
                da.Fill(strDataTab)

                cn.Close()

            Catch ex As Exception
                MFSQLBackupService.WriteToFile("Exception: " & ex.Message & "\n" & ex.StackTrace)
                MFSQLBackupService.EventLog.WriteEntry(ex.Message)
            End Try
        End Sub


        Public Function isDouble(ByVal s As String) As Boolean
            Try
                Double.Parse(s)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

#Region "Email"
        Public Sub SendEmail()
            Dim client As New System.Net.Mail.SmtpClient
            Dim message As New System.Net.Mail.MailMessage
            client.Credentials = New System.Net.NetworkCredential("VOTRE_EMAIL_ICI", "VOTRE_MOT_DE_PASSE_ICI")

            Try

                client.Port = 25 'définition du port 
                client.Host = "smtp.live.com" 'définition du serveur smtp
                client.EnableSsl = True
                message.From = New System.Net.Mail.MailAddress("ADRESSE_DE_LEMETTEUR_ICI")
                message.To.Add("ADRESSE_DU_DESTINATAIRE_ICI")

                Dim item As New System.Net.Mail.Attachment("LIEN_DE_LA_PIECE_JOINTE_EVENTUELLE_ICI")
                message.Attachments.Add(item) 'ajout de la pièce jointe au message

                message.Subject = "SUJET_DU_MESSAGE_ICI"
                message.Body = "CONTENU_DU_MESSAGE_ICI"

                client.Send(message) 'envoi du mail
            Catch ex As Exception
                'TODO traiter les erreurs
            End Try
        End Sub
#End Region

#Region "Licence"
        Public Function GetMotherBoardID() As String

            Dim wmi As Object = GetObject("WinMgmts:")
            Dim serial_numbers As String = ""
            Dim mother_boards As Object = wmi.InstancesOf("Win32_BaseBoard")

            For Each board As Object In mother_boards
                serial_numbers &= ", " & board.SerialNumber
            Next board

            If serial_numbers.Length > 0 Then
                serial_numbers = serial_numbers.Substring(2)
            End If

            Return serial_numbers

        End Function

        Public Function GetCpuID() As String

            Dim computer As String = "."
            Dim cpu_ids As String = ""

            Dim wmi As Object = GetObject("winmgmts:" & _
                "{impersonationLevel=impersonate}!\\" & _
                computer & "\root\cimv2")
            Dim processors As Object = wmi.ExecQuery("Select * from Win32_Processor")


            For Each cpu As Object In processors
                cpu_ids = cpu_ids & ", " & cpu.ProcessorId
            Next cpu

            If cpu_ids.Length > 0 Then
                cpu_ids = cpu_ids.Substring(2)
            End If

            Return cpu_ids

        End Function

        Public Function GetHDDId() As String
            Dim fileSystemObject As Object
            Dim drive As Object
            Dim driveName As String

            fileSystemObject = CreateObject("Scripting.FileSystemObject")
            driveName = "C:\"
            drive = fileSystemObject.GetDrive(fileSystemObject.GetDriveName(fileSystemObject.GetAbsolutePathName(driveName)))

            Return (drive.SerialNumber.ToString())
        End Function


        Public Function GenerateHash(ByVal SourceText As String) As String
            'Create an encoding object to ensure the encoding standard for the source text
            Dim Ue As New UnicodeEncoding()
            'Retrieve a byte array based on the source text
            Dim ByteSourceText() As Byte = Ue.GetBytes(SourceText)
            'Instantiate an MD5 Provider object
            Dim Md5 As New MD5CryptoServiceProvider()
            'Compute the hash value from the source
            Dim ByteHash() As Byte = Md5.ComputeHash(ByteSourceText)
            'And convert it to String format for return
            Dim sb As New StringBuilder(ByteHash.Length * 2)
            For Each b As Byte In ByteHash
                sb.Append(Conversion.Hex(b))
            Next
            Return sb.ToString
        End Function


        Public Sub writeLicense(lcs As String)
            System.IO.File.WriteAllText(".\eems.lcs", lcs)
        End Sub

        Public Function verifyLicense(lcs As String) As Boolean
            If System.IO.File.Exists(".\eems.lcs") = False Then
                Return False
            End If

            Dim strSerial As String = System.IO.File.ReadAllText(".\eems.lcs")
            If lcs = strSerial Then
                Return True
            Else
                Return False
            End If
        End Function



#End Region
    End Class
End Namespace

