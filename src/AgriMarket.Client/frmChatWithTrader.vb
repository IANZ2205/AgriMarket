Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmChatWithTrader
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UserID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderName As String

    Private pnlMessages As New Panel()
    Private txtMessage As New TextBox()
    Private btnSend As New Button()
    Private currentConvID As Integer = 0

    Private Async Sub frmChatWithTrader_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Chat with " & TraderName
        Me.ClientSize = New Size(600, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = modTheme.cSurface
        Me.Font = modTheme.fBody

        pnlMessages.Location = New Point(10, 10)
        pnlMessages.Size = New Size(Me.ClientSize.Width - 20, Me.ClientSize.Height - 60)
        pnlMessages.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        pnlMessages.AutoScroll = True
        pnlMessages.BackColor = modTheme.cWhite
        pnlMessages.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(pnlMessages)

        txtMessage.Location = New Point(10, Me.ClientSize.Height - 40)
        txtMessage.Size = New Size(Me.ClientSize.Width - 130, 30)
        txtMessage.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(txtMessage)

        btnSend.Text = "Send"
        btnSend.BackColor = modTheme.cOcean
        btnSend.ForeColor = modTheme.cWhite
        btnSend.FlatStyle = FlatStyle.Flat
        btnSend.FlatAppearance.BorderSize = 0
        btnSend.Location = New Point(Me.ClientSize.Width - 110, Me.ClientSize.Height - 40)
        btnSend.Size = New Size(100, 30)
        btnSend.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnSend.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnSend.Click, AddressOf SendClicked
        Me.Controls.Add(btnSend)

        Await OpenConversation()
    End Sub

    Private Async Function OpenConversation() As Task
        Try
            Dim msg As String = $"CHAT_OPEN|18|{Token}|{UserID}|{TraderID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(16384) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)

            pnlMessages.Controls.Clear()
            Dim yPos As Integer = 5 ' Track vertical position

            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then
                    Dim headerParts() As String = cleanLine.Split("|"c)
                    currentConvID = Integer.Parse(headerParts(2))
                    Continue For
                End If
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 7 AndAlso p(0) = "MSG" Then
                    yPos = AddMessageBubble(Integer.Parse(p(2)), p(4), p(5), yPos)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Error opening chat: " & ex.Message)
        End Try
    End Function

    Private Function AddMessageBubble(senderId As Integer, body As String, sentAt As String, yPos As Integer) As Integer
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

        ' Position the bubble
        bubble.Location = New Point(If(isMine, pnlMessages.Width - bubble.Width - 15, 15), yPos)
        pnlMessages.Controls.Add(bubble)

        ' Return the next Y position
        Return yPos + bubble.Height + 10
    End Function

    Private Async Sub SendClicked(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(txtMessage.Text.Trim()) Then Return
        Dim body As String = txtMessage.Text.Trim().Replace("|", "\|")
        txtMessage.Text = ""

        Try
            Dim msg As String = $"CHAT_SEND|19|{Token}|{currentConvID}|{UserID}|{body}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(512) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim p() As String = reply.Split("|"c)

            If p(0) = "OK" Then
                ' Refresh the conversation
                pnlMessages.Controls.Clear()
                Await OpenConversation()
            End If
        Catch ex As Exception
            MessageBox.Show("Error sending: " & ex.Message)
        End Try
    End Sub
End Class