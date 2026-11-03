Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucMyStock
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    Private lvStock As New ListView()
    Private lvBatches As New ListView()
    Private btnAddItem As New Button()
    Private btnTopUp As New Button()
    Private lblBatchesTitle As New Label()

    Private Sub ucMyStock_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildHeader()
        BuildStockTable()
        BuildBatchPanel()
        LoadStock()
    End Sub

    Private Sub BuildHeader()
        Dim lblTitle As New Label()
        lblTitle.Text = "My Stock"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim lblSub As New Label()
        lblSub.Text = "Manage your produce and track expiry dates."
        lblSub.Font = modTheme.fBody
        lblSub.ForeColor = modTheme.cMuted
        lblSub.Location = New Point(2, 45)
        lblSub.AutoSize = True
        Me.Controls.Add(lblSub)

        btnAddItem.Text = "+ Add Item"
        btnAddItem.BackColor = modTheme.cOcean
        btnAddItem.ForeColor = modTheme.cWhite
        btnAddItem.FlatStyle = FlatStyle.Flat
        btnAddItem.FlatAppearance.BorderSize = 0
        btnAddItem.Location = New Point(Me.Width - 150, 20)
        btnAddItem.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnAddItem.Size = New Size(130, 40)
        btnAddItem.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnAddItem.Cursor = Cursors.Hand
        AddHandler btnAddItem.Click, Sub(s, ev) OpenAddItem()
        Me.Controls.Add(btnAddItem)

        btnTopUp.Text = "Top Up"
        btnTopUp.BackColor = modTheme.cWhite
        btnTopUp.ForeColor = modTheme.cOcean
        btnTopUp.FlatStyle = FlatStyle.Flat
        btnTopUp.FlatAppearance.BorderColor = modTheme.cOcean
        btnTopUp.FlatAppearance.BorderSize = 1
        btnTopUp.Location = New Point(Me.Width - 300, 20)
        btnTopUp.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnTopUp.Size = New Size(130, 40)
        btnTopUp.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnTopUp.Cursor = Cursors.Hand
        AddHandler btnTopUp.Click, AddressOf btnTopUp_Click
        Me.Controls.Add(btnTopUp)


        Dim btnEdit As New Button()
        btnEdit.Text = "Edit Item"
        btnEdit.BackColor = modTheme.cWhite
        btnEdit.ForeColor = modTheme.cOcean
        btnEdit.FlatStyle = FlatStyle.Flat
        btnEdit.FlatAppearance.BorderColor = modTheme.cOcean
        btnEdit.FlatAppearance.BorderSize = 1
        btnEdit.Location = New Point(Me.Width - 450, 20)
        btnEdit.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnEdit.Size = New Size(130, 40)
        btnEdit.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnEdit.Cursor = Cursors.Hand
        AddHandler btnEdit.Click, AddressOf btnEdit_Click
        Me.Controls.Add(btnEdit)
    End Sub

    Private Sub BuildStockTable()
        lvStock.View = View.Details
        lvStock.FullRowSelect = True
        lvStock.GridLines = False
        lvStock.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvStock.Location = New Point(0, 90)
        lvStock.Size = New Size(Me.Width, 260)
        lvStock.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lvStock.BackColor = modTheme.cWhite
        lvStock.BorderStyle = BorderStyle.FixedSingle

        lvStock.Columns.Add("English Name", 180)
        lvStock.Columns.Add("Category", 130)
        lvStock.Columns.Add("Available Qty", 110)
        lvStock.Columns.Add("Unit", 70)
        lvStock.Columns.Add("Price (UGX)", 100)
        lvStock.Columns.Add("Status", 100)

        AddHandler lvStock.SelectedIndexChanged, AddressOf lvStock_SelectedIndexChanged
        Me.Controls.Add(lvStock)
    End Sub

    Private Sub BuildBatchPanel()
        lblBatchesTitle.Text = "Batches for selected item"
        lblBatchesTitle.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblBatchesTitle.ForeColor = modTheme.cOcean
        lblBatchesTitle.Location = New Point(0, 370)
        lblBatchesTitle.AutoSize = True
        lblBatchesTitle.Visible = False
        Me.Controls.Add(lblBatchesTitle)

        lvBatches.View = View.Details
        lvBatches.FullRowSelect = True
        lvBatches.GridLines = False
        lvBatches.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvBatches.Location = New Point(0, 400)
        lvBatches.Size = New Size(Me.Width, 150)
        lvBatches.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvBatches.BackColor = modTheme.cWhite
        lvBatches.BorderStyle = BorderStyle.FixedSingle
        lvBatches.Visible = False

        lvBatches.Columns.Add("Batch No", 140)
        lvBatches.Columns.Add("Date Added", 110)
        lvBatches.Columns.Add("Expiry Date", 110)
        lvBatches.Columns.Add("Days Left", 80)
        lvBatches.Columns.Add("Remaining", 80)
        lvBatches.Columns.Add("Reserved", 80)
        lvBatches.Columns.Add("Status", 90)

        Me.Controls.Add(lvBatches)

        Dim btnAdjust As New Button()
        btnAdjust.Text = "Adjust Selected Batch"
        btnAdjust.BackColor = modTheme.cWhite
        btnAdjust.ForeColor = modTheme.cOcean
        btnAdjust.FlatStyle = FlatStyle.Flat
        btnAdjust.FlatAppearance.BorderColor = modTheme.cOcean
        btnAdjust.FlatAppearance.BorderSize = 1
        btnAdjust.Location = New Point(0, 555)
        btnAdjust.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        btnAdjust.Size = New Size(200, 35)
        btnAdjust.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        btnAdjust.Cursor = Cursors.Hand
        AddHandler btnAdjust.Click, AddressOf btnAdjust_Click
        Me.Controls.Add(btnAdjust)
    End Sub

    Private Sub OpenAddItem()
        Dim frm As New frmAddItem()
        frm.TraderID = TraderID
        frm.Token = Token
        If frm.ShowDialog() = DialogResult.OK Then
            LoadStock()
        End If
    End Sub

    Private Sub btnTopUp_Click(sender As Object, e As EventArgs)
        If lvStock.SelectedItems.Count = 0 Then
            MessageBox.Show("Select an item from the table first.", "No Selection")
            Return
        End If
        Dim itemId As Integer = Integer.Parse(lvStock.SelectedItems(0).Tag.ToString())
        Dim frm As New frmTopUp()
        frm.ItemID = itemId
        frm.Token = Token
        If frm.ShowDialog() = DialogResult.OK Then
            LoadStock()
        End If
    End Sub

    Private Async Sub LoadStock()
        If String.IsNullOrEmpty(Token) Then Return
        Try
            Dim msg As String = $"STOCK_LIST|4|{Token}|{TraderID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(16384) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            lvStock.Items.Clear()

            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 8 AndAlso p(0) = "ITEM" Then
                    Dim lvi As New ListViewItem(p(2))
                    lvi.SubItems.Add(p(3))
                    lvi.SubItems.Add(p(4))
                    lvi.SubItems.Add(p(5))
                    lvi.SubItems.Add(p(6))

                    Dim qty As Double = Double.Parse(p(4))
                    If qty > 0 Then
                        lvi.SubItems.Add("Active")
                        lvi.ForeColor = modTheme.cSuccess
                    Else
                        lvi.SubItems.Add("Out of Stock")
                        lvi.ForeColor = modTheme.cDanger
                    End If
                    lvi.Tag = p(1)
                    lvStock.Items.Add(lvi)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Error loading stock: " & ex.Message)
        End Try
    End Sub

    Private Async Sub lvStock_SelectedIndexChanged(sender As Object, e As EventArgs)
        If lvStock.SelectedItems.Count = 0 Then
            lblBatchesTitle.Visible = False
            lvBatches.Visible = False
            Return
        End If

        Dim selectedItem As ListViewItem = lvStock.SelectedItems(0)
        Dim itemId As String = selectedItem.Tag.ToString()
        If String.IsNullOrEmpty(itemId) Then Return

        lblBatchesTitle.Visible = True
        lvBatches.Visible = True
        lvBatches.Items.Clear()

        Try
            Dim msg As String = $"BATCH_LIST|5|{Token}|{itemId}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(8192) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 9 AndAlso p(0) = "BATCH" Then
                    Dim lvi As New ListViewItem(p(2)) ' BatchNo
                    lvi.SubItems.Add(p(3))            ' DateAdded
                    lvi.SubItems.Add(p(4))            ' ExpiryDate
                    lvi.SubItems.Add(p(8))            ' DaysLeft
                    lvi.SubItems.Add(p(5))            ' Remaining
                    lvi.SubItems.Add(p(6))            ' Reserved
                    lvi.SubItems.Add(p(7))            ' Status
                    lvi.Tag = p(1)                    ' BatchID
                    lvBatches.Items.Add(lvi)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Error loading batches: " & ex.Message)
        End Try
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs)
        If lvStock.SelectedItems.Count = 0 Then
            MessageBox.Show("Select an item first.", "No Selection")
            Return
        End If
        ' For now, we'll pass placeholder values; the server uses them to update
        Dim frm As New frmEditItem()
        frm.ItemID = Integer.Parse(lvStock.SelectedItems(0).Tag.ToString())
        frm.Token = Token
        frm.CurrentName = lvStock.SelectedItems(0).SubItems(0).Text
        frm.CurrentDesc = ""
        frm.CurrentPrice = 0
        frm.CurrentLowStock = 10
        If frm.ShowDialog() = DialogResult.OK Then
            LoadStock()
        End If
    End Sub

    Private Sub btnAdjust_Click(sender As Object, e As EventArgs)
        If lvBatches.SelectedItems.Count = 0 Then
            MessageBox.Show("Select a batch from the batches list first.", "No Selection")
            Return
        End If
        Dim batchId As Integer = Integer.Parse(lvBatches.SelectedItems(0).Tag.ToString())
        Dim currentQty As Double = Double.Parse(lvBatches.SelectedItems(0).SubItems(4).Text)
        Dim currentShelf As Integer = 0 ' We don't have shelf life in the batch list yet; default
        Dim frm As New frmAdjustBatch()
        frm.BatchID = batchId
        frm.Token = Token
        frm.CurrentQty = currentQty
        frm.CurrentShelfLife = currentShelf
        If frm.ShowDialog() = DialogResult.OK Then
            LoadStock()
        End If
    End Sub


End Class