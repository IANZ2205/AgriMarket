Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucAdminPerformance
    Private lblCpu As New Label()
    Private lblMem As New Label()
    Private lblUsers As New Label()

    Private Sub ucAdminPerformance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        LoadStats()
    End Sub

    Private Sub BuildLayout()
        Dim lblTitle As New Label()
        lblTitle.Text = "Live Performance"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim pnlStats As New FlowLayoutPanel()
        pnlStats.Location = New Point(0, 60)
        pnlStats.Size = New Size(Me.Width - 10, 100)
        pnlStats.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pnlStats.FlowDirection = FlowDirection.LeftToRight
        pnlStats.WrapContents = False
        Me.Controls.Add(pnlStats)

        ' CPU
        Dim pnlCpu As New Panel()
        pnlCpu.BackColor = modTheme.cWhite
        pnlCpu.Size = New Size(200, 90)
        pnlCpu.BorderStyle = BorderStyle.FixedSingle
        lblCpu.Text = "0%"
        lblCpu.Font = New Font("Segoe UI", 20, FontStyle.Bold)
        lblCpu.ForeColor = modTheme.cOcean
        lblCpu.Location = New Point(15, 10)
        lblCpu.AutoSize = True
        pnlCpu.Controls.Add(lblCpu)
        Dim lblCpuTitle As New Label()
        lblCpuTitle.Text = "CPU Usage"
        lblCpuTitle.Location = New Point(17, 50)
        pnlCpu.Controls.Add(lblCpuTitle)
        pnlStats.Controls.Add(pnlCpu)

        ' Mem
        Dim pnlMem As New Panel()
        pnlMem.BackColor = modTheme.cWhite
        pnlMem.Size = New Size(200, 90)
        pnlMem.BorderStyle = BorderStyle.FixedSingle
        lblMem.Text = "0 MB"
        lblMem.Font = New Font("Segoe UI", 20, FontStyle.Bold)
        lblMem.ForeColor = modTheme.cTeal
        lblMem.Location = New Point(15, 10)
        lblMem.AutoSize = True
        pnlMem.Controls.Add(lblMem)
        Dim lblMemTitle As New Label()
        lblMemTitle.Text = "Memory Used"
        lblMemTitle.Location = New Point(17, 50)
        pnlMem.Controls.Add(lblMemTitle)
        pnlStats.Controls.Add(pnlMem)

        ' Users
        Dim pnlUsers As New Panel()
        pnlUsers.BackColor = modTheme.cWhite
        pnlUsers.Size = New Size(200, 90)
        pnlUsers.BorderStyle = BorderStyle.FixedSingle
        lblUsers.Text = "0"
        lblUsers.Font = New Font("Segoe UI", 20, FontStyle.Bold)
        lblUsers.ForeColor = modTheme.cSuccess
        lblUsers.Location = New Point(15, 10)
        lblUsers.AutoSize = True
        pnlUsers.Controls.Add(lblUsers)
        Dim lblUsersTitle As New Label()
        lblUsersTitle.Text = "Active Sessions"
        lblUsersTitle.Location = New Point(17, 50)
        pnlUsers.Controls.Add(lblUsersTitle)
        pnlStats.Controls.Add(pnlUsers)
    End Sub

    Private Sub LoadStats()
        Try
            ' CPU
            Dim cpuCounter As New System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total")
            cpuCounter.NextValue()
            System.Threading.Thread.Sleep(100)
            lblCpu.Text = Math.Round(cpuCounter.NextValue()).ToString() & "%"

            ' Memory
            Dim proc As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess()
            lblMem.Text = Math.Round(proc.WorkingSet64 / (1024 * 1024)).ToString() & " MB"

            ' Sessions
            Dim dt As DataTable = modDB.GetActiveSessions()
            lblUsers.Text = dt.Rows.Count.ToString()
        Catch ex As Exception
        End Try
    End Sub
End Class