Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucChats
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UserID As Integer

    Private lvConversations As New ListView()
    Private pnlMessages As New Panel()
    Private txtMessage As New TextBox()
    Private btnSend As New Button()
    Private lblChatHeader As New Label()
    Private currentConvID As Integer = 0
    Private currentOtherID As Integer = 0
    Private currentOtherName As String = ""

    Private Sub ucChats_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        LoadChatList()
    End Sub

    Private Sub BuildLayout()
        ' Title
        Dim lblTitle As New Label()
        lblTitle.Text = "Chats"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        ' Left panel: Conversation list
        Dim pnlLeft As New Panel()
        pnlLeft.Location = New Point(0, 50)
        pnlLeft.Size = New Size(300, Me.Height - 60)
        pnlLeft.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        pnlLeft.BackColor = modTheme.cWhite
        pnlLeft.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(pnlLeft)

        lvConversations.View = View.Details
        lvConversations.FullRowSelect = True
        lvConversations.GridLines = False
        lvConversations.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        lvConversations.Dock = DockStyle.Fill
        lvConversations.BackColor = modTheme.cWhite
        lvConversations.BorderStyle = BorderStyle.None
        lvConversations.Columns.Add("Name", 180)
        lvConversations.Columns.Add("Unread", 60)
        AddHandler lvConversations.SelectedIndexChanged, AddressOf ConversationSelected
        pnlLeft.Controls.Add(lvConversations)

        ' Right panel: Message thread
        Dim pnlRight As New Panel()
        pnlRight.Location = New Point(310, 50)
        pnlRight.Size = New Size(Me.Width - 320, Me.Height - 60)
        pnlRight.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlRight.BackColor = modTheme.cWhite
        pnlRight.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(pnlRight)

        lblChatHeader.Text = "Select a conversation"
        lblChatHeader.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblChatHeader.ForeColor = modTheme.cOcean
        lblChatHeader.Location = New Point(15, 15)
        lblChatHeader.AutoSize = True
        pnlRight.Controls.Add(lblChatHeader)

        pnlMessages.Location = New Point(0, 50)
        pnlMessages.Size = New Size(pnlRight.Width, pnlRight.Height - 100)
        pnlMessages.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlMessages.AutoScroll = True
        pnlMessages.BackColor = modTheme.cSurface
        pnlRight.Controls.Add(pnlMessages)

        txtMessage.Location = New Point(15, pnlRight.Height - 45)
        txtMessage.Size = New Size(pnlRight.Width - 130, 30)
        txtMessage.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtMessage.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        AddHandler txtMessage.KeyDown, AddressOf MessageKeyDown
        pnlRight.Controls.Add(txtMessage)

        btnSend.Text = "Send"
        btnSend.BackColor = modTheme.cOcean
        btnSend.ForeColor = modTheme.cWhite
        btnSend.FlatStyle = FlatStyle.Flat
        btnSend.FlatAppearance.BorderSize = 0
        btnSend.Location = New Point(pnlRight.Width - 110, pnlRight.Height - 46)
        btnSend.Size = New Size(100, 32)
        btnSend.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnSend.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnSend.Click, AddressOf SendClicked
        pnlRight.Controls.Add(btnSend)
    End Sub

    Private Async Sub LoadChatList()
        If String.IsNullOrEmpty(Token) Then Return
        Try
            Dim msg As String = $"CHAT_LIST|14|{Token}|{UserID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(8192) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            lvConversations.Items.Clear()

            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 8 AndAlso p(0) = "CONV" Then
                    Dim lvi As New ListViewItem(p(3)) ' OtherUserName
                    Dim unread As Integer = Integer.Parse(p(7))
                    If unread > 0 Then
                        lvi.SubItems.Add(unread.ToString())
                        lvi.Font = New Font("Segoe UI", 9, FontStyle.Bold)
                        lvi.ForeColor = modTheme.cOcean
                    Else
                        lvi.SubItems.Add("")
                    End If
                    lvi.Tag = p(1) ' ConvID
                    ' Store other ID in a hidden way (we'll use the subitem tag)
                    lvi.SubItems(0).Tag = p(2) ' OtherUserID
                    lvConversations.Items.Add(lvi)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Error loading chats: " & ex.Message)
        End Try
    End Sub

    Private Async Sub ConversationSelected(sender As Object, e As EventArgs)
        If lvConversations.SelectedItems.Count = 0 Then Return
        Dim item As ListViewItem = lvConversations.SelectedItems(0)
        currentConvID = Integer.Parse(item.Tag.ToString())
        currentOtherID = Integer.Parse(item.SubItems(0).Tag.ToString())
        currentOtherName = item.Text
        lblChatHeader.Text = currentOtherName

        ' Clear and load messages
        pnlMessages.Controls.Clear()

        Try
            Dim msg As String = $"CHAT_OPEN|15|{Token}|{UserID}|{currentOtherID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(16384) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            Dim yPos As Integer = 5
            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 7 AndAlso p(0) = "MSG" Then
                    Dim senderId As Integer = Integer.Parse(p(2))
                    Dim body As String = p(4)
                    Dim sentAt As String = p(5)
                    Dim isMine As Boolean = (senderId = UserID)

                    Dim bubble As New Label()
                    bubble.Text = body & vbCrLf & sentAt
                    bubble.Font = New Font("Segoe UI", 9, FontStyle.Regular)
                    bubble.AutoSize = True
                    bubble.MaximumSize = New Size(pnlMessages.Width - 100, 0)
                    bubble.Padding = New Padding(10, 8, 10, 8)

                    If isMine Then
                        bubble.BackColor = modTheme.cOcean
                        bubble.ForeColor = modTheme.cWhite
                    Else
                        bubble.BackColor = modTheme.cWhite
                        bubble.ForeColor = modTheme.cText
                    End If
                    bubble.Location = New Point(If(isMine, pnlMessages.Width - bubble.Width - 15, 15), yPos)
                    pnlMessages.Controls.Add(bubble)
                    yPos += bubble.Height + 10 ' <-- THIS is the critical line that was missing
                End If
            Next

            ' Mark as read
            Dim readMsg As String = $"CHAT_READ|16|{Token}|{currentConvID}|{UserID}" & vbCrLf
            Dim readData As Byte() = Encoding.UTF8.GetBytes(readMsg)
            Await frmConnect.Stream.WriteAsync(readData, 0, readData.Length)
        Catch ex As Exception
            MessageBox.Show("Error loading messages: " & ex.Message)
        End Try
    End Sub

    Private Async Sub SendClicked(sender As Object, e As EventArgs)
        If currentConvID = 0 OrElse String.IsNullOrEmpty(txtMessage.Text.Trim()) Then Return

        Try
            Dim body As String = txtMessage.Text.Trim().Replace("|", "\|")
            txtMessage.Text = ""

            Dim msg As String = $"CHAT_SEND|17|{Token}|{currentConvID}|{UserID}|{body}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                ' Reload the conversation
                ConversationSelected(Nothing, Nothing)
            Else
                MessageBox.Show("Error sending: " & parts(3))
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub

    Private Sub MessageKeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter AndAlso Not e.Shift Then
            e.SuppressKeyPress = True
            SendClicked(Nothing, Nothing)
        End If
    End Sub
End Class
