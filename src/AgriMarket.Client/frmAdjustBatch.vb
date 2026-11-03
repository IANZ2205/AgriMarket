Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmAdjustBatch
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property BatchID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentQty As Double

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentShelfLife As Integer

    Private txtQty As New TextBox()
    Private txtShelf As New TextBox()
    Private txtReason As New TextBox()

    Private Sub frmAdjustBatch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Adjust Batch"
        Me.ClientSize = New Size(500, 400)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        Dim lblTitle As New Label()
        lblTitle.Text = "Adjust Batch"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        AddLabel("New Quantity", 20, 80)
        txtQty.Location = New Point(20, 100)
        txtQty.Size = New Size(150, 25)
        txtQty.Text = CurrentQty.ToString()
        Me.Controls.Add(txtQty)

        AddLabel("New Shelf Life (days)", 200, 80)
        txtShelf.Location = New Point(200, 100)
        txtShelf.Size = New Size(150, 25)
        txtShelf.Text = CurrentShelfLife.ToString()
        Me.Controls.Add(txtShelf)

        AddLabel("Reason for adjustment (mandatory)", 20, 150)
        txtReason.Location = New Point(20, 170)
        txtReason.Size = New Size(400, 80)
        txtReason.Multiline = True
        Me.Controls.Add(txtReason)

        Dim btnSave As New Button()
        btnSave.Text = "Apply Adjustment"
        btnSave.BackColor = modTheme.cOcean
        btnSave.ForeColor = modTheme.cWhite
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.FlatAppearance.BorderSize = 0
        btnSave.Location = New Point(20, 280)
        btnSave.Size = New Size(180, 45)
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
        btnCancel.Location = New Point(220, 280)
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
        If String.IsNullOrEmpty(txtReason.Text.Trim()) Then
            MessageBox.Show("A reason is required for any adjustment.", "Validation")
            Return
        End If

        Try
            Dim qty As Double = Double.Parse(txtQty.Text)
            Dim shelf As Integer = Integer.Parse(txtShelf.Text)
            Dim reason As String = txtReason.Text.Replace("|", "\|")

            Dim reqId As String = DateTime.Now.Ticks.ToString()
            Dim msg As String = $"BATCH_UPDATE|{reqId}|{Token}|{BatchID}|{qty}|{shelf}|{reason}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Batch adjusted successfully!", "Success")
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