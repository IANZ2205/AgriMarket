Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmPlaceOrder
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property ItemID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property ItemName As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UnitPrice As Decimal

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property AvailableQty As Double

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Unit As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property BuyerID As Integer

    Private txtQty As New TextBox()
    Private dtpNeededBy As New DateTimePicker()
    Private cmbDelivery As New ComboBox()
    Private txtNote As New TextBox()
    Private lblTotal As New Label()

    Private Sub frmPlaceOrder_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Place Order"
        Me.ClientSize = New Size(550, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        Dim lblTitle As New Label()
        lblTitle.Text = "Order: " & ItemName
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        AddLabel("Quantity (" & Unit & ")", 20, 80)
        txtQty.Location = New Point(20, 100)
        txtQty.Size = New Size(150, 25)
        AddHandler txtQty.TextChanged, AddressOf QtyChanged
        Me.Controls.Add(txtQty)

        AddLabel("Available: " & AvailableQty.ToString("F2") & " " & Unit, 200, 80)

        AddLabel("Needed by", 20, 150)
        dtpNeededBy.Location = New Point(20, 170)
        dtpNeededBy.Size = New Size(200, 25)
        dtpNeededBy.Value = DateTime.Now.AddDays(2)
        Me.Controls.Add(dtpNeededBy)

        AddLabel("Delivery mode", 250, 150)
        cmbDelivery.Items.AddRange(New Object() {"Pickup", "Delivery"})
        cmbDelivery.Location = New Point(250, 170)
        cmbDelivery.Size = New Size(150, 25)
        cmbDelivery.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDelivery.SelectedIndex = 0
        Me.Controls.Add(cmbDelivery)

        AddLabel("Note to trader", 20, 220)
        txtNote.Location = New Point(20, 240)
        txtNote.Size = New Size(500, 70)
        txtNote.Multiline = True
        Me.Controls.Add(txtNote)

        lblTotal.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblTotal.ForeColor = modTheme.cText
        lblTotal.Location = New Point(20, 330)
        lblTotal.AutoSize = True
        lblTotal.Text = "Estimated total: UGX 0"
        Me.Controls.Add(lblTotal)

        Dim btnPlace As New Button()
        btnPlace.Text = "Place Order"
        btnPlace.BackColor = modTheme.cOcean
        btnPlace.ForeColor = modTheme.cWhite
        btnPlace.FlatStyle = FlatStyle.Flat
        btnPlace.FlatAppearance.BorderSize = 0
        btnPlace.Location = New Point(20, 380)
        btnPlace.Size = New Size(180, 45)
        btnPlace.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnPlace.Click, AddressOf btnPlace_Click
        Me.Controls.Add(btnPlace)

        Dim btnCancel As New Button()
        btnCancel.Text = "Cancel"
        btnCancel.BackColor = modTheme.cWhite
        btnCancel.ForeColor = modTheme.cOcean
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.FlatAppearance.BorderColor = modTheme.cOcean
        btnCancel.FlatAppearance.BorderSize = 1
        btnCancel.Location = New Point(220, 380)
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

    Private Sub QtyChanged(sender As Object, e As EventArgs)
        Dim qty As Double = 0
        Double.TryParse(txtQty.Text, qty)
        Dim total As Decimal = CDec(qty) * UnitPrice
        lblTotal.Text = "Estimated total: UGX " & total.ToString("N0")
    End Sub

    Private Async Sub btnPlace_Click(sender As Object, e As EventArgs)
        Dim qty As Double = 0
        If Not Double.TryParse(txtQty.Text, qty) OrElse qty <= 0 Then
            MessageBox.Show("Please enter a valid quantity.", "Validation")
            Return
        End If
        If qty > AvailableQty Then
            MessageBox.Show($"Only {AvailableQty} {Unit} available. Please lower the quantity.", "Insufficient Stock")
            Return
        End If

        Try
            Dim neededBy As String = dtpNeededBy.Value.ToString("yyyy-MM-dd")
            Dim delivery As String = cmbDelivery.SelectedItem.ToString()
            Dim note As String = txtNote.Text.Replace("|", "\|").Replace(vbCrLf, "\n")

            Dim reqId As String = DateTime.Now.Ticks.ToString()
            Dim msg As String = $"ORDER_PLACE|{reqId}|{Token}|{ItemID}|{qty}|{neededBy}|{delivery}|{note}|{BuyerID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Order placed successfully!" & vbCrLf & "Order No: " & parts(2), "Success")
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