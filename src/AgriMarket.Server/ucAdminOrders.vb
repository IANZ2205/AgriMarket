Imports AgriMarket.Shared

Public Class ucAdminOrders
    Private lvOrders As New ListView()
    Private btnRefresh As New Button()

    Private Sub ucAdminOrders_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        LoadOrders()
    End Sub

    Private Sub BuildLayout()
        Dim lblTitle As New Label()
        lblTitle.Text = "All Orders"
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
        AddHandler btnRefresh.Click, Sub(s, ev) LoadOrders()
        Me.Controls.Add(btnRefresh)

        lvOrders.View = View.Details
        lvOrders.FullRowSelect = True
        lvOrders.GridLines = False
        lvOrders.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvOrders.Location = New Point(0, 70)
        lvOrders.Size = New Size(Me.Width - 10, Me.Height - 80)
        lvOrders.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvOrders.BackColor = modTheme.cWhite
        lvOrders.BorderStyle = BorderStyle.FixedSingle

        lvOrders.Columns.Add("Order No", 120)
        lvOrders.Columns.Add("Date", 130)
        lvOrders.Columns.Add("Item", 150)
        lvOrders.Columns.Add("Buyer", 150)
        lvOrders.Columns.Add("Trader", 150)
        lvOrders.Columns.Add("Total (UGX)", 120)
        lvOrders.Columns.Add("Status", 100)

        Me.Controls.Add(lvOrders)
    End Sub

    Private Sub LoadOrders()
        lvOrders.Items.Clear()
        Dim dt As DataTable = modDB.GetAllOrders()
        For Each row As DataRow In dt.Rows
            Dim lvi As New ListViewItem(row("OrderNo").ToString())
            lvi.SubItems.Add(row("CreatedAt").ToString())
            lvi.SubItems.Add(row("ItemName").ToString())
            lvi.SubItems.Add(row("BuyerName").ToString())
            lvi.SubItems.Add(row("TraderName").ToString())
            lvi.SubItems.Add(Convert.ToDecimal(row("TotalAmount")).ToString("F0"))
            lvi.SubItems.Add(row("Status").ToString())

            Select Case row("Status").ToString()
                Case "Pending"
                    lvi.ForeColor = modTheme.cWarning
                Case "Confirmed"
                    lvi.ForeColor = modTheme.cOcean
                Case "Completed"
                    lvi.ForeColor = modTheme.cSuccess
                Case "Rejected", "Cancelled"
                    lvi.ForeColor = modTheme.cDanger
            End Select

            lvOrders.Items.Add(lvi)
        Next
    End Sub
End Class