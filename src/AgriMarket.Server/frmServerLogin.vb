Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmServerLogin
    ' --- We declare these at the top of the class so the button can see them ---
    Private txtEmail As New TextBox()
    Private txtPass As New TextBox()

    Private Sub frmServerLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "AgriMarket Server"
        Me.ClientSize = New Size(800, 450)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        ' ---> ADD THESE TWO LINES HERE <---
        modDB.InitializeDatabase()
        modServer.StartListening()

        BuildLayout()
    End Sub

    ' ---> ADD THIS TOO TO CLEAN UP WHEN CLOSING <---
    Private Sub frmServerLogin_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        modServer.StopListening()
    End Sub
    Private Sub BuildLayout()
        ' LEFT PANEL
        Dim pnlLeft As New Panel()
        pnlLeft.BackColor = modTheme.cOcean
        pnlLeft.Dock = DockStyle.Left
        pnlLeft.Width = 350
        Me.Controls.Add(pnlLeft)

        Dim lblBrand As New Label()
        lblBrand.Text = "AgriMarket"
        lblBrand.Font = New Font("Segoe UI", 28, FontStyle.Bold)
        lblBrand.ForeColor = modTheme.cWhite
        lblBrand.Location = New Point(30, 50)
        lblBrand.AutoSize = True
        pnlLeft.Controls.Add(lblBrand)

        Dim lblTagline As New Label()
        lblTagline.Text = "Server Control Centre"
        lblTagline.Font = New Font("Segoe UI", 14, FontStyle.Regular)
        lblTagline.ForeColor = modTheme.cTeal
        lblTagline.Location = New Point(32, 100)
        lblTagline.AutoSize = True
        pnlLeft.Controls.Add(lblTagline)

        Dim pnlStatus As New Panel()
        pnlStatus.BackColor = Color.FromArgb(0, 80, 85)
        pnlStatus.Location = New Point(30, 330)
        pnlStatus.Size = New Size(290, 80)
        pnlLeft.Controls.Add(pnlStatus)

        Dim lblStatusTitle As New Label()
        lblStatusTitle.Text = "Server Status"
        lblStatusTitle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lblStatusTitle.ForeColor = modTheme.cTeal
        lblStatusTitle.Location = New Point(15, 10)
        pnlStatus.Controls.Add(lblStatusTitle)

        Dim lblStatusLive As New Label()
        lblStatusLive.Text = "● Listening on port 5000"
        lblStatusLive.Font = modTheme.fBody
        lblStatusLive.ForeColor = modTheme.cWhite
        lblStatusLive.Location = New Point(15, 35)
        lblStatusLive.AutoSize = True
        pnlStatus.Controls.Add(lblStatusLive)

        ' RIGHT PANEL
        Dim lblWelcome As New Label()
        lblWelcome.Text = "Admin Login"
        lblWelcome.Font = modTheme.fTitle
        lblWelcome.ForeColor = modTheme.cOcean
        lblWelcome.Location = New Point(400, 80)
        lblWelcome.AutoSize = True
        Me.Controls.Add(lblWelcome)

        Dim lblSub As New Label()
        lblSub.Text = "Only administrators can open the dashboard."
        lblSub.ForeColor = modTheme.cMuted
        lblSub.Location = New Point(405, 130)
        lblSub.AutoSize = True
        Me.Controls.Add(lblSub)

        ' Email Field
        Dim lblEmail As New Label()
        lblEmail.Text = "Admin Email"
        lblEmail.ForeColor = modTheme.cText
        lblEmail.Location = New Point(405, 180)
        Me.Controls.Add(lblEmail)

        ' Note: We don't use "Dim" here because it's declared at the top
        txtEmail.Location = New Point(405, 200)
        txtEmail.Size = New Size(300, 25)
        txtEmail.Font = modTheme.fBody
        Me.Controls.Add(txtEmail)

        ' Password Field
        Dim lblPass As New Label()
        lblPass.Text = "Password"
        lblPass.ForeColor = modTheme.cText
        lblPass.Location = New Point(405, 240)
        Me.Controls.Add(lblPass)

        ' Note: We don't use "Dim" here either
        txtPass.Location = New Point(405, 260)
        txtPass.Size = New Size(300, 25)
        txtPass.UseSystemPasswordChar = True
        txtPass.Font = modTheme.fBody
        Me.Controls.Add(txtPass)

        ' Login Button
        Dim btnLogin As New Button()
        btnLogin.Text = "Log In"
        btnLogin.BackColor = modTheme.cOcean
        btnLogin.ForeColor = modTheme.cWhite
        btnLogin.FlatStyle = FlatStyle.Flat
        btnLogin.FlatAppearance.BorderSize = 0
        btnLogin.Location = New Point(405, 320)
        btnLogin.Size = New Size(300, 42)
        btnLogin.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnLogin.Cursor = Cursors.Hand
        AddHandler btnLogin.Click, AddressOf btnLogin_Click
        Me.Controls.Add(btnLogin)
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs)
        Dim email As String = txtEmail.Text.Trim()
        Dim password As String = txtPass.Text

        If String.IsNullOrEmpty(email) OrElse String.IsNullOrEmpty(password) Then
            MessageBox.Show("Please enter both email and password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If modDB.AuthenticateUser(email, password) Then
            MessageBox.Show("Login Successful!", "Success")
            Dim dashboard As New frmAdminDashboard()
            dashboard.Show()
            Me.Hide()
        Else
            MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub
End Class