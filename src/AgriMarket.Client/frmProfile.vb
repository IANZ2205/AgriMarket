Imports System.Drawing
Imports System.Windows.Forms
Imports System.Text
Imports AgriMarket.Shared

Public Class frmProfile
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UserID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentPhone As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentLocation As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentDisplayName As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentPic As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CurrentRole As String

    Private txtPhone As New TextBox()
    Private txtLocation As New TextBox()
    Private lblSelectedPhoto As New Label()
    Private selectedPhotoPath As String = ""

    Private Async Sub frmProfile_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "My Profile"
        Me.ClientSize = New Size(600, 450)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = modTheme.cWhite
        Me.Font = modTheme.fBody

        Dim lblTitle As New Label()
        lblTitle.Text = "My Profile"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(20, 20)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        ' Fetch current profile data
        Try
            Dim msg As String = $"GET_PROFILE|6|{Token}|{UserID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" AndAlso parts.Length >= 7 Then
                CurrentDisplayName = parts(2)
                CurrentRole = parts(3)
                CurrentPhone = parts(4)
                CurrentLocation = parts(5)
                CurrentPic = parts(6)
            End If
        Catch ex As Exception
        End Try

        ' --- LEFT SIDE: Picture ---
        Dim picBox As New PictureBox()
        picBox.Name = "picProfile"
        picBox.Location = New Point(20, 80)
        picBox.Size = New Size(150, 150)
        picBox.SizeMode = PictureBoxSizeMode.Zoom
        picBox.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(picBox)

        If Not String.IsNullOrEmpty(CurrentPic) Then
            picBox.Image = Await DownloadImage(CurrentPic)
        End If

        Dim btnChoosePhoto As New Button()
        btnChoosePhoto.Text = "Change Photo"
        btnChoosePhoto.Location = New Point(20, 240)
        btnChoosePhoto.Size = New Size(150, 30)
        AddHandler btnChoosePhoto.Click, AddressOf btnChoosePhoto_Click
        Me.Controls.Add(btnChoosePhoto)

        lblSelectedPhoto.Text = "No photo selected."
        lblSelectedPhoto.Location = New Point(20, 280)
        lblSelectedPhoto.AutoSize = True
        Me.Controls.Add(lblSelectedPhoto)

        ' --- RIGHT SIDE: Details ---
        AddLabel("Display Name (Read-only)", 200, 80)
        Dim txtName As New TextBox()
        txtName.Text = CurrentDisplayName
        txtName.ReadOnly = True
        txtName.Location = New Point(200, 100)
        txtName.Size = New Size(350, 25)
        txtName.BackColor = Color.LightGray
        Me.Controls.Add(txtName)

        AddLabel("Role", 200, 140)
        Dim txtRole As New TextBox()
        txtRole.Text = CurrentRole
        txtRole.ReadOnly = True
        txtRole.Location = New Point(200, 160)
        txtRole.Size = New Size(150, 25)
        txtRole.BackColor = Color.LightGray
        Me.Controls.Add(txtRole)

        AddLabel("Phone Number", 380, 140)
        txtPhone.Location = New Point(380, 160)
        txtPhone.Size = New Size(170, 25)
        txtPhone.Text = CurrentPhone
        Me.Controls.Add(txtPhone)

        AddLabel("Location (District / Village)", 200, 200)
        txtLocation.Location = New Point(200, 220)
        txtLocation.Size = New Size(350, 25)
        txtLocation.Text = CurrentLocation
        Me.Controls.Add(txtLocation)

        ' Save Button
        Dim btnSave As New Button()
        btnSave.Text = "Save Profile"
        btnSave.BackColor = modTheme.cOcean
        btnSave.ForeColor = modTheme.cWhite
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.FlatAppearance.BorderSize = 0
        btnSave.Location = New Point(200, 300)
        btnSave.Size = New Size(350, 45)
        btnSave.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnSave.Click, AddressOf btnSave_Click
        Me.Controls.Add(btnSave)
    End Sub

    ' --- This is the missing helper function ---
    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Dim lbl As New Label()
        lbl.Text = text
        lbl.Location = New Point(x, y)
        lbl.AutoSize = True
        lbl.ForeColor = modTheme.cText
        Me.Controls.Add(lbl)
    End Sub

    Private Sub btnChoosePhoto_Click(sender As Object, e As EventArgs)
        Dim ofd As New OpenFileDialog()
        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png"
        If ofd.ShowDialog() = DialogResult.OK Then
            selectedPhotoPath = ofd.FileName
            lblSelectedPhoto.Text = System.IO.Path.GetFileName(selectedPhotoPath)
            lblSelectedPhoto.ForeColor = modTheme.cSuccess
        End If
    End Sub

    Private Async Sub btnSave_Click(sender As Object, e As EventArgs)
        Try
            Dim storedFileName As String = ""

            ' 1. Upload Photo (if selected)
            If Not String.IsNullOrEmpty(selectedPhotoPath) Then
                Dim fileBytes As Byte() = System.IO.File.ReadAllBytes(selectedPhotoPath)
                Dim base64String As String = Convert.ToBase64String(fileBytes)
                Dim originalName As String = System.IO.Path.GetFileName(selectedPhotoPath)

                Dim uploadMsg As String = $"FILE_UPLOAD|0|{base64String}|{originalName}" & vbCrLf
                Dim uploadData As Byte() = Encoding.UTF8.GetBytes(uploadMsg)
                Await frmConnect.Stream.WriteAsync(uploadData, 0, uploadData.Length)

                Dim uploadBuffer(4096) As Byte
                Dim uploadRead As Integer = Await frmConnect.Stream.ReadAsync(uploadBuffer, 0, uploadBuffer.Length)
                Dim uploadReply As String = Encoding.UTF8.GetString(uploadBuffer, 0, uploadRead).Trim()
                Dim uploadParts() As String = uploadReply.Split("|"c)

                If uploadParts(0) <> "OK" Then
                    MessageBox.Show("Photo upload failed: " & uploadParts(3))
                    Return
                End If
                storedFileName = uploadParts(2)
            End If

            ' 2. Update Profile
            Dim phone As String = txtPhone.Text.Replace("|", "\|")
            Dim location As String = txtLocation.Text.Replace("|", "\|")
            Dim msg As String = $"UPDATE_PROFILE|7|{UserID}|{phone}|{location}|{storedFileName}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(1024) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                MessageBox.Show("Profile updated successfully!", "Success")
                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                MessageBox.Show("Error: " & parts(3), "Error")
            End If
        Catch ex As Exception
            MessageBox.Show("Connection Error: " & ex.Message)
        End Try
    End Sub

    Private Async Function DownloadImage(fileName As String) As Task(Of Image)
        Try
            Dim msg As String = $"FILE_GET|1|{Token}|{fileName}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(16384) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                Dim imgBytes As Byte() = Convert.FromBase64String(parts(2))
                Using ms As New IO.MemoryStream(imgBytes)
                    Return Image.FromStream(ms)
                End Using
            End If
        Catch ex As Exception
        End Try
        Return Nothing
    End Function

End Class