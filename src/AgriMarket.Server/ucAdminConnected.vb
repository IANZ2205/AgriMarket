Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucAdminConnected
    Private lvConnected As New ListView()
    Private btnRefresh As New Button()

    Private Sub ucAdminConnected_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        LoadConnected()
    End Sub

    Private Sub BuildLayout()
        Dim lblTitle As New Label()
        lblTitle.Text = "Connected Users"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

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
        AddHandler btnRefresh.Click, Sub(s, ev) LoadConnected()
        Me.Controls.Add(btnRefresh)

        lvConnected.View = View.Details
        lvConnected.FullRowSelect = True
        lvConnected.GridLines = False
        lvConnected.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvConnected.Location = New Point(0, 70)
        lvConnected.Size = New Size(Me.Width, Me.Height - 80)
        lvConnected.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvConnected.BackColor = modTheme.cWhite
        lvConnected.BorderStyle = BorderStyle.FixedSingle

        lvConnected.Columns.Add("Name", 200)
        lvConnected.Columns.Add("Role", 120)
        lvConnected.Columns.Add("Email", 200)
        lvConnected.Columns.Add("Last Login", 150)

        Me.Controls.Add(lvConnected)
    End Sub

    Private Sub LoadConnected()
        Dim dt As DataTable = modDB.GetActiveSessions()
        lvConnected.Items.Clear()

        For Each row As DataRow In dt.Rows
            Dim lvi As New ListViewItem(row("DisplayName").ToString())
            lvi.SubItems.Add(row("Role").ToString())
            lvi.SubItems.Add(row("Email").ToString())
            lvi.SubItems.Add(row("LoginAt").ToString())
            lvi.SubItems.Add(row("IPAddress").ToString())
            lvConnected.Items.Add(lvi)
        Next
    End Sub
End Class