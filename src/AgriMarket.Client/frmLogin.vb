Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmLogin
    Private txtEmail As New TextBox()
    Private txtPass As New TextBox()

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "AgriMarket - Login"
        Me.ClientSize = New Size(800, 450)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        ' Left Brand Panel
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
        lblTagline.Text = "Sell fresh. Buy fast."
        lblTagline.Font = New Font("Segoe UI", 14, FontStyle.Italic)
        lblTagline.ForeColor = modTheme.cTeal
        lblTagline.Location = New Point(32, 100)
        lblTagline.AutoSize = True
        pnlLeft.Controls.Add(lblTagline)

        ' Right Form Fields
        Dim lblWelcome As New Label()
        lblWelcome.Text = "Welcome back"
        lblWelcome.Font = modTheme.fTitle
        lblWelcome.ForeColor = modTheme.cOcean
        lblWelcome.Location = New Point(400, 80)
        lblWelcome.AutoSize = True
        Me.Controls.Add(lblWelcome)

        Dim lblEmail As New Label()
        lblEmail.Text = "Email"
        lblEmail.ForeColor = modTheme.cText
        lblEmail.Location = New Point(405, 150)
        Me.Controls.Add(lblEmail)

        txtEmail.Location = New Point(405, 170)
        txtEmail.Size = New Size(300, 25)
        Me.Controls.Add(txtEmail)

        Dim lblPass As New Label()
        lblPass.Text = "Password"
        lblPass.ForeColor = modTheme.cText
        lblPass.Location = New Point(405, 210)
        Me.Controls.Add(lblPass)

        txtPass.Location = New Point(405, 230)
        txtPass.Size = New Size(300, 25)
        txtPass.UseSystemPasswordChar = True
        Me.Controls.Add(txtPass)

        Dim btnLogin As New Button()
        btnLogin.Text = "Log In"
        btnLogin.BackColor = modTheme.cOcean
        btnLogin.ForeColor = modTheme.cWhite
        btnLogin.FlatStyle = FlatStyle.Flat
        btnLogin.FlatAppearance.BorderSize = 0
        btnLogin.Location = New Point(405, 290)
        btnLogin.Size = New Size(300, 42)
        btnLogin.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        Me.Controls.Add(btnLogin)

        ' Bind the click event safely
        AddHandler btnLogin.Click, AddressOf btnLogin_Click

        ' Sign up Link (Placeholder)
        Dim lnkSignUp As New LinkLabel()
        lnkSignUp.Text = "No account yet? Sign up"
        lnkSignUp.ForeColor = modTheme.cTeal
        lnkSignUp.LinkColor = modTheme.cTeal
        lnkSignUp.Location = New Point(405, 350)
        lnkSignUp.AutoSize = True
        Me.Controls.Add(lnkSignUp)
    End Sub

    Private Async Sub btnLogin_Click(sender As Object, e As EventArgs)
        Dim email As String = txtEmail.Text.Trim()
        Dim password As String = txtPass.Text

        If String.IsNullOrEmpty(email) OrElse String.IsNullOrEmpty(password) Then
            MessageBox.Show("Please enter email and password.", "Validation")
            Return
        End If

        Try
            ' Format: LOGIN|ReqID|Token|email|password
            ' We use ReqID 2 because HELLO was 1.
            Dim loginMsg As String = $"LOGIN|2||{email}|{password}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(loginMsg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            ' Wait for the reply
            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                Dim role As String = parts(2)
                Dim userName As String = parts(3)
                ' Open the main dashboard and hide the login
                Dim mainForm As New frmClientMain(role, userName)
                mainForm.Show()
                Me.Hide()
            Else
                MessageBox.Show(parts(3), "Login Failed") ' parts(3) is the error message
            End If
        Catch ex As Exception
            MessageBox.Show("Error communicating with server: " & ex.Message)
        End Try
    End Sub
End Class