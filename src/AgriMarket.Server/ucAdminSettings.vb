Imports AgriMarket.Shared

Public Class ucAdminSettings
    Private dgvSettings As New DataGridView()
    Private btnSave As New Button()

    Private Sub ucAdminSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        LoadSettings()
    End Sub

    Private Sub BuildLayout()
        Dim lblTitle As New Label()
        lblTitle.Text = "System Settings"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        dgvSettings.Location = New Point(0, 60)
        dgvSettings.Size = New Size(Me.Width - 10, 300)
        dgvSettings.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        dgvSettings.BackgroundColor = modTheme.cWhite
        dgvSettings.BorderStyle = BorderStyle.FixedSingle
        dgvSettings.AllowUserToAddRows = False
        dgvSettings.AllowUserToDeleteRows = False
        dgvSettings.RowHeadersVisible = False
        dgvSettings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvSettings.ColumnCount = 3
        dgvSettings.Columns(0).Name = "Key"
        dgvSettings.Columns(1).Name = "Value"
        dgvSettings.Columns(2).Name = "Description"
        dgvSettings.Columns(0).ReadOnly = True
        dgvSettings.Columns(2).ReadOnly = True
        Me.Controls.Add(dgvSettings)

        btnSave.Text = "Save Changes"
        btnSave.BackColor = modTheme.cOcean
        btnSave.ForeColor = modTheme.cWhite
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.FlatAppearance.BorderSize = 0
        btnSave.Location = New Point(0, 380)
        btnSave.Size = New Size(150, 45)
        btnSave.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnSave.Click, AddressOf btnSave_Click
        Me.Controls.Add(btnSave)
    End Sub

    Private Sub LoadSettings()
        dgvSettings.Rows.Clear()
        Dim dt As DataTable = modDB.GetSettings()
        For Each row As DataRow In dt.Rows
            dgvSettings.Rows.Add(row("SettingKey").ToString(), row("SettingValue").ToString(), row("Description").ToString())
        Next
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs)
        For Each row As DataGridViewRow In dgvSettings.Rows
            Dim key As String = row.Cells(0).Value.ToString()
            Dim val As String = row.Cells(1).Value.ToString()
            modDB.UpdateSetting(key, val)
        Next
        MessageBox.Show("Settings saved.", "Success")
    End Sub
End Class