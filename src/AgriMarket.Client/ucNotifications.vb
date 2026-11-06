Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucNotifications
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UserID As Integer

    Private lvNotifs As New ListView()
    Private btnRefresh As New Button()

    Private Sub ucNotifications_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildHeader()
        BuildTable()
        LoadNotifications()
    End Sub

    Private Sub BuildHeader()
        Dim lblTitle As New Label()
        lblTitle.Text = "Notifications"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim lblSub As New Label()
        lblSub.Text = "Updates about your orders, stock, and messages."
        lblSub.Font = modTheme.fBody
        lblSub.ForeColor = modTheme.cMuted
        lblSub.Location = New Point(2, 45)
        lblSub.AutoSize = True
        Me.Controls.Add(lblSub)

        btnRefresh.Text = "Refresh"
        btnRefresh.BackColor = modTheme.cWhite
        btnRefresh.ForeColor = modTheme.cOcean
        btnRefresh.FlatStyle = FlatStyle.Flat
        btnRefresh.FlatAppearance.BorderColor = modTheme.cOcean
        btnRefresh.FlatAppearance.BorderSize = 1
        btnRefresh.Location = New Point(Me.Width - 130, 20)
        btnRefresh.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRefresh.Size = New Size(110, 40)
        btnRefresh.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnRefresh.Cursor = Cursors.Hand
        AddHandler btnRefresh.Click, Sub(s, ev) LoadNotifications()
        Me.Controls.Add(btnRefresh)
    End Sub

    Private Sub BuildTable()
        lvNotifs.View = View.Details
        lvNotifs.FullRowSelect = True
        lvNotifs.GridLines = False
        lvNotifs.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvNotifs.Location = New Point(0, 90)
        lvNotifs.Size = New Size(Me.Width, Me.Height - 100)
        lvNotifs.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvNotifs.BackColor = modTheme.cWhite
        lvNotifs.BorderStyle = BorderStyle.FixedSingle

        lvNotifs.Columns.Add("Type", 120)
        lvNotifs.Columns.Add("Title", 200)
        lvNotifs.Columns.Add("Message", 400)
        lvNotifs.Columns.Add("Status", 80)
        lvNotifs.Columns.Add("Date", 150)

        AddHandler lvNotifs.SelectedIndexChanged, AddressOf NotifSelected
        Me.Controls.Add(lvNotifs)
    End Sub

    Private Async Sub LoadNotifications()
        If String.IsNullOrEmpty(Token) Then Return
        Try
            Dim msg As String = $"NOTIF_LIST|12|{Token}|{UserID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(16384) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            lvNotifs.Items.Clear()

            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 7 AndAlso p(0) = "NOTIF" Then
                    Dim lvi As New ListViewItem(p(2)) ' Type
                    lvi.SubItems.Add(p(3))            ' Title
                    lvi.SubItems.Add(p(4))            ' Body
                    If p(5) = "0" Then
                        lvi.SubItems.Add("Unread")
                        lvi.ForeColor = modTheme.cWarning
                    Else
                        lvi.SubItems.Add("Read")
                        lvi.ForeColor = modTheme.cMuted
                    End If
                    lvi.SubItems.Add(p(6))            ' Date
                    lvi.Tag = p(1)                    ' NotifID
                    lvNotifs.Items.Add(lvi)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Error loading notifications: " & ex.Message)
        End Try
    End Sub

    Private Async Sub NotifSelected(sender As Object, e As EventArgs)
        If lvNotifs.SelectedItems.Count = 0 Then Return
        Dim item As ListViewItem = lvNotifs.SelectedItems(0)
        If item.SubItems(3).Text = "Unread" Then
            Dim notifId As Integer = Integer.Parse(item.Tag.ToString())
            Try
                Dim msg As String = $"NOTIF_READ|13|{Token}|{notifId}" & vbCrLf
                Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
                Await frmConnect.Stream.WriteAsync(data, 0, data.Length)
                Dim buffer(512) As Byte
                Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
                item.SubItems(3).Text = "Read"
                item.ForeColor = modTheme.cMuted
            Catch ex As Exception
            End Try
        End If
    End Sub
End Class
