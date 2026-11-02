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
                Dim t = Task.Run(Sub() HandleClient(client))
            Catch
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

                            reply = $"OK|{reqId}|{role}|{name}|{token}|{userId}" & vbCrLf
                        Else
                            reply = $"ERR|{reqId}|BAD_CREDENTIALS|Invalid email or password" & vbCrLf
                        End If
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

                        ' --- REGISTER ---
                    ElseIf parts(0) = "REGISTER" Then
                        Dim reqId As String = parts(1)
                        Dim role As String = parts(3)
                        Dim displayName As String = parts(4)
                        Dim email As String = parts(5)
                        Dim phone As String = parts(6)
                        Dim district As String = parts(7)
                        Dim password As String = parts(8)
                        Dim firstName As String = parts(9)
                        Dim lastName As String = parts(10)

                        Dim newUserId As Integer = modDB.RegisterUser(role, displayName, firstName, lastName, email, phone, district, password)
                        Dim reply As String

                        If newUserId > 0 Then
                            reply = $"OK|{reqId}|{newUserId}" & vbCrLf
                        ElseIf newUserId = -1 Then
                            reply = $"ERR|{reqId}|DUPLICATE_EMAIL|Email already registered" & vbCrLf
                        Else
                            reply = $"ERR|{reqId}|VALIDATION|Registration failed" & vbCrLf
                        End If
                        Dim regReply As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(regReply, 0, regReply.Length)

                        ' --- ITEM_ADD ---
                    ElseIf parts(0) = "ITEM_ADD" Then
                        Dim reqId As String = parts(1)
                        Dim traderId As Integer = Integer.Parse(parts(3))
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

                        Dim dt As DataTable = modDB.GetTraderStock(traderId)

                        Dim rowsList As New List(Of String)
                        For Each row As DataRow In dt.Rows
                            Dim rowStr As String = String.Join("|",
                                row("ItemID").ToString(),
                                row("EnglishName").ToString(),
                                Convert.ToDouble(row("AvailableQty")).ToString("F2"),
                                row("Unit").ToString(),
                                Convert.ToDecimal(row("UnitPrice")).ToString("F0"),
                                row("BatchCount").ToString()
                            )
                            rowsList.Add(rowStr)
                        Next

                        Dim reply As String = "OK|" & reqId & "|" & String.Join("~", rowsList) & vbCrLf
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)
                    End If
                End While
            End While
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("HandleClient error: " & ex.Message)
        Finally
            client.Close()
        End Try
    End Sub
End Module