Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmTopUp
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property ItemID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    Private txtQty As New TextBox()
    Private txtExpiry As New TextBox()
    Private txtNewPrice As New TextBox()

    Private Sub frmTopUp_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Top Up Item"
        Me.ClientSize = New Size(500, 400)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        Dim lblTitle As New Label()
        lblTitle.Text = "Top Up Stock"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        AddLabel("Quantity to add", 20, 80)
        txtQty.Location = New Point(20, 100)
        txtQty.Size = New Size(150, 25)
        Me.Controls.Add(txtQty)

        AddLabel("Shelf life (days)", 200, 80)
        txtExpiry.Location = New Point(200, 100)
        txtExpiry.Size = New Size(150, 25)
        Me.Controls.Add(txtExpiry)

        AddLabel("New price per unit (leave 0 to keep current)", 20, 150)
        txtNewPrice.Location = New Point(20, 170)
        txtNewPrice.Size = New Size(300, 25)
        txtNewPrice.Text = "0"
        Me.Controls.Add(txtNewPrice)

        Dim btnSave As New Button()
        btnSave.Text = "Top Up"
        btnSave.BackColor = modTheme.cOcean
        btnSave.ForeColor = modTheme.cWhite
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.FlatAppearance.BorderSize = 0
        btnSave.Location = New Point(20, 240)
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
        btnCancel.Location = New Point(180, 240)
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
        If String.IsNullOrEmpty(txtQty.Text) OrElse String.IsNullOrEmpty(txtExpiry.Text) Then
            MessageBox.Show("Please fill in quantity and shelf life.", "Validation")
            Return
        End If

        Try
            Dim qty As Double = Double.Parse(txtQty.Text)
            Dim shelf As Integer = Integer.Parse(txtExpiry.Text)
            Dim newPrice As Decimal = 0
            If Not String.IsNullOrEmpty(txtNewPrice.Text) Then newPrice = Decimal.Parse(txtNewPrice.Text)

            Dim reqId As String = DateTime.Now.Ticks.ToString()
            Dim msg As String = $"ITEM_TOPUP|{reqId}|{Token}|{ItemID}|{qty}|{shelf}|{newPrice}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Top-up successful!" & vbCrLf & "Batch No: " & parts(2), "Success")
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