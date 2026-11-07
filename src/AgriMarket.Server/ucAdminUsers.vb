Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]
Imports Microsoft.Data.Sqlite

Public Class ucAdminUsers
    Private lvUsers As New ListView()
    Private btnApprove As New Button()
    Private btnReject As New Button()
    Private btnRefresh As New Button()

    Private Sub ucAdminUsers_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildHeader()
        BuildTable()
        BuildActions()
        LoadUsers()
    End Sub

    Private Sub BuildHeader()
        Dim lblTitle As New Label()
        lblTitle.Text = "Users Management"
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
        AddHandler btnRefresh.Click, Sub(s, ev) LoadUsers()
        Me.Controls.Add(btnRefresh)
    End Sub

    Private Sub BuildTable()
        lvUsers.View = View.Details
        lvUsers.FullRowSelect = True
        lvUsers.GridLines = False
        lvUsers.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvUsers.Location = New Point(0, 70)
        lvUsers.Size = New Size(Me.Width, Me.Height - 160)
        lvUsers.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvUsers.BackColor = modTheme.cWhite
        lvUsers.BorderStyle = BorderStyle.FixedSingle

        lvUsers.Columns.Add("Name", 180)
        lvUsers.Columns.Add("Email", 200)
        lvUsers.Columns.Add("Role", 100)
        lvUsers.Columns.Add("Status", 100)
        lvUsers.Columns.Add("Phone", 120)
        lvUsers.Columns.Add("Joined", 120)

        Me.Controls.Add(lvUsers)
    End Sub

    Private Sub BuildActions()
        btnApprove.Text = "Approve"
        btnApprove.BackColor = modTheme.cOcean
        btnApprove.ForeColor = modTheme.cWhite
        btnApprove.FlatStyle = FlatStyle.Flat
        btnApprove.FlatAppearance.BorderSize = 0
        btnApprove.Location = New Point(0, Me.Height - 70)
        btnApprove.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnApprove.Size = New Size(150, 45)
        btnApprove.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnApprove.Click, AddressOf btnApprove_Click
        Me.Controls.Add(btnApprove)

        btnReject.Text = "Reject / Deactivate"
        btnReject.BackColor = modTheme.cWhite
        btnReject.ForeColor = modTheme.cDanger
        btnReject.FlatStyle = FlatStyle.Flat
        btnReject.FlatAppearance.BorderColor = modTheme.cDanger
        btnReject.FlatAppearance.BorderSize = 1
        btnReject.Location = New Point(160, Me.Height - 70)
        btnReject.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnReject.Size = New Size(180, 45)
        btnReject.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnReject.Click, AddressOf btnReject_Click
        Me.Controls.Add(btnReject)
    End Sub

    Private Sub LoadUsers()
        lvUsers.Items.Clear()
        Try
            Using conn As New SqliteConnection(modDB.ConnString)
                conn.Open()
                Dim sql As String = "SELECT UserID, DisplayName, Email, Role, Status, Phone, CreatedAt FROM Users ORDER BY CreatedAt DESC"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim lvi As New ListViewItem(reader("DisplayName").ToString())
                            lvi.SubItems.Add(reader("Email").ToString())
                            lvi.SubItems.Add(reader("Role").ToString())
                            lvi.SubItems.Add(reader("Status").ToString())
                            lvi.SubItems.Add(reader("Phone").ToString())
                            lvi.SubItems.Add(reader("CreatedAt").ToString())

                            Dim status As String = reader("Status").ToString()
                            Select Case status
                                Case "Active"
                                    lvi.ForeColor = modTheme.cSuccess
                                Case "Pending"
                                    lvi.ForeColor = modTheme.cWarning
                                Case "Suspended", "Deactivated"
                                    lvi.ForeColor = modTheme.cDanger
                            End Select

                            lvi.Tag = reader("UserID")
                            lvUsers.Items.Add(lvi)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading users: " & ex.Message)
        End Try
    End Sub

    Private Sub btnApprove_Click(sender As Object, e As EventArgs)
        If lvUsers.SelectedItems.Count = 0 Then Return
        Dim userId As Integer = Integer.Parse(lvUsers.SelectedItems(0).Tag.ToString())
        If modDB.ApproveUser(userId, 1) Then
            MessageBox.Show("User approved!")
            LoadUsers()
        End If
    End Sub

    Private Sub btnReject_Click(sender As Object, e As EventArgs)
        If lvUsers.SelectedItems.Count = 0 Then Return
        Dim userId As Integer = Integer.Parse(lvUsers.SelectedItems(0).Tag.ToString())
        If modDB.RejectUser(userId) Then
            MessageBox.Show("User rejected/deactivated.")
            LoadUsers()
        End If
    End Sub
End Class
