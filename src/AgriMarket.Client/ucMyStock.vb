Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]
Imports System.Text

Public Class ucMyStock
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    Private lvStock As New ListView()
    Private btnAddItem As New Button()

    Private Sub ucMyStock_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill

        BuildHeader()
        BuildTable()
        LoadStock() ' <-- Call it when the screen opens
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
    End Sub

    Private Sub BuildTable()
        lvStock.View = View.Details
        lvStock.FullRowSelect = True
        lvStock.GridLines = False
        lvStock.Font = modTheme.fBody
        lvStock.Location = New Point(0, 90)
        lvStock.Size = New Size(Me.Width, Me.Height - 100)
        lvStock.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvStock.BackColor = modTheme.cWhite
        lvStock.BorderStyle = BorderStyle.FixedSingle

        lvStock.Columns.Add("English Name", 200)
        lvStock.Columns.Add("Category", 150)
        lvStock.Columns.Add("Available Qty", 120)
        lvStock.Columns.Add("Unit", 80)
        lvStock.Columns.Add("Price (UGX)", 120)
        lvStock.Columns.Add("Status", 150)

        Me.Controls.Add(lvStock)
    End Sub

    Private Sub OpenAddItem()
        Dim frm As New frmAddItem()
        frm.TraderID = TraderID
        frm.Token = Token
        If frm.ShowDialog() = DialogResult.OK Then
            LoadStock() ' <-- Refresh the list if they saved a new item
        End If
    End Sub
    Private Async Sub LoadStock()
        If String.IsNullOrEmpty(Token) Then Return

        Try
            ' Format: STOCK_LIST|ReqID|Token|TraderID
            Dim msg As String = $"STOCK_LIST|4|{Token}|{TraderID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(8192) As Byte ' Make buffer larger for lists
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim parts() As String = reply.Split("|"c)

            If parts(0) = "OK" Then
                lvStock.Items.Clear()

                ' If there are records, split by Chr(30)
                If parts.Length > 2 Then
                    Dim records() As String = parts(2).Split(Chr(30))

                    For Each record In records
                        If String.IsNullOrEmpty(record) Then Continue For

                        Dim fields() As String = record.Split(Chr(31))
                        ' fields: 0=ItemID, 1=Name, 2=AvailableQty, 3=Unit, 4=Price, 5=BatchCount

                        Dim lvi As New ListViewItem(fields(1)) ' Name
                        lvi.SubItems.Add("Produce") ' Category (Placeholder for now)
                        lvi.SubItems.Add(fields(2)) ' Qty
                        lvi.SubItems.Add(fields(3)) ' Unit
                        lvi.SubItems.Add(fields(4)) ' Price

                        ' Simple Status logic for now
                        Dim qty As Double = Double.Parse(fields(2))
                        If qty > 0 Then
                            lvi.SubItems.Add("Active")
                            lvi.ForeColor = modTheme.cSuccess
                        Else
                            lvi.SubItems.Add("Out of Stock")
                            lvi.ForeColor = modTheme.cDanger
                        End If

                        lvStock.Items.Add(lvi)
                    Next
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading stock: " & ex.Message)
        End Try
    End Sub
End Class