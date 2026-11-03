Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucOrders
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderID As Integer

    Private lvOrders As New ListView()
    Private btnRefresh As New Button()

    Private Sub ucOrders_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill

        BuildHeader()
        BuildTable()
        LoadOrders()
    End Sub

    Private Sub BuildHeader()
        Dim lblTitle As New Label()
        lblTitle.Text = "Orders"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim lblSub As New Label()
        lblSub.Text = "Manage incoming and ongoing orders."
        lblSub.Font = modTheme.fBody
        lblSub.ForeColor = modTheme.cMuted
        lblSub.Location = New Point(2, 45)
        lblSub.AutoSize = True
        Me.Controls.Add(lblSub)

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
        btnRefresh.Cursor = Cursors.Hand
        AddHandler btnRefresh.Click, Sub(s, ev) LoadOrders()
        Me.Controls.Add(btnRefresh)
    End Sub

    Private Sub BuildTable()
        lvOrders.View = View.Details
        lvOrders.FullRowSelect = True
        lvOrders.GridLines = False
        lvOrders.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvOrders.Location = New Point(0, 90)
        lvOrders.Size = New Size(Me.Width, Me.Height - 100)
        lvOrders.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvOrders.BackColor = modTheme.cWhite
        lvOrders.BorderStyle = BorderStyle.FixedSingle

        lvOrders.Columns.Add("Order No", 120)
        lvOrders.Columns.Add("Buyer", 150)
        lvOrders.Columns.Add("Item", 150)
        lvOrders.Columns.Add("Qty", 70)
        lvOrders.Columns.Add("Total (UGX)", 100)
        lvOrders.Columns.Add("Status", 100)
        lvOrders.Columns.Add("Date", 120)

        Me.Controls.Add(lvOrders)
    End Sub

    Private Async Sub LoadOrders()
        If String.IsNullOrEmpty(Token) Then Return

        Try
            Dim msg As String = $"ORDER_LIST|8|{Token}|{TraderID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(16384) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            lvOrders.Items.Clear()

            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 8 AndAlso p(0) = "ORDER" Then
                    Dim lvi As New ListViewItem(p(2)) ' OrderNo
                    lvi.SubItems.Add(p(3))            ' Buyer
                    lvi.SubItems.Add(p(4))            ' Item
                    lvi.SubItems.Add(p(5))            ' Qty
                    lvi.SubItems.Add(p(6))            ' Total
                    lvi.SubItems.Add(p(7))            ' Status
                    lvi.SubItems.Add(p(8))            ' Date

                    ' Color-code by status
                    Select Case p(7)
                        Case "Pending"
                            lvi.ForeColor = modTheme.cWarning
                        Case "Confirmed"
                            lvi.ForeColor = modTheme.cOcean
                        Case "Completed"
                            lvi.ForeColor = modTheme.cSuccess
                        Case "Rejected", "Cancelled"
                            lvi.ForeColor = modTheme.cDanger
                    End Select

                    lvi.Tag = p(1) ' Store OrderID
                    lvOrders.Items.Add(lvi)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Error loading orders: " & ex.Message)
        End Try
    End Sub
End Class
