Imports System.Drawing.Printing
Imports System.IO
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucAdminReports
    Private dtpFrom As New DateTimePicker()
    Private dtpTo As New DateTimePicker()
    Private cmbReportType As New ComboBox()
    Private btnGenerate As New Button()
    Private lvReport As New ListView()
    Private btnPrint As New Button()
    Private lblSummary As New Label()

    Private currentReport As String = "Sales"
    Private previewText As String = ""
    Private printLines As New List(Of String)()

    Private Sub ucAdminReports_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        Dim lblTitle As New Label()
        lblTitle.Text = "Reports"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim lblFrom As New Label()
        lblFrom.Text = "From:"
        lblFrom.Location = New Point(0, 60)
        lblFrom.AutoSize = True
        Me.Controls.Add(lblFrom)

        dtpFrom.Location = New Point(0, 80)
        dtpFrom.Size = New Size(150, 25)
        dtpFrom.Value = DateTime.Now.AddMonths(-1)
        Me.Controls.Add(dtpFrom)

        Dim lblTo As New Label()
        lblTo.Text = "To:"
        lblTo.Location = New Point(170, 60)
        lblTo.AutoSize = True
        Me.Controls.Add(lblTo)

        dtpTo.Location = New Point(170, 80)
        dtpTo.Size = New Size(150, 25)
        Me.Controls.Add(dtpTo)

        Dim lblType As New Label()
        lblType.Text = "Report Type:"
        lblType.Location = New Point(340, 60)
        lblType.AutoSize = True
        Me.Controls.Add(lblType)

        cmbReportType.Items.AddRange(New Object() {"Sales", "Stock", "Users"})
        cmbReportType.Location = New Point(340, 80)
        cmbReportType.Size = New Size(150, 25)
        cmbReportType.DropDownStyle = ComboBoxStyle.DropDownList
        cmbReportType.SelectedIndex = 0
        Me.Controls.Add(cmbReportType)

        btnGenerate.Text = "Generate Report"
        btnGenerate.BackColor = modTheme.cOcean
        btnGenerate.ForeColor = modTheme.cWhite
        btnGenerate.FlatStyle = FlatStyle.Flat
        btnGenerate.FlatAppearance.BorderSize = 0
        btnGenerate.Location = New Point(520, 75)
        btnGenerate.Size = New Size(160, 35)
        btnGenerate.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnGenerate.Click, AddressOf btnGenerate_Click
        Me.Controls.Add(btnGenerate)

        btnPrint.Text = "Print Report"
        btnPrint.BackColor = modTheme.cWhite
        btnPrint.ForeColor = modTheme.cOcean
        btnPrint.FlatStyle = FlatStyle.Flat
        btnPrint.FlatAppearance.BorderColor = modTheme.cOcean
        btnPrint.FlatAppearance.BorderSize = 1
        btnPrint.Location = New Point(690, 75)
        btnPrint.Size = New Size(150, 35)
        btnPrint.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnPrint.Click, AddressOf btnPrint_Click
        Me.Controls.Add(btnPrint)

        lblSummary.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        lblSummary.ForeColor = modTheme.cOcean
        lblSummary.Location = New Point(0, 130)
        lblSummary.AutoSize = True
        lblSummary.Text = "No report generated yet."
        Me.Controls.Add(lblSummary)

        lvReport.View = View.Details
        lvReport.FullRowSelect = True
        lvReport.GridLines = False
        lvReport.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvReport.Location = New Point(0, 160)
        lvReport.Size = New Size(Me.Width, Me.Height - 170)
        lvReport.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvReport.BackColor = modTheme.cWhite
        lvReport.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(lvReport)
    End Sub

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs)
        lvReport.Clear()
        currentReport = cmbReportType.SelectedItem.ToString()
        printLines.Clear()

        Select Case currentReport
            Case "Sales"
                GenerateSalesReport()
            Case "Stock"
                GenerateStockReport()
            Case "Users"
                GenerateUsersReport()
        End Select
    End Sub

    Private Sub GenerateSalesReport()
        lvReport.Columns.Clear()
        lvReport.Columns.Add("Order No", 120)
        lvReport.Columns.Add("Date", 120)
        lvReport.Columns.Add("Buyer", 140)
        lvReport.Columns.Add("Item", 140)
        lvReport.Columns.Add("Qty", 80)
        lvReport.Columns.Add("Unit Price (UGX)", 120)
        lvReport.Columns.Add("Total (UGX)", 120)

        Dim fromDate As String = dtpFrom.Value.ToString("yyyy-MM-dd")
        Dim toDate As String = dtpTo.Value.ToString("yyyy-MM-dd")
        Dim dt As DataTable = modDB.GetSalesReport(fromDate, toDate)

        Dim grandTotal As Decimal = 0
        For Each row As DataRow In dt.Rows
            Dim lvi As New ListViewItem(row("OrderNo").ToString())
            lvi.SubItems.Add(row("CreatedAt").ToString())
            lvi.SubItems.Add(row("BuyerName").ToString())
            lvi.SubItems.Add(row("ItemName").ToString())
            lvi.SubItems.Add(Convert.ToDouble(row("Qty")).ToString("F2"))
            lvi.SubItems.Add(Convert.ToDecimal(row("UnitPriceAtOrder")).ToString("F0"))
            lvi.SubItems.Add(Convert.ToDecimal(row("TotalAmount")).ToString("F0"))
            lvReport.Items.Add(lvi)
            grandTotal += Convert.ToDecimal(row("TotalAmount"))
        Next

        lblSummary.Text = $"Sales Report ({fromDate} to {toDate}) — {dt.Rows.Count} orders, Total: UGX {grandTotal.ToString("N0")}"
        previewText = lblSummary.Text
        printLines.Add("AgriMarket Sales Report")
        printLines.Add($"Period: {fromDate} to {toDate}")
        printLines.Add($"Total Orders: {dt.Rows.Count}")
        printLines.Add($"Grand Total: UGX {grandTotal.ToString("N0")}")
        printLines.Add("")
        For Each row As DataRow In dt.Rows
            printLines.Add($"{row("OrderNo")} | {row("CreatedAt")} | {row("BuyerName")} | {row("ItemName")} | Qty: {row("Qty")} | UGX {row("TotalAmount")}")
        Next
    End Sub

    Private Sub GenerateStockReport()
        lvReport.Columns.Clear()
        lvReport.Columns.Add("Item", 180)
        lvReport.Columns.Add("Category", 120)
        lvReport.Columns.Add("Trader", 160)
        lvReport.Columns.Add("Available", 100)
        lvReport.Columns.Add("Unit", 80)
        lvReport.Columns.Add("Price (UGX)", 120)
        lvReport.Columns.Add("Active Batches", 100)

        Dim dt As DataTable = modDB.GetStockReport()
        For Each row As DataRow In dt.Rows
            Dim lvi As New ListViewItem(row("EnglishName").ToString())
            lvi.SubItems.Add(row("Category").ToString())
            lvi.SubItems.Add(row("TraderName").ToString())
            lvi.SubItems.Add(Convert.ToDouble(row("AvailableQty")).ToString("F2"))
            lvi.SubItems.Add(row("Unit").ToString())
            lvi.SubItems.Add(Convert.ToDecimal(row("UnitPrice")).ToString("F0"))
            lvi.SubItems.Add(row("ActiveBatches").ToString())
            lvReport.Items.Add(lvi)
        Next

        lblSummary.Text = $"Stock Report — {dt.Rows.Count} active items"
        printLines.Clear()
        printLines.Add("AgriMarket Stock Report")
        printLines.Add($"Items: {dt.Rows.Count}")
        printLines.Add("")
        For Each row As DataRow In dt.Rows
            printLines.Add($"{row("EnglishName")} | {row("Category")} | {row("TraderName")} | Available: {row("AvailableQty")} {row("Unit")} | UGX {row("UnitPrice")}")
        Next
    End Sub

    Private Sub GenerateUsersReport()
        lvReport.Columns.Clear()
        lvReport.Columns.Add("Name", 180)
        lvReport.Columns.Add("Email", 200)
        lvReport.Columns.Add("Role", 100)
        lvReport.Columns.Add("Status", 100)
        lvReport.Columns.Add("Phone", 130)
        lvReport.Columns.Add("Joined", 120)

        Dim dt As DataTable = modDB.GetUsersReport()
        For Each row As DataRow In dt.Rows
            Dim lvi As New ListViewItem(row("DisplayName").ToString())
            lvi.SubItems.Add(row("Email").ToString())
            lvi.SubItems.Add(row("Role").ToString())
            lvi.SubItems.Add(row("Status").ToString())
            lvi.SubItems.Add(row("Phone").ToString())
            lvi.SubItems.Add(row("CreatedAt").ToString())
            lvReport.Items.Add(lvi)
        Next

        lblSummary.Text = $"Users Report — {dt.Rows.Count} total users"
        printLines.Clear()
        printLines.Add("AgriMarket Users Report")
        printLines.Add($"Total Users: {dt.Rows.Count}")
        printLines.Add("")
        For Each row As DataRow In dt.Rows
            printLines.Add($"{row("DisplayName")} | {row("Email")} | {row("Role")} | {row("Status")}")
        Next
    End Sub

    Private Sub btnPrint_Click(sender As Object, e As EventArgs)
        If printLines.Count = 0 Then
            MessageBox.Show("Generate a report first.")
            Return
        End If

        Dim pd As New PrintDocument()
        Dim linesToPrint As New List(Of String)(printLines)
        AddHandler pd.PrintPage, Sub(s, args)
                                     Dim y As Integer = 100
                                     Dim font As New Font("Segoe UI", 10, FontStyle.Regular)
                                     Dim titleFont As New Font("Segoe UI", 16, FontStyle.Bold)

                                     ' Print header
                                     args.Graphics.DrawString("AgriMarket", titleFont, Brushes.Black, 100, 50)
                                     args.Graphics.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm"), font, Brushes.Gray, 100, 75)

                                     ' Print lines
                                     For Each line In linesToPrint
                                         If y > args.MarginBounds.Bottom - 50 Then
                                             args.HasMorePages = True
                                             Return
                                         End If
                                         args.Graphics.DrawString(line, font, Brushes.Black, 100, y)
                                         y += 20
                                     Next
                                 End Sub

        Dim pdDialog As New PrintDialog()
        pdDialog.Document = pd
        If pdDialog.ShowDialog() = DialogResult.OK Then
            pd.Print()
        End If
    End Sub
End Class