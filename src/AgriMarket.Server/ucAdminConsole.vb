Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucAdminConsole
    Private lblStatus As New Label()
    Private btnStart As New Button()
    Private btnStop As New Button()
    Private lblUsers As New Label()
    Private lblRequests As New Label()
    Private lblErrors As New Label()
    Private lvActivity As New ListView()

    Private Sub ucAdminConsole_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        UpdateStatus()
        LoadActivity()
    End Sub

    Private Sub BuildLayout()
        Dim lblTitle As New Label()
        lblTitle.Text = "Server Console"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        ' Status Panel
        Dim pnlStatus As New Panel()
        pnlStatus.BackColor = modTheme.cWhite
        pnlStatus.BorderStyle = BorderStyle.FixedSingle
        pnlStatus.Location = New Point(0, 60)
        pnlStatus.Size = New Size(Me.Width - 10, 80)
        pnlStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(pnlStatus)

        lblStatus.Text = "Server is stopped."
        lblStatus.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblStatus.ForeColor = modTheme.cDanger
        lblStatus.Location = New Point(15, 25)
        lblStatus.AutoSize = True
        pnlStatus.Controls.Add(lblStatus)

        btnStart.Text = "Start Server"
        btnStart.BackColor = modTheme.cSuccess
        btnStart.ForeColor = modTheme.cWhite
        btnStart.FlatStyle = FlatStyle.Flat
        btnStart.FlatAppearance.BorderSize = 0
        btnStart.Location = New Point(300, 20)
        btnStart.Size = New Size(120, 35)
        btnStart.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnStart.Click, AddressOf btnStart_Click
        pnlStatus.Controls.Add(btnStart)

        btnStop.Text = "Stop Server"
        btnStop.BackColor = modTheme.cDanger
        btnStop.ForeColor = modTheme.cWhite
        btnStop.FlatStyle = FlatStyle.Flat
        btnStop.FlatAppearance.BorderSize = 0
        btnStop.Location = New Point(430, 20)
        btnStop.Size = New Size(120, 35)
        btnStop.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnStop.Click, AddressOf btnStop_Click
        pnlStatus.Controls.Add(btnStop)

        ' Stats
        Dim pnlStats As New FlowLayoutPanel()
        pnlStats.Location = New Point(0, 160)
        pnlStats.Size = New Size(Me.Width - 10, 100)
        pnlStats.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pnlStats.FlowDirection = FlowDirection.LeftToRight
        pnlStats.WrapContents = False
        Me.Controls.Add(pnlStats)

        pnlStats.Controls.Add(CreateStatCard("Users Online", "0", modTheme.cOcean))
        pnlStats.Controls.Add(CreateStatCard("Requests Today", "0", modTheme.cTeal))
        pnlStats.Controls.Add(CreateStatCard("Errors Today", "0", modTheme.cDanger))

        ' Live Activity
        Dim lblActivity As New Label()
        lblActivity.Text = "Live Activity"
        lblActivity.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblActivity.ForeColor = modTheme.cOcean
        lblActivity.Location = New Point(0, 280)
        lblActivity.AutoSize = True
        Me.Controls.Add(lblActivity)

        lvActivity.View = View.Details
        lvActivity.FullRowSelect = True
        lvActivity.GridLines = False
        lvActivity.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvActivity.Location = New Point(0, 310)
        lvActivity.Size = New Size(Me.Width - 10, 200)
        lvActivity.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lvActivity.BackColor = modTheme.cWhite
        lvActivity.BorderStyle = BorderStyle.FixedSingle
        lvActivity.Columns.Add("Time", 150)
        lvActivity.Columns.Add("Level", 80)
        lvActivity.Columns.Add("Event", 150)
        lvActivity.Columns.Add("Details", 400)
        Me.Controls.Add(lvActivity)
    End Sub

    Private Function CreateStatCard(title As String, value As String, accentColor As Color) As Panel
        Dim card As New Panel()
        card.BackColor = modTheme.cWhite
        card.Size = New Size(200, 90)
        card.Margin = New Padding(5)
        card.BorderStyle = BorderStyle.FixedSingle

        Dim accent As New Panel()
        accent.BackColor = accentColor
        accent.Size = New Size(8, 90)
        accent.Dock = DockStyle.Left
        card.Controls.Add(accent)

        Dim lblVal As New Label()
        lblVal.Text = value
        lblVal.Font = New Font("Segoe UI", 20, FontStyle.Bold)
        lblVal.ForeColor = accentColor
        lblVal.Location = New Point(15, 10)
        lblVal.AutoSize = True
        card.Controls.Add(lblVal)

        Dim lblTit As New Label()
        lblTit.Text = title
        lblTit.Font = modTheme.fBody
        lblTit.ForeColor = modTheme.cMuted
        lblTit.Location = New Point(17, 50)
        lblTit.AutoSize = True
        card.Controls.Add(lblTit)

        Return card
    End Function

    Private Sub UpdateStatus()
        If modServer.IsListening Then
            lblStatus.Text = "Server is running on port 5000."
            lblStatus.ForeColor = modTheme.cSuccess
        Else
            lblStatus.Text = "Server is stopped."
            lblStatus.ForeColor = modTheme.cDanger
        End If
    End Sub

    Private Sub btnStart_Click(sender As Object, e As EventArgs)
        modServer.StartListening()
        UpdateStatus()
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs)
        modServer.StopListening()
        UpdateStatus()
    End Sub

    Private Sub LoadActivity()
        Dim dt As DataTable = modDB.GetServerLogs("All", "")
        lvActivity.Items.Clear()
        For Each row As DataRow In dt.Rows
            Dim lvi As New ListViewItem(row("LoggedAt").ToString())
            lvi.SubItems.Add(row("Level").ToString())
            lvi.SubItems.Add(row("EventType").ToString())
            lvi.SubItems.Add(row("Details").ToString())
            lvActivity.Items.Add(lvi)
        Next
    End Sub
End Class