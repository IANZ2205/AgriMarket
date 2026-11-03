Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmEditItem
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property ItemID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentName As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentDesc As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentPrice As Decimal

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentLowStock As Integer

    Private txtName As New TextBox()
    Private txtDesc As New TextBox()
    Private txtPrice As New TextBox()
    Private txtLowStock As New TextBox()

    Private Sub frmEditItem_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Edit Item"
        Me.ClientSize = New Size(550, 450)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        Dim lblTitle As New Label()
        lblTitle.Text = "Edit Item"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        AddLabel("English Name", 20, 80)
        txtName.Location = New Point(20, 100)
        txtName.Size = New Size(250, 25)
        txtName.Text = CurrentName
        Me.Controls.Add(txtName)

        AddLabel("Description", 20, 150)
        txtDesc.Location = New Point(20, 170)
        txtDesc.Size = New Size(500, 80)
        txtDesc.Multiline = True
        txtDesc.Text = CurrentDesc
        Me.Controls.Add(txtDesc)

        AddLabel("Price per unit (UGX)", 20, 270)
        txtPrice.Location = New Point(20, 290)
        txtPrice.Size = New Size(150, 25)
        txtPrice.Text = CurrentPrice.ToString()
        Me.Controls.Add(txtPrice)

        AddLabel("Low Stock Level", 200, 270)
        txtLowStock.Location = New Point(200, 290)
        txtLowStock.Size = New Size(100, 25)
        txtLowStock.Text = CurrentLowStock.ToString()
        Me.Controls.Add(txtLowStock)

        Dim btnSave As New Button()
        btnSave.Text = "Save Changes"
        btnSave.BackColor = modTheme.cOcean
        btnSave.ForeColor = modTheme.cWhite
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.FlatAppearance.BorderSize = 0
        btnSave.Location = New Point(20, 350)
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
        btnCancel.Location = New Point(180, 350)
        btnCancel.Size = New Size(150, 45)
        btnCancel.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnCancel.Click, Sub(s, ev) Me.Close()
        Me.Controls.Add(btnCancel)
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
        Try
            Dim price As Decimal = Decimal.Parse(txtPrice.Text)
            Dim lowStock As Integer = Integer.Parse(txtLowStock.Text)
            Dim name As String = txtName.Text.Replace("|", "\|")
            Dim desc As String = txtDesc.Text.Replace("|", "\|").Replace(vbCrLf, "\n")

            Dim reqId As String = DateTime.Now.Ticks.ToString()
            Dim msg As String = $"ITEM_UPDATE|{reqId}|{Token}|{ItemID}|{name}|{desc}|{price}|{lowStock}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Item updated successfully!", "Success")
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                MessageBox.Show("Error: " & parts(3), "Error")
            End If
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub
End Class