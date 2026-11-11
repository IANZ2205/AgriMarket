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

                    If parts(0) = "HELLO" Then
                        Dim reply As String = $"OK|{parts(1)}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}|1.0" & vbCrLf
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

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
                            modDB.CreateSession(Integer.Parse(userId), token, "127.0.0.1")
                            modDB.WriteLog("Info", "Login", "User logged in: " & email, Integer.Parse(userId))
                            reply = $"OK|{reqId}|{role}|{name}|{token}|{userId}" & vbCrLf
                        Else
                            reply = $"ERR|{reqId}|BAD_CREDENTIALS|Invalid email or password" & vbCrLf
                        End If
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

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
                        Dim addReply As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(addReply, 0, addReply.Length)

                    ElseIf parts(0) = "ITEM_TOPUP" Then
                        Dim reqId As String = parts(1)
                        Dim itemId As Integer = Integer.Parse(parts(3))
                        Dim qty As Double = Double.Parse(parts(4))
                        Dim shelfLife As Integer = Integer.Parse(parts(5))
                        Dim newPrice As Decimal = Decimal.Parse(parts(6))
                        Dim batchNo As String = modDB.TopUpItem(itemId, qty, shelfLife, newPrice)
                        Dim reply As String
                        If Not String.IsNullOrEmpty(batchNo) Then
                            reply = $"OK|{reqId}|{batchNo}" & vbCrLf
                        Else
                            reply = $"ERR|{reqId}|VALIDATION|Failed to top up item" & vbCrLf
                        End If
                        Dim topupReply As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(topupReply, 0, topupReply.Length)

                    ElseIf parts(0) = "BATCH_LIST" Then
                        Dim reqId As String = parts(1)
                        Dim itemId As Integer = Integer.Parse(parts(3))
                        Dim dt As DataTable = modDB.GetItemBatches(itemId)
                        Dim sbReply As New StringBuilder()
                        sbReply.AppendLine($"OK|{reqId}")
                        For Each row As DataRow In dt.Rows
                            sbReply.AppendLine("BATCH|" &
                                row("BatchID").ToString() & "|" &
                                row("BatchNo").ToString() & "|" &
                                row("DateAdded").ToString() & "|" &
                                row("ExpiryDate").ToString() & "|" &
                                Convert.ToDouble(row("QtyRemaining")).ToString("F2") & "|" &
                                Convert.ToDouble(row("QtyReserved")).ToString("F2") & "|" &
                                row("Status").ToString() & "|" &
                                row("DaysLeft").ToString())
                        Next
                        sbReply.AppendLine("END")
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "ITEM_UPDATE" Then
                        Dim reqId As String = parts(1)
                        Dim itemId As Integer = Integer.Parse(parts(3))
                        Dim name As String = parts(4)
                        Dim desc As String = parts(5)
                        Dim price As Decimal = Decimal.Parse(parts(6))
                        Dim lowStock As Integer = Integer.Parse(parts(7))
                        If modDB.UpdateItem(itemId, name, desc, price, lowStock) Then
                            Dim reply As String = $"OK|{reqId}|Updated" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|VALIDATION|Update failed" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If

                    ElseIf parts(0) = "BATCH_UPDATE" Then
                        Dim reqId As String = parts(1)
                        Dim batchId As Integer = Integer.Parse(parts(3))
                        Dim newQty As Double = Double.Parse(parts(4))
                        Dim newShelf As Integer = Integer.Parse(parts(5))
                        Dim reason As String = parts(6)
                        If modDB.AdjustBatch(batchId, newQty, newShelf, reason) Then
                            Dim reply As String = $"OK|{reqId}|Adjusted" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|VALIDATION|Adjustment failed" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If


                    ElseIf parts(0) = "BROWSE" Then
                        Dim reqId As String = parts(1)
                        Dim category As String = ""
                        If parts.Length > 3 Then category = parts(3)

                        Dim dt As DataTable = modDB.GetBrowseListings(category)
                        Dim sbReply As New StringBuilder()
                        sbReply.AppendLine($"OK|{reqId}")
                        For Each row As DataRow In dt.Rows
                            sbReply.AppendLine("LISTING|" &
                                row("ItemID").ToString() & "|" &
                                row("EnglishName").ToString() & "|" &
                                row("Category").ToString() & "|" &
                                Convert.ToDouble(row("AvailableQty")).ToString("F2") & "|" &
                                row("Unit").ToString() & "|" &
                                Convert.ToDecimal(row("UnitPrice")).ToString("F0") & "|" &
                                row("TraderName").ToString() & "|" &
                                row("DaysLeft").ToString())
                        Next
                        sbReply.AppendLine("END")
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "ITEM_DETAIL" Then
                        Dim reqId As String = parts(1)
                        Dim itemId As Integer = Integer.Parse(parts(3))
                        Dim dt As DataTable = modDB.GetItemDetail(itemId)
                        Dim reply As String
                        If dt.Rows.Count > 0 Then
                            Dim row As DataRow = dt.Rows(0)
                            reply = "OK|" & reqId & "|" &
                                row("ItemID").ToString() & "|" &
                                row("EnglishName").ToString() & "|" &
                                row("Category").ToString() & "|" &
                                row("Description").ToString().Replace(vbCrLf, "\n") & "|" &
                                Convert.ToDouble(row("AvailableQty")).ToString("F2") & "|" &
                                row("Unit").ToString() & "|" &
                                Convert.ToDecimal(row("UnitPrice")).ToString("F0") & "|" &
                                row("TraderName").ToString() & "|" &
                                row("TraderPhone").ToString() & "|" &
                                row("TraderID").ToString() & "|" &
                                row("DaysLeft").ToString() & "|" &
                                row("LocalName").ToString() & vbCrLf
                        Else
                            reply = "ERR|" & reqId & "|NOT_FOUND|Item not found" & vbCrLf
                        End If
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "ORDER_PLACE" Then
                        Dim reqId As String = parts(1)
                        Dim itemId As Integer = Integer.Parse(parts(3))
                        Dim qty As Double = Double.Parse(parts(4))
                        Dim neededBy As String = parts(5)
                        Dim delivery As String = parts(6)
                        Dim note As String = parts(7)
                        Dim buyerId As Integer = Integer.Parse(parts(8)) ' <-- BuyerID is parts(8)

                        ' Fetch item details
                        Dim dt As DataTable = modDB.GetItemDetail(itemId)
                        If dt.Rows.Count = 0 Then
                            Dim replyErr As String = $"ERR|{reqId}|NOT_FOUND|Item not found" & vbCrLf
                            Dim errBytes As Byte() = Encoding.UTF8.GetBytes(replyErr)
                            stream.Write(errBytes, 0, errBytes.Length)
                        Else
                            Dim price As Decimal = Convert.ToDecimal(dt.Rows(0)("UnitPrice"))
                            Dim traderId As Integer = Convert.ToInt32(dt.Rows(0)("TraderID"))
                            Dim available As Double = Convert.ToDouble(dt.Rows(0)("AvailableQty"))

                            If qty > available Then
                                Dim replyErr As String = $"ERR|{reqId}|NOT_ENOUGH_STOCK|Only {available} available" & vbCrLf
                                Dim errBytes As Byte() = Encoding.UTF8.GetBytes(replyErr)
                                stream.Write(errBytes, 0, errBytes.Length)
                            Else
                                ' Use the buyerId from the message
                                Dim orderNo As String = modDB.PlaceOrder(buyerId, itemId, qty, neededBy, delivery, note, price, traderId)
                                Dim reply As String
                                If Not String.IsNullOrEmpty(orderNo) Then
                                    reply = $"OK|{reqId}|{orderNo}" & vbCrLf
                                Else
                                    reply = $"ERR|{reqId}|VALIDATION|Failed to place order" & vbCrLf
                                End If
                                Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                                stream.Write(replyBytes, 0, replyBytes.Length)
                            End If
                        End If

                    ElseIf parts(0) = "ORDER_LIST" Then
                        Dim reqId As String = parts(1)
                        Dim traderId As Integer = Integer.Parse(parts(3))
                        Dim dt As DataTable = modDB.GetTraderOrders(traderId)

                        Dim sbReply As New StringBuilder()
                        sbReply.AppendLine($"OK|{reqId}")
                        For Each row As DataRow In dt.Rows
                            sbReply.AppendLine("ORDER|" &
                                row("OrderID").ToString() & "|" &
                                row("OrderNo").ToString() & "|" &
                                row("BuyerName").ToString() & "|" &
                                row("ItemName").ToString() & "|" &
                                Convert.ToDouble(row("Qty")).ToString("F2") & "|" &
                                Convert.ToDecimal(row("TotalAmount")).ToString("F0") & "|" &
                                row("Status").ToString() & "|" &
                                row("CreatedAt").ToString())
                        Next
                        sbReply.AppendLine("END")
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "STOCK_LIST" Then
                        Dim reqId As String = parts(1)
                        Dim traderId As Integer = Integer.Parse(parts(3))
                        Dim dt As DataTable = modDB.GetTraderStock(traderId)
                        Dim sbReply As New StringBuilder()
                        sbReply.AppendLine($"OK|{reqId}")
                        For Each row As DataRow In dt.Rows
                            sbReply.AppendLine("ITEM|" &
                                row("ItemID").ToString() & "|" &
                                row("EnglishName").ToString() & "|" &
                                row("Category").ToString() & "|" &
                                Convert.ToDouble(row("AvailableQty")).ToString("F2") & "|" &
                                row("Unit").ToString() & "|" &
                                Convert.ToDecimal(row("UnitPrice")).ToString("F0") & "|" &
                                row("BatchCount").ToString())
                        Next
                        sbReply.AppendLine("END")
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "ORDER_CONFIRM" Then
                        Dim reqId As String = parts(1)
                        Dim orderId As Integer = Integer.Parse(parts(3))
                        Dim result As String = modDB.ConfirmOrder(orderId)

                        If result = "OK" Then
                            Dim reply As String = $"OK|{reqId}|Confirmed" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|{result.Split("|"c)(1)}|{result.Split("|"c)(2)}" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If

                    ElseIf parts(0) = "ORDER_REJECT" Then
                        Dim reqId As String = parts(1)
                        Dim orderId As Integer = Integer.Parse(parts(3))
                        Dim reason As String = parts(4)
                        Dim result As String = modDB.RejectOrder(orderId, reason)

                        If result = "OK" Then
                            Dim reply As String = $"OK|{reqId}|Rejected" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|{result.Split("|"c)(1)}|{result.Split("|"c)(2)}" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If

                    ElseIf parts(0) = "ORDER_COMPLETE" Then
                        Dim reqId As String = parts(1)
                        Dim orderId As Integer = Integer.Parse(parts(3))
                        Dim result As String = modDB.CompleteOrder(orderId)

                        If result = "OK" Then
                            Dim reply As String = $"OK|{reqId}|Completed" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|{result.Split("|"c)(1)}|{result.Split("|"c)(2)}" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If

                    ElseIf parts(0) = "NOTIF_LIST" Then
                        Dim reqId As String = parts(1)
                        Dim userId As Integer = Integer.Parse(parts(3))
                        Dim dt As DataTable = modDB.GetNotifications(userId)
                        Dim sbReply As New StringBuilder()
                        sbReply.AppendLine($"OK|{reqId}")
                        For Each row As DataRow In dt.Rows
                            sbReply.AppendLine("NOTIF|" &
                                row("NotifID").ToString() & "|" &
                                row("NotifType").ToString() & "|" &
                                row("Title").ToString() & "|" &
                                row("Body").ToString() & "|" &
                                row("IsRead").ToString() & "|" &
                                row("CreatedAt").ToString())
                        Next
                        sbReply.AppendLine("END")
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "NOTIF_READ" Then
                        Dim reqId As String = parts(1)
                        Dim notifId As Integer = Integer.Parse(parts(3))
                        modDB.MarkNotificationRead(notifId)
                        Dim reply As String = $"OK|{reqId}|Read" & vbCrLf
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "NOTIF_COUNT" Then
                        Dim reqId As String = parts(1)
                        Dim userId As Integer = Integer.Parse(parts(3))
                        Dim count As Integer = modDB.GetUnreadNotificationCount(userId)
                        Dim reply As String = $"OK|{reqId}|{count}" & vbCrLf
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)


                    ElseIf parts(0) = "CHAT_LIST" Then
                        Dim reqId As String = parts(1)
                        Dim userId As Integer = Integer.Parse(parts(3))
                        Dim dt As DataTable = modDB.GetChatList(userId)
                        Dim sbReply As New StringBuilder()
                        sbReply.AppendLine($"OK|{reqId}")
                        For Each row As DataRow In dt.Rows
                            sbReply.AppendLine("CONV|" &
                                row("ConvID").ToString() & "|" &
                                row("OtherUserID").ToString() & "|" &
                                row("OtherUserName").ToString() & "|" &
                                row("OtherUserRole").ToString() & "|" &
                                If(IsDBNull(row("LastMessage")), "", row("LastMessage").ToString()) & "|" &
                                row("LastMessageAt").ToString() & "|" &
                                row("UnreadCount").ToString())
                        Next
                        sbReply.AppendLine("END")
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "CHAT_OPEN" Then
                        Dim reqId As String = parts(1)
                        Dim userId As Integer = Integer.Parse(parts(3))
                        Dim otherId As Integer = Integer.Parse(parts(4))
                        Dim convId As Integer = modDB.GetOrCreateConversation(userId, otherId)
                        If convId > 0 Then
                            Dim dt As DataTable = modDB.GetMessages(convId)
                            Dim sbReply As New StringBuilder()
                            sbReply.AppendLine($"OK|{reqId}|{convId}")
                            For Each row As DataRow In dt.Rows
                                sbReply.AppendLine("MSG|" &
                                    row("MessageID").ToString() & "|" &
                                    row("SenderID").ToString() & "|" &
                                    row("MsgType").ToString() & "|" &
                                    If(IsDBNull(row("Body")), "", row("Body").ToString()) & "|" &
                                    row("SentAt").ToString() & "|" &
                                    If(IsDBNull(row("ReadAt")), "", row("ReadAt").ToString()))
                            Next
                            sbReply.AppendLine("END")
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|VALIDATION|Could not open chat" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If

                    ElseIf parts(0) = "CHAT_SEND" Then
                        Dim reqId As String = parts(1)
                        Dim convId As Integer = Integer.Parse(parts(3))
                        Dim senderId As Integer = Integer.Parse(parts(4))
                        Dim body As String = parts(5)
                        Dim msgId As Integer = modDB.SendMessage(convId, senderId, body)
                        Dim reply As String
                        If msgId > 0 Then
                            reply = $"OK|{reqId}|{msgId}" & vbCrLf
                        Else
                            reply = $"ERR|{reqId}|VALIDATION|Message failed" & vbCrLf
                        End If
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "CHAT_READ" Then
                        Dim reqId As String = parts(1)
                        Dim convId As Integer = Integer.Parse(parts(3))
                        Dim userId As Integer = Integer.Parse(parts(4))
                        modDB.MarkMessagesRead(convId, userId)
                        Dim reply As String = $"OK|{reqId}|Read" & vbCrLf
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)


                    ElseIf parts(0) = "BUYER_ORDERS" Then
                        Dim reqId As String = parts(1)
                        Dim buyerId As Integer = Integer.Parse(parts(3))
                        Dim dt As DataTable = modDB.GetBuyerOrders(buyerId)
                        Dim sbReply As New StringBuilder()
                        sbReply.AppendLine($"OK|{reqId}")
                        For Each row As DataRow In dt.Rows
                            sbReply.AppendLine("BORDER|" &
                                row("OrderID").ToString() & "|" &
                                row("OrderNo").ToString() & "|" &
                                row("ItemName").ToString() & "|" &
                                Convert.ToDouble(row("Qty")).ToString("F2") & "|" &
                                row("ItemUnit").ToString() & "|" &
                                Convert.ToDecimal(row("TotalAmount")).ToString("F0") & "|" &
                                row("Status").ToString() & "|" &
                                row("TraderName").ToString() & "|" &
                                row("CreatedAt").ToString() & "|" &
                                If(IsDBNull(row("ReasonComment")), "", row("ReasonComment").ToString()))
                        Next
                        sbReply.AppendLine("END")
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "ORDER_CANCEL" Then
                        Dim reqId As String = parts(1)
                        Dim orderId As Integer = Integer.Parse(parts(3))
                        Dim result As String = modDB.CancelOrder(orderId, "Buyer")
                        If result = "OK" Then
                            Dim reply As String = $"OK|{reqId}|Cancelled" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|{result.Split("|"c)(1)}|{result.Split("|"c)(2)}" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If

                    ElseIf parts(0) = "FILE_UPLOAD" Then
                        Dim reqId As String = parts(1)
                        Dim base64Data As String = parts(2)
                        Dim originalName As String = parts(3)

                        Dim cleanName As String = System.IO.Path.GetFileName(originalName)
                        Dim uniqueName As String = Guid.NewGuid().ToString() & "_" & cleanName
                        Dim savePath As String = System.IO.Path.Combine(Application.StartupPath, "Files", "profiles", uniqueName)

                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(savePath))

                        Try
                            Dim fileBytes As Byte() = Convert.FromBase64String(base64Data)
                            System.IO.File.WriteAllBytes(savePath, fileBytes)

                            Dim reply As String = $"OK|{reqId}|{uniqueName}" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Catch ex As Exception
                            Dim reply As String = $"ERR|{reqId}|UPLOAD_FAILED|{ex.Message}" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End Try

                    ElseIf parts(0) = "UPDATE_PROFILE" Then
                        Dim reqId As String = parts(1)
                        Dim userId As Integer = Integer.Parse(parts(2))
                        Dim phone As String = parts(3)
                        Dim location As String = parts(4)
                        Dim picName As String = parts(5)

                        If modDB.UpdateUserProfile(userId, phone, location, picName) Then
                            Dim reply As String = $"OK|{reqId}|Updated" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|VALIDATION|Update failed" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If

                    ElseIf parts(0) = "GET_PROFILE" Then
                        Dim reqId As String = parts(1)
                        Dim userId As Integer = Integer.Parse(parts(3))
                        Dim dt As DataTable = modDB.GetUserProfile(userId)
                        Dim reply As String
                        If dt.Rows.Count > 0 Then
                            Dim row As DataRow = dt.Rows(0)
                            Dim pic As String = If(IsDBNull(row("ProfilePic")), "", row("ProfilePic").ToString())

                            ' Ensure we always send 7 parts: OK|ReqID|Name|Role|Phone|Location|Pic
                            reply = $"OK|{reqId}|{row("DisplayName")}|{row("Role")}|{row("Phone")}|{row("LocationDetail")}|{pic}" & vbCrLf
                        Else
                            reply = $"ERR|{reqId}|NOT_FOUND|User not found" & vbCrLf
                        End If
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "FILE_GET" Then
                        Dim reqId As String = parts(1)
                        Dim fileName As String = parts(3)
                        Dim filePath As String = System.IO.Path.Combine(Application.StartupPath, "Files", "profiles", fileName)
                        If System.IO.File.Exists(filePath) Then
                            Dim fileBytes As Byte() = System.IO.File.ReadAllBytes(filePath)
                            Dim base64 As String = Convert.ToBase64String(fileBytes)
                            Dim reply As String = $"OK|{reqId}|{base64}" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        Else
                            Dim reply As String = $"ERR|{reqId}|NOT_FOUND|File not found" & vbCrLf
                            Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(reply)
                            stream.Write(replyBytes, 0, replyBytes.Length)
                        End If

                    ElseIf parts(0) = "TRADER_HOME" Then
                        Dim reqId As String = parts(1)
                        Dim traderId As Integer = Integer.Parse(parts(3))

                        Dim stats As DataTable = modDB.GetTraderHomeStats(traderId)
                        Dim batches As DataTable = modDB.GetBatchesToWatch(traderId)

                        Dim sbReply As New StringBuilder()
                        sbReply.AppendLine($"OK|{reqId}")

                        ' Stats record
                        If stats.Rows.Count > 0 Then
                            Dim row As DataRow = stats.Rows(0)
                            sbReply.AppendLine("STATS|" & row("ItemsInStock").ToString() & "|" & row("PendingOrders").ToString() & "|" & row("CompletedOrders").ToString() & "|" & Convert.ToDecimal(row("SalesThisMonth")).ToString("F0"))
                        End If

                        ' Batches record
                        For Each row As DataRow In batches.Rows
                            sbReply.AppendLine("WATCH|" & row("EnglishName").ToString() & "|" & row("BatchNo").ToString() & "|" & row("DaysLeft").ToString())
                        Next

                        sbReply.AppendLine("END")
                        Dim replyBytes As Byte() = Encoding.UTF8.GetBytes(sbReply.ToString())
                        stream.Write(replyBytes, 0, replyBytes.Length)

                    ElseIf parts(0) = "LOGOUT" Then
                        Dim token As String = parts(1)
                        modDB.CloseSession(token, "Logout")
                        modDB.WriteLog("Info", "Logout", "User logged out")
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