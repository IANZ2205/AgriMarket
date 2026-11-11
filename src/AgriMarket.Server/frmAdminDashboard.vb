Imports System.IO
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]
Imports Microsoft.Data.Sqlite

Public Class frmAdminDashboard
    Private contentPanel As Panel
    Private menuLabels As New List(Of Label)

    Private Sub frmAdminDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "AgriMarket - Admin Dashboard"
        Me.WindowState = FormWindowState.Maximized
        Me.BackColor = modTheme.cSurface
        Me.Font = modTheme.fBody

        BuildContentArea()
        BuildTopBar()
        LoadDashboard()
    End Sub

    Private Sub BuildTopBar()
        Dim topBar As New Panel()
        topBar.BackColor = modTheme.cOcean
        topBar.Dock = DockStyle.Top
        topBar.Height = 64
        Me.Controls.Add(topBar)

        Dim lblLogo As New Label()
        lblLogo.Text = "AgriMarket"
        lblLogo.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        lblLogo.ForeColor = modTheme.cWhite
        lblLogo.Location = New Point(20, 15)
        lblLogo.AutoSize = True
        topBar.Controls.Add(lblLogo)

        Dim menus As String() = {"Dashboard", "Users", "Listings", "Orders", "Reports", "Logs", "Connected", "Console", "Performance", "Settings"}
        Dim pnlMenu As New FlowLayoutPanel()
        pnlMenu.Dock = DockStyle.Fill
        pnlMenu.WrapContents = False
        pnlMenu.FlowDirection = FlowDirection.LeftToRight
        pnlMenu.Padding = New Padding(180, 22, 0, 0)
        pnlMenu.BackColor = Color.Transparent
        topBar.Controls.Add(pnlMenu)
        pnlMenu.BringToFront()

        ' --- 3. Loop through the menus ---
        For Each menuText In menus
            Dim lblMenu As New Label()
            lblMenu.Text = menuText
            lblMenu.Font = New Font("Segoe UI", 10, FontStyle.Regular)
            lblMenu.ForeColor = modTheme.cWhite
            lblMenu.AutoSize = True
            lblMenu.Cursor = Cursors.Hand
            lblMenu.Margin = New Padding(0, 0, 30, 0)
            AddHandler lblMenu.Click, Sub(s, ev) LoadScreen(menuText)
            pnlMenu.Controls.Add(lblMenu)
            menuLabels.Add(lblMenu)
        Next

        Dim lblUser As New Label()
        lblUser.Text = "Admin"
        lblUser.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblUser.ForeColor = modTheme.cWhite
        lblUser.AutoSize = True
        lblUser.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblUser.Location = New Point(topBar.Width - 100, 22)
        topBar.Controls.Add(lblUser)
    End Sub

    Private Sub BuildContentArea()
        contentPanel = New Panel()
        contentPanel.Dock = DockStyle.Fill
        contentPanel.BackColor = modTheme.cSurface
        contentPanel.Padding = New Padding(20)
        Me.Controls.Add(contentPanel)
    End Sub

    Private Sub LoadScreen(menuText As String)
        contentPanel.Controls.Clear()
        For Each lbl In menuLabels
            lbl.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        Next
        For Each lbl In menuLabels
            If lbl.Text = menuText Then
                lbl.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            End If
        Next

        Select Case menuText
                Case "Dashboard"
                    LoadDashboard()
                Case "Users"
                    Dim usersCtrl As New ucAdminUsers()
                    contentPanel.Controls.Add(usersCtrl)
                Case "Listings"
                    Dim listingsCtrl As New ucAdminListings()
                    contentPanel.Controls.Add(listingsCtrl)
                Case "Orders"
                    Dim ordersCtrl As New ucAdminOrders()
                    contentPanel.Controls.Add(ordersCtrl)
                Case "Reports"
                    Dim reportsCtrl As New ucAdminReports()
                    contentPanel.Controls.Add(reportsCtrl)
                Case "Logs"
                    Dim logsCtrl As New ucAdminLogs()
                    contentPanel.Controls.Add(logsCtrl)
                Case "Connected"
                    Dim connCtrl As New ucAdminConnected()
                    contentPanel.Controls.Add(connCtrl)
                Case "Console"
                    Dim consoleCtrl As New ucAdminConsole()
                    contentPanel.Controls.Add(consoleCtrl)
                Case "Performance"
                    Dim perfCtrl As New ucAdminPerformance()
                    contentPanel.Controls.Add(perfCtrl)
                Case "Settings"
                    Dim settingsCtrl As New ucAdminSettings()
                    contentPanel.Controls.Add(settingsCtrl)
                Case Else
                    Dim lblPlaceholder As New Label()
                    lblPlaceholder.Text = menuText & " Screen Coming Soon"
                    lblPlaceholder.Font = modTheme.fTitle
                    lblPlaceholder.ForeColor = modTheme.cMuted
                    lblPlaceholder.Location = New Point(20, 20)
                    lblPlaceholder.AutoSize = True
                    contentPanel.Controls.Add(lblPlaceholder)
            End Select
    End Sub


    Private Sub LoadDashboard()
        contentPanel.Controls.Clear()

        Dim lblTitle As New Label()
        lblTitle.Text = "System Overview"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        contentPanel.Controls.Add(lblTitle)

        Try
            Dim dt As DataTable = modDB.GetAdminStats()
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                Dim statCards As New FlowLayoutPanel()
                statCards.Location = New Point(20, 80)
                statCards.Size = New Size(contentPanel.Width - 40, 120)
                statCards.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
                statCards.FlowDirection = FlowDirection.LeftToRight
                statCards.WrapContents = False
                contentPanel.Controls.Add(statCards)

                statCards.Controls.Add(CreateStatCard("Traders", row("TotalTraders").ToString(), modTheme.cOcean))
                statCards.Controls.Add(CreateStatCard("Retailers & Wholesalers", row("TotalBuyers").ToString(), modTheme.cTeal))
                statCards.Controls.Add(CreateStatCard("Active Listings", row("ActiveListings").ToString(), modTheme.cSuccess))
                statCards.Controls.Add(CreateStatCard("Orders Today", row("OrdersToday").ToString(), modTheme.cWarning))
            End If

            ' Pending traders list
            Dim lblPending As New Label()
            lblPending.Text = "Traders Waiting for Approval"
            lblPending.Font = New Font("Segoe UI", 14, FontStyle.Bold)
            lblPending.ForeColor = modTheme.cOcean
            lblPending.Location = New Point(20, 220)
            lblPending.AutoSize = True
            contentPanel.Controls.Add(lblPending)

            Dim lvPending As New ListView()
            lvPending.View = View.Details
            lvPending.FullRowSelect = True
            lvPending.GridLines = False
            lvPending.Font = New Font("Segoe UI", 10, FontStyle.Regular)
            lvPending.Location = New Point(20, 260)
            lvPending.Size = New Size(contentPanel.Width - 60, 200)
            lvPending.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            lvPending.BackColor = modTheme.cWhite
            lvPending.BorderStyle = BorderStyle.FixedSingle

            lvPending.Columns.Add("Name", 180)
            lvPending.Columns.Add("Email", 200)
            lvPending.Columns.Add("Phone", 130)
            lvPending.Columns.Add("Registered", 130)

            Dim pendingDt As DataTable = modDB.GetPendingTraders()
            For Each prow As DataRow In pendingDt.Rows
                Dim lvi As New ListViewItem(prow("DisplayName").ToString())
                lvi.SubItems.Add(prow("Email").ToString())
                lvi.SubItems.Add(prow("Phone").ToString())
                lvi.SubItems.Add(prow("CreatedAt").ToString())
                lvi.Tag = prow("UserID")
                lvPending.Items.Add(lvi)
            Next

            contentPanel.Controls.Add(lvPending)
        Catch ex As Exception
            MessageBox.Show("Error loading dashboard: " & ex.Message)
        End Try
    End Sub

    Private Function CreateStatCard(title As String, value As String, accentColor As Color) As Panel
        Dim card As New Panel()
        card.BackColor = modTheme.cWhite
        card.Size = New Size(220, 100)
        card.Margin = New Padding(8)
        card.BorderStyle = BorderStyle.FixedSingle

        Dim accent As New Panel()
        accent.BackColor = accentColor
        accent.Size = New Size(8, 100)
        accent.Dock = DockStyle.Left
        card.Controls.Add(accent)

        Dim lblValue As New Label()
        lblValue.Text = value
        lblValue.Font = New Font("Segoe UI", 24, FontStyle.Bold)
        lblValue.ForeColor = accentColor
        lblValue.Location = New Point(20, 15)
        lblValue.AutoSize = True
        card.Controls.Add(lblValue)

        Dim lblTitle As New Label()
        lblTitle.Text = title
        lblTitle.Font = modTheme.fBody
        lblTitle.ForeColor = modTheme.cMuted
        lblTitle.Location = New Point(22, 60)
        lblTitle.AutoSize = True
        card.Controls.Add(lblTitle)

        Return card
    End Function
End Class