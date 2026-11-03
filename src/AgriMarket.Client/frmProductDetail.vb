Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmProductDetail
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property ItemID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property BuyerID As Integer

    Private lblName As New Label()
    Private lblTrader As New Label()
    Private lblPrice As New Label()
    Private lblQty As New Label()
    Private lblDesc As New Label()
    Private lblFresh As New Label()
    Private btnPlaceOrder As New Button()

    Private _availableQty As Double = 0
    Private _unit As String = ""

    Private Async Sub frmProductDetail_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Product Detail"
        Me.ClientSize = New Size(650, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        BuildLayout()
        Await LoadDetail()
    End Sub

    Private Sub BuildLayout()
        lblName.Text = "Loading..."
        lblName.Font = modTheme.fTitle
        lblName.ForeColor = modTheme.cOcean
        lblName.Location = New Point(20, 20)
        lblName.AutoSize = True
        Me.Controls.Add(lblName)

        lblTrader.Font = modTheme.fBody
        lblTrader.ForeColor = modTheme.cMuted
        lblTrader.Location = New Point(22, 65)
        lblTrader.AutoSize = True
        Me.Controls.Add(lblTrader)

        lblFresh.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lblFresh.AutoSize = True
        lblFresh.Location = New Point(20, 95)
        Me.Controls.Add(lblFresh)

        lblPrice.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        lblPrice.ForeColor = modTheme.cText
        lblPrice.Location = New Point(20, 130)
        lblPrice.AutoSize = True
        Me.Controls.Add(lblPrice)

        lblQty.Font = modTheme.fBody
        lblQty.ForeColor = modTheme.cMuted
        lblQty.Location = New Point(22, 170)
        lblQty.AutoSize = True
        Me.Controls.Add(lblQty)

        lblDesc.Font = modTheme.fBody
        lblDesc.ForeColor = modTheme.cText
        lblDesc.Location = New Point(20, 210)
        lblDesc.Size = New Size(600, 120)
        Me.Controls.Add(lblDesc)

        btnPlaceOrder.Text = "Place Order"
        btnPlaceOrder.BackColor = modTheme.cOcean
        btnPlaceOrder.ForeColor = modTheme.cWhite
        btnPlaceOrder.FlatStyle = FlatStyle.Flat
        btnPlaceOrder.FlatAppearance.BorderSize = 0
        btnPlaceOrder.Location = New Point(20, 380)
        btnPlaceOrder.Size = New Size(180, 45)
        btnPlaceOrder.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnPlaceOrder.Click, AddressOf btnPlaceOrder_Click
        Me.Controls.Add(btnPlaceOrder)

        Dim btnClose As New Button()
        btnClose.Text = "Close"
        btnClose.BackColor = modTheme.cWhite
        btnClose.ForeColor = modTheme.cOcean
        btnClose.FlatStyle = FlatStyle.Flat
        btnClose.FlatAppearance.BorderColor = modTheme.cOcean
        btnClose.FlatAppearance.BorderSize = 1
        btnClose.Location = New Point(220, 380)
        btnClose.Size = New Size(150, 45)
        btnClose.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnClose.Click, Sub(s, ev) Me.Close()
        Me.Controls.Add(btnClose)
    End Sub

    Private Async Function LoadDetail() As Task
        If String.IsNullOrEmpty(Token) Then Return
        Try
            Dim msg As String = $"ITEM_DETAIL|7|{Token}|{ItemID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(4096) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                ' Ensure the reply has enough parts to avoid crashes
                If parts.Length < 14 Then
                    MessageBox.Show("Invalid data received from server.")
                    Me.Close()
                    Return
                End If

                ' Name and Local Name
                lblName.Text = parts(3)
                Dim localName As String = parts(13)
                If Not String.IsNullOrEmpty(localName) Then
                    lblName.Text &= " (" & localName & ")"
                End If

                ' Trader details
                lblTrader.Text = "By: " & parts(9) & "  |  " & parts(10)

                ' Price and Quantity
                lblPrice.Text = "UGX " & parts(8) & " / " & parts(7)
                _availableQty = Double.Parse(parts(6))
                _unit = parts(7)
                lblQty.Text = _availableQty.ToString("F2") & " " & _unit & " available"

                ' Description
                lblDesc.Text = parts(5).Replace("\n", vbCrLf)

                ' Freshness Badge
                Dim daysLeft As Integer = Integer.Parse(parts(12))
                lblFresh.Text = daysLeft.ToString() & " days left"
                If daysLeft <= 2 Then
                    lblFresh.BackColor = Color.FromArgb(250, 225, 225)
                    lblFresh.ForeColor = modTheme.cDanger
                ElseIf daysLeft <= 5 Then
                    lblFresh.BackColor = Color.FromArgb(250, 240, 220)
                    lblFresh.ForeColor = modTheme.cWarning
                Else
                    lblFresh.BackColor = Color.FromArgb(220, 240, 230)
                    lblFresh.ForeColor = modTheme.cSuccess
                End If
            Else
                MessageBox.Show("Error: " & parts(3))
                Me.Close()
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading detail: " & ex.Message)
        End Try
    End Function

    Private Sub btnPlaceOrder_Click(sender As Object, e As EventArgs)
        Dim orderForm As New frmPlaceOrder()
        orderForm.ItemID = ItemID
        orderForm.ItemName = lblName.Text
        orderForm.UnitPrice = Decimal.Parse(lblPrice.Text.Replace("UGX ", "").Split("/"c)(0).Trim())
        orderForm.AvailableQty = _availableQty
        orderForm.Unit = _unit
        orderForm.Token = Token
        orderForm.BuyerID = BuyerID

        If orderForm.ShowDialog() = DialogResult.OK Then
            Me.Close()
        End If
    End Sub
End Class