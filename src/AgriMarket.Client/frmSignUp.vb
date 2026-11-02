Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmSignUp
    Private cmbRole As New ComboBox()
    Private txtFirstName As New TextBox()
    Private txtLastName As New TextBox()
    Private txtDisplayName As New TextBox()
    Private txtEmail As New TextBox()
    Private txtPhone As New TextBox()
    Private txtPassword As New TextBox()
    Private txtConfirm As New TextBox()
    Private cmbDistrict As New ComboBox()

    Private Sub frmSignUp_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "AgriMarket - Sign Up"
        Me.ClientSize = New Size(850, 600)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        ' Left brand panel
        Dim pnlLeft As New Panel()
        pnlLeft.BackColor = modTheme.cOcean
        pnlLeft.Dock = DockStyle.Left
        pnlLeft.Width = 300
        Me.Controls.Add(pnlLeft)

        Dim lblBrand As New Label()
        lblBrand.Text = "AgriMarket"
        lblBrand.Font = New Font("Segoe UI", 24, FontStyle.Bold)
        lblBrand.ForeColor = modTheme.cWhite
        lblBrand.Location = New Point(30, 50)
        lblBrand.AutoSize = True
        pnlLeft.Controls.Add(lblBrand)

        Dim lblTagline As New Label()
        lblTagline.Text = "Join the market."
        lblTagline.Font = New Font("Segoe UI", 12, FontStyle.Italic)
        lblTagline.ForeColor = modTheme.cTeal
        lblTagline.Location = New Point(32, 90)
        lblTagline.AutoSize = True
        pnlLeft.Controls.Add(lblTagline)

        Dim lblNote As New Label()
        lblNote.Text = "Traders are approved by an admin before they can post stock."
        lblNote.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        lblNote.ForeColor = modTheme.cWhite
        lblNote.Location = New Point(32, 480)
        lblNote.Size = New Size(240, 40)
        pnlLeft.Controls.Add(lblNote)

        ' Right form fields
        Dim lblTitle As New Label()
        lblTitle.Text = "Create your account"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(340, 30)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        ' Row 1: Role switch
        AddLabel("I am a:", 340, 80)
        cmbRole.Items.AddRange(New Object() {"Trader", "Retailer", "Wholesaler"})
        cmbRole.Location = New Point(340, 100)
        cmbRole.Size = New Size(200, 25)
        cmbRole.DropDownStyle = ComboBoxStyle.DropDownList
        cmbRole.SelectedIndex = 0
        Me.Controls.Add(cmbRole)

        ' Row 2: First & Last Name
        AddField("First Name", txtFirstName, 340, 150)
        AddField("Last Name", txtLastName, 540, 150)

        ' Row 3: Display Name (for traders) & Email
        ' AddField("Display Name (username)", txtDisplayName, 340, 210)
        AddField("Email (your login)", txtEmail, 540, 210)

        ' Row 4: Phone & District
        AddField("Phone number", txtPhone, 340, 270)
        AddLabel("Location (district)", 540, 270)
        cmbDistrict.Items.AddRange(New Object() {"Kampala", "Gulu", "Lira", "Mbale", "Masaka", "Mbarara", "Jinja"})
        cmbDistrict.Location = New Point(540, 290)
        cmbDistrict.Size = New Size(200, 25)
        cmbDistrict.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDistrict.SelectedIndex = 0
        Me.Controls.Add(cmbDistrict)

        ' Row 5: Password & Confirm
        AddField("Password", txtPassword, 340, 330)
        txtPassword.UseSystemPasswordChar = True
        AddField("Confirm password", txtConfirm, 540, 330)
        txtConfirm.UseSystemPasswordChar = True

        ' Buttons
        Dim btnCreate As New Button()
        btnCreate.Text = "Create Account"
        btnCreate.BackColor = modTheme.cOcean
        btnCreate.ForeColor = modTheme.cWhite
        btnCreate.FlatStyle = FlatStyle.Flat
        btnCreate.FlatAppearance.BorderSize = 0
        btnCreate.Location = New Point(340, 430)
        btnCreate.Size = New Size(200, 45)
        btnCreate.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnCreate.Click, AddressOf btnCreate_Click
        Me.Controls.Add(btnCreate)

        Dim btnCancel As New Button()
        btnCancel.Text = "Cancel"
        btnCancel.BackColor = modTheme.cWhite
        btnCancel.ForeColor = modTheme.cOcean
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.FlatAppearance.BorderColor = modTheme.cOcean
        btnCancel.FlatAppearance.BorderSize = 1
        btnCancel.Location = New Point(560, 430)
        btnCancel.Size = New Size(150, 45)
        btnCancel.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnCancel.Click, Sub(s, ev) Me.Close()
        Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Location = New Point(x, y)
        lbl.AutoSize = True
        lbl.ForeColor = modTheme.cText
        Me.Controls.Add(lbl)
    End Sub

    Private Sub AddField(labelText As String, txt As TextBox, x As Integer, y As Integer)
        AddLabel(labelText, x, y)
        txt.Location = New Point(x, y + 20)
        txt.Size = New Size(200, 25)
        Me.Controls.Add(txt)
    End Sub

    Private Async Sub btnCreate_Click(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(txtFirstName.Text) OrElse String.IsNullOrEmpty(txtLastName.Text) OrElse
           String.IsNullOrEmpty(txtEmail.Text) OrElse String.IsNullOrEmpty(txtPassword.Text) Then
            MessageBox.Show("Please fill in all required fields.", "Validation")
            Return
        End If
        If txtPassword.Text <> txtConfirm.Text Then
            MessageBox.Show("Passwords do not match.", "Validation")
            Return
        End If

        Try
            Dim role As String = cmbRole.SelectedItem.ToString()
            Dim reqId As String = DateTime.Now.Ticks.ToString()

            Dim autoDisplayName As String = txtFirstName.Text.Trim() & " " & txtLastName.Text.Trim()
            Dim msg As String = "REGISTER|" & reqId & "||" & role & "|" & autoDisplayName & "|" & txtEmail.Text & "|" & txtPhone.Text & "|" & cmbDistrict.SelectedItem.ToString() & "|" & txtPassword.Text & "|" & txtFirstName.Text & "|" & txtLastName.Text & vbCrLf
            ' REGISTER|ReqID|Token|Role|DisplayName|Email|Phone|District|Password|FirstName|LastName
            'Dim msg As String = "REGISTER|" & reqId & "||" & role & "|" & txtDisplayName.Text & "|" & txtEmail.Text & "|" & txtPhone.Text & "|" & cmbDistrict.SelectedItem.ToString() & "|" & txtPassword.Text & "|" & txtFirstName.Text & "|" & txtLastName.Text & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Account created! You can now log in.", "Success")
                Me.Close()
            Else
                MessageBox.Show("Error: " & parts(3), "Registration Failed")
            End If
        Catch ex As Exception
            MessageBox.Show("Connection Error: " & ex.Message)
        End Try
    End Sub
End Class