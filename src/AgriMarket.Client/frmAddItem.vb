Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmAddItem
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    Private cmbCategory As New ComboBox()
    Private txtItemCode As New TextBox()
    Private txtName As New TextBox()
    Private txtDesc As New TextBox()
    Private txtPrice As New TextBox()
    Private cmbUnit As New ComboBox()
    Private txtLowStock As New TextBox()
    Private txtQty As New TextBox()
    Private txtExpiry As New TextBox()
    Private txtLuganda As New TextBox()
    Private txtRunyankole As New TextBox()
    Private txtLusoga As New TextBox()
    Private txtAcholi As New TextBox()

    Private Sub frmAddItem_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Add Item"
        Me.ClientSize = New Size(750, 650)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        Dim lblTitle As New Label()
        lblTitle.Text = "Add New Item"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        ' Row 1
        AddLabel("Category", 20, 80)
        cmbCategory.Items.AddRange(New Object() {"Food crop", "Cash crop", "Livestock", "Poultry", "Fish", "Dairy and eggs", "Other"})
        cmbCategory.Location = New Point(20, 100)
        cmbCategory.Size = New Size(180, 25)
        cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCategory.SelectedIndex = 0
        Me.Controls.Add(cmbCategory)

        AddLabel("Item Code", 220, 80)
        txtItemCode.Location = New Point(220, 100)
        txtItemCode.Size = New Size(120, 25)
        txtItemCode.MaxLength = 5
        Me.Controls.Add(txtItemCode)

        AddLabel("English Name *", 360, 80)
        txtName.Location = New Point(360, 100)
        txtName.Size = New Size(350, 25)
        Me.Controls.Add(txtName)

        ' Row 2
        AddLabel("Quantity *", 20, 150)
        txtQty.Location = New Point(20, 170)
        txtQty.Size = New Size(100, 25)
        Me.Controls.Add(txtQty)

        AddLabel("Unit", 140, 150)
        cmbUnit.Items.AddRange(New Object() {"kg", "g", "tonne", "bag", "sack", "crate", "basket", "bunch", "tin", "litre", "dozen", "tray", "head", "bird", "piece"})
        cmbUnit.Location = New Point(140, 170)
        cmbUnit.Size = New Size(100, 25)
        cmbUnit.DropDownStyle = ComboBoxStyle.DropDownList
        cmbUnit.SelectedIndex = 0
        Me.Controls.Add(cmbUnit)

        AddLabel("Price per unit (UGX) *", 260, 150)
        txtPrice.Location = New Point(260, 170)
        txtPrice.Size = New Size(150, 25)
        Me.Controls.Add(txtPrice)

        AddLabel("Low Stock Alert Level", 430, 150)
        txtLowStock.Location = New Point(430, 170)
        txtLowStock.Size = New Size(100, 25)
        txtLowStock.Text = "10"
        Me.Controls.Add(txtLowStock)

        ' Row 3
        AddLabel("Days until it goes bad *", 20, 220)
        txtExpiry.Location = New Point(20, 240)
        txtExpiry.Size = New Size(100, 25)
        Me.Controls.Add(txtExpiry)

        AddLabel("Description", 140, 220)
        txtDesc.Location = New Point(140, 240)
        txtDesc.Size = New Size(570, 80)
        txtDesc.Multiline = True
        Me.Controls.Add(txtDesc)

        ' Row 4 - Local Names
        Dim lblLocalTitle As New Label()
        lblLocalTitle.Text = "Local Names (Optional)"
        lblLocalTitle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblLocalTitle.ForeColor = modTheme.cOcean
        lblLocalTitle.Location = New Point(20, 340)
        lblLocalTitle.AutoSize = True
        Me.Controls.Add(lblLocalTitle)

        AddLabel("Luganda", 20, 370)
        txtLuganda.Location = New Point(20, 390)
        txtLuganda.Size = New Size(160, 25)
        Me.Controls.Add(txtLuganda)

        AddLabel("Runyankole / Rukiga", 200, 370)
        txtRunyankole.Location = New Point(200, 390)
        txtRunyankole.Size = New Size(160, 25)
        Me.Controls.Add(txtRunyankole)

        AddLabel("Lusoga", 380, 370)
        txtLusoga.Location = New Point(380, 390)
        txtLusoga.Size = New Size(160, 25)
        Me.Controls.Add(txtLusoga)

        AddLabel("Acholi", 560, 370)
        txtAcholi.Location = New Point(560, 390)
        txtAcholi.Size = New Size(160, 25)
        Me.Controls.Add(txtAcholi)

        ' Buttons
        Dim btnSave As New Button()
        btnSave.Text = "Save Item"
        btnSave.BackColor = modTheme.cOcean
        btnSave.ForeColor = modTheme.cWhite
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.FlatAppearance.BorderSize = 0
        btnSave.Location = New Point(20, 480)
        btnSave.Size = New Size(150, 45)
        btnSave.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnSave.Click, AddressOf btnSave_Click
        Me.Controls.Add(btnSave)

        Dim btnCancel As New Button()
        btnCancel.Text = "Cancel"
        btnCancel.BackColor = modTheme.cWhite
        btnCancel.ForeColor = modTheme.cOcean
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.FlatAppearance.BorderColor = modTheme.cOcean
        btnCancel.FlatAppearance.BorderSize = 1
        btnCancel.Location = New Point(180, 480)
        btnCancel.Size = New Size(150, 45)
        btnCancel.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnCancel.Click, AddressOf btnCancel_Click
        Me.Controls.Add(btnCancel)
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Location = New Point(x, y)
        lbl.AutoSize = True
        lbl.ForeColor = modTheme.cText
        Me.Controls.Add(lbl)
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(txtName.Text) OrElse String.IsNullOrEmpty(txtItemCode.Text) OrElse String.IsNullOrEmpty(txtPrice.Text) OrElse String.IsNullOrEmpty(txtQty.Text) OrElse String.IsNullOrEmpty(txtExpiry.Text) Then
            MessageBox.Show("Please fill in all required (*) fields.", "Validation Error")
            Return
        End If

        Try
            Dim price As Decimal = Decimal.Parse(txtPrice.Text)
            Dim qty As Double = Double.Parse(txtQty.Text)
            Dim shelf As Integer = Integer.Parse(txtExpiry.Text)
            Dim lowStock As Integer = Integer.Parse(txtLowStock.Text)

            Dim name As String = txtName.Text.Replace("|", "\|")
            Dim desc As String = txtDesc.Text.Replace("|", "\|").Replace(vbCrLf, "\n")
            Dim code As String = txtItemCode.Text.ToUpper().Replace("|", "")
            Dim category As String = cmbCategory.SelectedItem.ToString()
            Dim unit As String = cmbUnit.SelectedItem.ToString()

            Dim localNames As New List(Of String)
            If Not String.IsNullOrWhiteSpace(txtLuganda.Text) Then
                localNames.Add("Luganda:" & txtLuganda.Text.Replace("|", ""))
            End If
            If Not String.IsNullOrWhiteSpace(txtRunyankole.Text) Then
                localNames.Add("Runyankole:" & txtRunyankole.Text.Replace("|", ""))
            End If
            If Not String.IsNullOrWhiteSpace(txtLusoga.Text) Then
                localNames.Add("Lusoga:" & txtLusoga.Text.Replace("|", ""))
            End If
            If Not String.IsNullOrWhiteSpace(txtAcholi.Text) Then
                localNames.Add("Acholi:" & txtAcholi.Text.Replace("|", ""))
            End If

            Dim localNamesStr As String = String.Join("|", localNames)
            Dim reqId As String = DateTime.Now.Ticks.ToString()

            ' Build the message using plain concatenation to avoid any interpolation issues
            Dim msg As String = "ITEM_ADD|" & reqId & "|" & Token & "|" & TraderID.ToString() & "|" & category & "|" & code & "|" & name & "|" & desc & "|" & price.ToString() & "|" & unit & "|" & lowStock.ToString() & "|" & qty.ToString() & "|" & shelf.ToString() & "|" & localNamesStr & vbCrLf

            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Item saved successfully!" & vbCrLf & "Batch No: " & parts(2), "Success")
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                MessageBox.Show("Error: " & parts(3), "Server Error")
            End If
        Catch ex As Exception
            MessageBox.Show("Connection Error: " & ex.Message)
        End Try
    End Sub
End Class