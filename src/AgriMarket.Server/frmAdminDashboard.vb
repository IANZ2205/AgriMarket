Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmAdminDashboard
    Private Sub frmAdminDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "AgriMarket - Admin Dashboard"
        Me.WindowState = FormWindowState.Maximized
        Me.BackColor = modTheme.cSurface
        Me.Font = modTheme.fBody

        BuildContentArea()
        BuildTopBar()
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

        Dim menuX As Integer = 200
        Dim menus As String() = {"Dashboard", "Users", "Listings", "Orders"}

        For Each menuText In menus
            Dim lblMenu As New Label()
            lblMenu.Text = menuText
            lblMenu.Font = New Font("Segoe UI", 10, FontStyle.Regular)
            lblMenu.ForeColor = modTheme.cWhite
            lblMenu.Location = New Point(menuX, 20)
            lblMenu.AutoSize = True
            lblMenu.Cursor = Cursors.Hand

            If menuText = "Dashboard" Then
                lblMenu.Font = New Font("Segoe UI", 10, FontStyle.Bold)
                Dim pill As New Panel()
                pill.BackColor = modTheme.cTeal
                pill.Size = New Size(lblMenu.Width, 3)
                pill.Location = New Point(menuX, 42)
                topBar.Controls.Add(pill)
            End If

            topBar.Controls.Add(lblMenu)
            menuX += lblMenu.Width + 30
        Next

        Dim lblAdmin As New Label()
        lblAdmin.Text = "Admin"
        lblAdmin.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblAdmin.ForeColor = modTheme.cWhite
        lblAdmin.AutoSize = True
        lblAdmin.Location = New Point(topBar.Width - 100, 22)
        lblAdmin.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        topBar.Controls.Add(lblAdmin)
    End Sub

    Private Sub BuildContentArea()
        Dim contentPanel As New Panel()
        contentPanel.Name = "contentPanel"
        contentPanel.Dock = DockStyle.Fill
        contentPanel.BackColor = modTheme.cSurface
        contentPanel.Padding = New Padding(20)
        Me.Controls.Add(contentPanel)

        Dim lblTitle As New Label()
        lblTitle.Text = "System Overview"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        contentPanel.Controls.Add(lblTitle)

        ' --- ADD 4 STAT CARDS ---
        ' We position them side by side
        contentPanel.Controls.Add(CreateStatCard("Traders", "128", 20, 80, modTheme.cOcean))
        contentPanel.Controls.Add(CreateStatCard("Retailers & Wholesalers", "342", 270, 80, modTheme.cTeal))
        contentPanel.Controls.Add(CreateStatCard("Active Listings", "516", 520, 80, modTheme.cSuccess))
        contentPanel.Controls.Add(CreateStatCard("Orders Today", "47", 770, 80, modTheme.cWarning))
    End Sub

    ' --- HELPER FUNCTION TO BUILD A SINGLE CARD ---
    Private Function CreateStatCard(title As String, value As String, x As Integer, y As Integer, accentColor As Color) As Panel
        Dim card As New Panel()
        card.BackColor = modTheme.cWhite
        card.Size = New Size(230, 100)
        card.Location = New Point(x, y)
        card.BorderStyle = BorderStyle.FixedSingle

        ' Colored accent block on the left
        Dim accent As New Panel()
        accent.BackColor = accentColor
        accent.Size = New Size(8, 100)
        accent.Dock = DockStyle.Left
        card.Controls.Add(accent)

        ' The Big Number
        Dim lblValue As New Label()
        lblValue.Text = value
        lblValue.Font = New Font("Segoe UI", 24, FontStyle.Bold)
        lblValue.ForeColor = accentColor
        lblValue.Location = New Point(20, 15)
        lblValue.AutoSize = True
        card.Controls.Add(lblValue)

        ' The Title
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