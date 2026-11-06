Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucBuyerOrders
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property BuyerID As Integer

    Private lvOrders As New ListView()
    Private btnCancel As New Button()
    Private btnRefresh As New Button()
    Private lblTotalSales As New Label()

    Private Sub ucBuyerOrders_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill

        BuildHeader()
        BuildTable()
        BuildActionButtons()
        LoadOrders()
    End Sub

    Private Sub BuildHeader()
        Dim lblTitle As New Label()
        lblTitle.Text = "My Orders"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim lblSub As New Label()
        lblSub.Text = "Track your orders and their status."
        lblSub.Font = modTheme.fBody
        lblSub.ForeColor = modTheme.cMuted
        lblSub.Location = New Point(2, 45)
        lblSub.AutoSize = True
        Me.Controls.Add(lblSub)

        lblTotalSales.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblTotalSales.ForeColor = modTheme.cOcean
        lblTotalSales.Location = New Point(Me.Width - 250, 20)
        lblTotalSales.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblTotalSales.AutoSize = True
        lblTotalSales.Text = "Total: UGX 0"
        Me.Controls.Add(lblTotalSales)

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
    End Sub

    Private Sub BuildTable()
        lvOrders.View = View.Details
        lvOrders.FullRowSelect = True
        lvOrders.GridLines = False
        lvOrders.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvOrders.Location = New Point(0, 90)
        lvOrders.Size = New Size(Me.Width, Me.Height - 180)
        lvOrders.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvOrders.BackColor = modTheme.cWhite
        lvOrders.BorderStyle = BorderStyle.FixedSingle

        lvOrders.Columns.Add("Order No", 120)
        lvOrders.Columns.Add("Item", 150)
        lvOrders.Columns.Add("Qty", 70)
        lvOrders.Columns.Add("Total (UGX)", 100)
        lvOrders.Columns.Add("Trader", 150)
        lvOrders.Columns.Add("Status", 100)
        lvOrders.Columns.Add("Date", 120)

        Me.Controls.Add(lvOrders)
    End Sub

    Private Sub BuildActionButtons()
        btnCancel.Text = "Cancel Order"
        btnCancel.BackColor = modTheme.cDanger
        btnCancel.ForeColor = modTheme.cWhite
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.FlatAppearance.BorderSize = 0
        btnCancel.Location = New Point(0, Me.Height - 70)
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnCancel.Size = New Size(150, 45)
        btnCancel.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnCancel.Click, AddressOf btnCancel_Click
        Me.Controls.Add(btnCancel)
    End Sub

    Private Async Sub LoadOrders()
        If String.IsNullOrEmpty(Token) Then Return
        Try
            Dim msg As String = $"BUYER_ORDERS|20|{Token}|{BuyerID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(16384) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            lvOrders.Items.Clear()
            Dim total As Decimal = 0

            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 11 AndAlso p(0) = "BORDER" Then
                    Dim lvi As New ListViewItem(p(2)) ' OrderNo
                    lvi.SubItems.Add(p(3))            ' Item
                    lvi.SubItems.Add(p(4))            ' Qty
                    lvi.SubItems.Add(p(6))            ' Total
                    lvi.SubItems.Add(p(8))            ' Trader
                    lvi.SubItems.Add(p(7))            ' Status
                    lvi.SubItems.Add(p(9))            ' Date

                    Dim status As String = p(7)
                    Select Case status
                        Case "Pending"
                            lvi.ForeColor = modTheme.cWarning
                        Case "Confirmed"
                            lvi.ForeColor = modTheme.cOcean
                        Case "Completed"
                            lvi.ForeColor = modTheme.cSuccess
                        Case "Rejected", "Cancelled"
                            lvi.ForeColor = modTheme.cDanger
                    End Select

                    If status = "Completed" Then
                        total += Decimal.Parse(p(6))
                    End If

                    lvi.Tag = p(1) ' Store OrderID
                    lvOrders.Items.Add(lvi)
                End If
            Next

            lblTotalSales.Text = "Total: UGX " & total.ToString("N0")
        Catch ex As Exception
            MessageBox.Show("Error loading orders: " & ex.Message)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs)
        If lvOrders.SelectedItems.Count = 0 Then
            MessageBox.Show("Select an order to cancel.")
            Return
        End If
        Dim status As String = lvOrders.SelectedItems(0).SubItems(5).Text
        If status <> "Pending" AndAlso status <> "Confirmed" Then
            MessageBox.Show("Only Pending or Confirmed orders can be cancelled.")
            Return
        End If
        Dim orderId As Integer = Integer.Parse(lvOrders.SelectedItems(0).Tag.ToString())
        If MessageBox.Show("Cancel this order?", "Confirm", MessageBoxButtons.YesNo) = DialogResult.Yes Then
            CancelOrder(orderId)
        End If
    End Sub

    Private Async Sub CancelOrder(orderId As Integer)
        Try
            Dim msg As String = $"ORDER_CANCEL|21|{Token}|{orderId}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(512) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim p() As String = reply.Split("|"c)

            If p(0) = "OK" Then
                MessageBox.Show("Order cancelled.", "Success")
                LoadOrders()
            Else
                MessageBox.Show("Error: " & p(3))
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub
End Class