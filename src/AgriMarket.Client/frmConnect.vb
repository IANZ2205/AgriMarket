Imports System.Net.Sockets
Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmConnect
    Private txtIP As New TextBox()
    Private txtPort As New TextBox()
    Private lblStatus As New Label()
    Private btnConnect As New Button()

    ' Connection objects
    Public Shared Client As TcpClient
    Public Shared Stream As NetworkStream
    Public Shared Token As String

    Private Sub frmConnect_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "AgriMarket Client"
        Me.ClientSize = New Size(800, 450)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        BuildLayout()
    End Sub

    Private Sub BuildLayout()
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

        Dim lblTitle As New Label()
        lblTitle.Text = "Connect to Server"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(400, 80)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim lblIP As New Label()
        lblIP.Text = "Server IP Address"
        lblIP.ForeColor = modTheme.cText
        lblIP.Location = New Point(405, 150)
        Me.Controls.Add(lblIP)

        txtIP.Text = "127.0.0.1"
        txtIP.Location = New Point(405, 170)
        txtIP.Size = New Size(200, 25)
        Me.Controls.Add(txtIP)

        Dim lblPort As New Label()
        lblPort.Text = "Port"
        lblPort.ForeColor = modTheme.cText
        lblPort.Location = New Point(620, 150)
        Me.Controls.Add(lblPort)

        txtPort.Text = "5000"
        txtPort.Location = New Point(620, 170)
        txtPort.Size = New Size(85, 25)
        Me.Controls.Add(txtPort)

        btnConnect.Text = "Connect"
        btnConnect.BackColor = modTheme.cOcean
        btnConnect.ForeColor = modTheme.cWhite
        btnConnect.FlatStyle = FlatStyle.Flat
        btnConnect.FlatAppearance.BorderSize = 0
        btnConnect.Location = New Point(405, 230)
        btnConnect.Size = New Size(300, 42)
        btnConnect.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnConnect.Cursor = Cursors.Hand
        AddHandler btnConnect.Click, AddressOf btnConnect_Click
        Me.Controls.Add(btnConnect)

        lblStatus.Text = "Not connected."
        lblStatus.ForeColor = modTheme.cDanger
        lblStatus.Location = New Point(405, 290)
        lblStatus.AutoSize = True
        Me.Controls.Add(lblStatus)
    End Sub

    ' --- Helper to close existing sockets cleanly ---
    Public Shared Sub DisposeConnection()
        Try
            If Stream IsNot Nothing Then
                Stream.Close()
                Stream = Nothing
            End If
        Catch
        End Try
        Try
            If Client IsNot Nothing Then
                Client.Close()
                Client = Nothing
            End If
        Catch
        End Try
        Token = Nothing
    End Sub

    Private Async Sub btnConnect_Click(sender As Object, e As EventArgs)
        btnConnect.Enabled = False
        lblStatus.ForeColor = modTheme.cWarning
        lblStatus.Text = "Connecting..."

        ' Make sure no previous connection is left dangling
        DisposeConnection()

        Try
            Client = New TcpClient()
            Await Client.ConnectAsync(txtIP.Text, Integer.Parse(txtPort.Text))
            Stream = Client.GetStream()

            Dim helloMsg As String = "HELLO|1||1.0" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(helloMsg)
            Await Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            If reply.StartsWith("OK") Then
                lblStatus.ForeColor = modTheme.cSuccess
                lblStatus.Text = "Connected successfully!"
                Dim loginForm As New frmLogin()
                loginForm.Show()
                Me.Hide()
            Else
                MessageBox.Show("Server rejected connection: " & reply)
                btnConnect.Enabled = True
            End If
        Catch ex As Exception
            lblStatus.ForeColor = modTheme.cDanger
            lblStatus.Text = "Connection failed: " & ex.Message
            btnConnect.Enabled = True
        End Try
    End Sub
End Class