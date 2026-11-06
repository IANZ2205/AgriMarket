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
    Private btnConfirm As New Button()
    Private btnReject As New Button()
    Private btnComplete As New Button()

    Private Sub ucOrders_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill

        BuildHeader()
        BuildTable()
        BuildActionButtons()
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
        lvOrders.Size = New Size(Me.Width, Me.Height - 180)
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

    Private Sub BuildActionButtons()
        btnConfirm.Text = "Confirm Order"
        btnConfirm.BackColor = modTheme.cOcean
        btnConfirm.ForeColor = modTheme.cWhite
        btnConfirm.FlatStyle = FlatStyle.Flat
        btnConfirm.FlatAppearance.BorderSize = 0
        btnConfirm.Location = New Point(0, Me.Height - 70)
        btnConfirm.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnConfirm.Size = New Size(150, 45)
        btnConfirm.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnConfirm.Click, AddressOf btnConfirm_Click
        Me.Controls.Add(btnConfirm)

        btnReject.Text = "Reject Order"
        btnReject.BackColor = modTheme.cWhite
        btnReject.ForeColor = modTheme.cDanger
        btnReject.FlatStyle = FlatStyle.Flat
        btnReject.FlatAppearance.BorderColor = modTheme.cDanger
        btnReject.FlatAppearance.BorderSize = 1
        btnReject.Location = New Point(170, Me.Height - 70)
        btnReject.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnReject.Size = New Size(150, 45)
        btnReject.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnReject.Click, AddressOf btnReject_Click
        Me.Controls.Add(btnReject)

        btnComplete.Text = "Mark Completed"
        btnComplete.BackColor = modTheme.cSuccess
        btnComplete.ForeColor = modTheme.cWhite
        btnComplete.FlatStyle = FlatStyle.Flat
        btnComplete.FlatAppearance.BorderSize = 0
        btnComplete.Location = New Point(340, Me.Height - 70)
        btnComplete.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnComplete.Size = New Size(160, 45)
        btnComplete.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnComplete.Click, AddressOf btnComplete_Click
        Me.Controls.Add(btnComplete)
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
                If p.Length >= 9 AndAlso p(0) = "ORDER" Then
                    Dim lvi As New ListViewItem(p(2)) ' OrderNo
                    lvi.SubItems.Add(p(3))            ' Buyer
                    lvi.SubItems.Add(p(4))            ' Item
                    lvi.SubItems.Add(p(5))            ' Qty
                    lvi.SubItems.Add(p(6))            ' Total
                    lvi.SubItems.Add(p(7))            ' Status
                    lvi.SubItems.Add(p(8))            ' Date

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

    Private Sub btnConfirm_Click(sender As Object, e As EventArgs)
        If lvOrders.SelectedItems.Count = 0 Then Return
        Dim orderId As Integer = Integer.Parse(lvOrders.SelectedItems(0).Tag.ToString())
        ConfirmOrder(orderId)
    End Sub

    Private Async Sub ConfirmOrder(orderId As Integer)
        Try
            Dim msg As String = $"ORDER_CONFIRM|9|{Token}|{orderId}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Order Confirmed!", "Success")
                LoadOrders()
            Else
                MessageBox.Show("Error: " & parts(3), "Error")
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub

    Private Sub btnReject_Click(sender As Object, e As EventArgs)
        If lvOrders.SelectedItems.Count = 0 Then Return
        Dim orderId As Integer = Integer.Parse(lvOrders.SelectedItems(0).Tag.ToString())
        Dim reason As String = InputBox("Enter rejection reason:", "Reject Order")
        If Not String.IsNullOrEmpty(reason) Then RejectOrder(orderId, reason)
    End Sub

    Private Async Sub RejectOrder(orderId As Integer, reason As String)
        Try
            Dim msg As String = $"ORDER_REJECT|10|{Token}|{orderId}|{reason}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Order Rejected!", "Success")
                LoadOrders()
            Else
                MessageBox.Show("Error: " & parts(3), "Error")
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub

    Private Sub btnComplete_Click(sender As Object, e As EventArgs)
        If lvOrders.SelectedItems.Count = 0 Then Return
        Dim orderId As Integer = Integer.Parse(lvOrders.SelectedItems(0).Tag.ToString())
        CompleteOrder(orderId)
    End Sub

    Private Async Sub CompleteOrder(orderId As Integer)
        Try
            Dim msg As String = $"ORDER_COMPLETE|11|{Token}|{orderId}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Order Completed!", "Success")
                LoadOrders()
            Else
                MessageBox.Show("Error: " & parts(3), "Error")
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub
End Class