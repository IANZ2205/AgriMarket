Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucAdminLogs
    Private cmbLevel As New ComboBox()
    Private txtSearch As New TextBox()
    Private btnSearch As New Button()
    Private lvLogs As New ListView()

    Private Sub ucAdminLogs_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        LoadLogs()
    End Sub

    Private Sub BuildLayout()
        Dim lblTitle As New Label()
        lblTitle.Text = "Server Logs"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim lblLevel As New Label()
        lblLevel.Text = "Level:"
        lblLevel.Location = New Point(0, 60)
        lblLevel.AutoSize = True
        Me.Controls.Add(lblLevel)

        cmbLevel.Items.AddRange(New Object() {"All", "Info", "Warning", "Error"})
        cmbLevel.Location = New Point(0, 80)
        cmbLevel.Size = New Size(120, 25)
        cmbLevel.DropDownStyle = ComboBoxStyle.DropDownList
        cmbLevel.SelectedIndex = 0
        Me.Controls.Add(cmbLevel)

        Dim lblSearch As New Label()
        lblSearch.Text = "Search:"
        lblSearch.Location = New Point(140, 60)
        lblSearch.AutoSize = True
        Me.Controls.Add(lblSearch)

        txtSearch.Location = New Point(140, 80)
        txtSearch.Size = New Size(300, 25)
        Me.Controls.Add(txtSearch)

        btnSearch.Text = "Filter"
        btnSearch.BackColor = modTheme.cOcean
        btnSearch.ForeColor = modTheme.cWhite
        btnSearch.FlatStyle = FlatStyle.Flat
        btnSearch.FlatAppearance.BorderSize = 0
        btnSearch.Location = New Point(460, 80)
        btnSearch.Size = New Size(100, 28)
        btnSearch.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnSearch.Click, Sub(s, ev) LoadLogs()
        Me.Controls.Add(btnSearch)

        lvLogs.View = View.Details
        lvLogs.FullRowSelect = True
        lvLogs.GridLines = False
        lvLogs.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvLogs.Location = New Point(0, 120)
        lvLogs.Size = New Size(Me.Width, Me.Height - 130)
        lvLogs.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvLogs.BackColor = modTheme.cWhite
        lvLogs.BorderStyle = BorderStyle.FixedSingle

        lvLogs.Columns.Add("Time", 150)
        lvLogs.Columns.Add("Level", 80)
        lvLogs.Columns.Add("Event", 150)
        lvLogs.Columns.Add("Details", 400)

        Me.Controls.Add(lvLogs)
    End Sub

    Private Sub LoadLogs()
        Dim level As String = cmbLevel.SelectedItem.ToString()
        Dim search As String = txtSearch.Text.Trim()
        Dim dt As DataTable = modDB.GetServerLogs(level, search)
        lvLogs.Items.Clear()

        For Each row As DataRow In dt.Rows
            Dim lvi As New ListViewItem(row("LoggedAt").ToString())
            lvi.SubItems.Add(row("Level").ToString())
            lvi.SubItems.Add(row("EventType").ToString())
            lvi.SubItems.Add(row("Details").ToString())

            Select Case row("Level").ToString()
                Case "Info"
                    lvi.ForeColor = modTheme.cSuccess
                Case "Warning"
                    lvi.ForeColor = modTheme.cWarning
                Case "Error"
                    lvi.ForeColor = modTheme.cDanger
            End Select

            lvLogs.Items.Add(lvi)
        Next
    End Sub
End Class