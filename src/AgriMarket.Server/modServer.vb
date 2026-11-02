Imports System.Net
Imports System.Net.Sockets
Imports System.Threading.Tasks
Imports System.Text

Public Module modServer
    Private Listener As TcpListener
    Public IsListening As Boolean = False

    Public Sub StartListening()
        If IsListening Then Return
        Try
            Listener = New TcpListener(IPAddress.Any, 5000)
            Listener.Start()
            IsListening = True
            Task.Run(AddressOf AcceptClients)
        Catch ex As Exception
            MessageBox.Show("Could not start server: " & ex.Message)
        End Try
    End Sub

    Public Sub StopListening()
        If Not IsListening Then Return
        Listener.Stop()
        IsListening = False
    End Sub

    Private Async Sub AcceptClients()
        While IsListening
            Try
                Dim client As TcpClient = Await Listener.AcceptTcpClientAsync()
                ' Handle each client in the background so it doesn't block others
                Dim t = Task.Run(Sub() HandleClient(client))
            Catch
                ' Ignore shutdown errors
            End Try
        End While
    End Sub

    Private Sub HandleClient(client As TcpClient)
        Try
            Dim stream As NetworkStream = client.GetStream()
            Dim buffer(1024) As Byte
            Dim sb As New StringBuilder()

            While client.Connected
                Dim bytesRead As Integer = stream.Read(buffer, 0, buffer.Length)
                If bytesRead = 0 Then Exit While

                Dim chunk As String = Encoding.UTF8.GetString(buffer, 0, bytesRead)
                sb.Append(chunk)

                While sb.ToString().Contains(vbCrLf)
                    Dim idx As Integer = sb.ToString().IndexOf(vbCrLf)
                    Dim completeMessage As String = sb.ToString().Substring(0, idx)
                    sb.Remove(0, idx + 2)

                    System.Diagnostics.Debug.WriteLine("Server Received: " & completeMessage)
                    Dim parts() As String = completeMessage.Split("|"c)

                    ' --- HELLO ---
                    If parts(0) = "HELLO" Then
                        Dim reqId As String = parts(1)
                        Dim reply As String = $"OK|{reqId}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}|1.0" & vbCrLf
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

                        ' --- LOGIN ---
                    ElseIf parts(0) = "LOGIN" Then
                        Dim reqId As String = parts(1)
                        Dim email As String = parts(3)
                        Dim password As String = parts(4)

                        Dim userInfo As DataTable = modDB.GetUserDetails(email, password)
                        Dim reply As String

                        If userInfo.Rows.Count > 0 Then
                            Dim userId As String = userInfo.Rows(0)("UserID").ToString()
                            Dim role As String = userInfo.Rows(0)("Role").ToString()
                            Dim name As String = userInfo.Rows(0)("DisplayName").ToString()
                            Dim token As String = Guid.NewGuid().ToString()

                            ' OK|ReqID|Role|DisplayName|Token|UserID
                            reply = $"OK|{reqId}|{role}|{name}|{token}|{userId}" & vbCrLf
                        Else
                            reply = $"ERR|{reqId}|BAD_CREDENTIALS|Invalid email or password" & vbCrLf
                        End If
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

                        '                     ' --- ITEM_ADD ---
                    ElseIf parts(0) = "ITEM_ADD" Then
                        Dim reqId As String = parts(1)
                        ' Token = parts(2)
                        Dim traderId As Integer = Integer.Parse(parts(3))
                        MessageBox.Show("DEBUG: Server received STOCK_LIST for TraderID = " & traderId.ToString())
                        Dim category As String = parts(4)
                        Dim itemCode As String = parts(5)
                        Dim name As String = parts(6)
                        Dim desc As String = parts(7)
                        Dim price As Decimal = Decimal.Parse(parts(8))
                        Dim unit As String = parts(9)
                        Dim lowStock As Integer = Integer.Parse(parts(10))
                        Dim qty As Double = Double.Parse(parts(11))
                        Dim shelfLife As Integer = Integer.Parse(parts(12))
                        Dim localNames As String = parts(13)

                        Dim batchNo As String = modDB.AddItem(traderId, category, itemCode, name, desc, price, unit, lowStock, qty, shelfLife, localNames)

                        Dim reply As String
                        If Not String.IsNullOrEmpty(batchNo) Then
                            reply = $"OK|{reqId}|{batchNo}" & vbCrLf
                        Else
                            reply = $"ERR|{reqId}|VALIDATION|Failed to save item" & vbCrLf
                        End If

                        Dim addReplyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(addReplyBytes, 0, addReplyBytes.Length)

                        ' --- STOCK_LIST ---
                    ElseIf parts(0) = "STOCK_LIST" Then
                        Dim reqId As String = parts(1)
                        Dim traderId As Integer = Integer.Parse(parts(3))
                        System.Diagnostics.Debug.WriteLine("DEBUG - TraderID received: " & traderId.ToString())

                        Dim dt As DataTable = modDB.GetTraderStock(traderId)

                        ' Format for multiple records: 
                        ' Field delimiter = Chr(31), Record delimiter = Chr(30)
                        Dim sbReply As New StringBuilder()
                        sbReply.Append($"OK|{reqId}|")

                        For Each row As DataRow In dt.Rows
                            sbReply.Append(Chr(30)) ' Start a new record
                            sbReply.Append(row("ItemID").ToString() & Chr(31))
                            sbReply.Append(row("EnglishName").ToString() & Chr(31))
                            sbReply.Append(row("AvailableQty").ToString() & Chr(31))
                            sbReply.Append(row("Unit").ToString() & Chr(31))
                            sbReply.Append(row("UnitPrice").ToString() & Chr(31))
                            sbReply.Append(row("BatchCount").ToString())
                        Next

                        Dim reply As String = sbReply.ToString() & vbCrLf
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)
                    End If
                End While
            End While
        Catch ex As Exception
            ' Handle disconnects
        Finally
            client.Close()
        End Try
    End Sub
End Module
