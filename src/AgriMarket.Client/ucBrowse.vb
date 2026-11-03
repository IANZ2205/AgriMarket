Imports System.Drawing.Drawing2D
Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class ucBrowse
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property BuyerID As Integer

    Private txtSearch As New TextBox()
    Private flpCategories As New FlowLayoutPanel()
    Private flpProducts As New FlowLayoutPanel()
    Private selectedCategory As String = "All"

    Private Sub ucBrowse_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill

        BuildHeader()
        BuildCategories()
        BuildProductGrid()
        LoadListings()
    End Sub

    Private Sub BuildHeader()
        Dim lblTitle As New Label()
        lblTitle.Text = "Fresh produce from traders near you"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        Dim lblSub As New Label()
        lblSub.Text = "Search by product name, local name, or trader."
        lblSub.Font = modTheme.fBody
        lblSub.ForeColor = modTheme.cMuted
        lblSub.Location = New Point(2, 45)
        lblSub.AutoSize = True
        Me.Controls.Add(lblSub)

        txtSearch.Location = New Point(0, 85)
        txtSearch.Size = New Size(400, 30)
        txtSearch.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        Me.Controls.Add(txtSearch)

        Dim btnSearch As New Button()
        btnSearch.Text = "Search"
        btnSearch.BackColor = modTheme.cOcean
        btnSearch.ForeColor = modTheme.cWhite
        btnSearch.FlatStyle = FlatStyle.Flat
        btnSearch.FlatAppearance.BorderSize = 0
        btnSearch.Location = New Point(420, 84)
        btnSearch.Size = New Size(100, 32)
        btnSearch.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnSearch.Click, AddressOf DoSearch
        Me.Controls.Add(btnSearch)
    End Sub

    Private Sub BuildCategories()
        flpCategories.Location = New Point(0, 130)
        flpCategories.Size = New Size(Me.Width, 40)
        flpCategories.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        flpCategories.FlowDirection = FlowDirection.LeftToRight
        flpCategories.WrapContents = False
        Me.Controls.Add(flpCategories)

        Dim cats As String() = {"All", "Food crop", "Cash crop", "Livestock", "Poultry", "Fish", "Dairy and eggs", "Other"}
        For Each cat In cats
            Dim btn As New Button()
            btn.Text = cat
            btn.Tag = cat
            btn.AutoSize = True
            btn.FlatStyle = FlatStyle.Flat
            btn.FlatAppearance.BorderSize = 1
            btn.FlatAppearance.BorderColor = modTheme.cBorder
            btn.BackColor = If(cat = "All", modTheme.cOcean, modTheme.cWhite)
            btn.ForeColor = If(cat = "All", modTheme.cWhite, modTheme.cText)
            btn.Font = New Font("Segoe UI", 9, FontStyle.Regular)
            btn.Padding = New Padding(8, 2, 8, 2)
            AddHandler btn.Click, AddressOf CategoryClicked
            flpCategories.Controls.Add(btn)
        Next
    End Sub

    Private Sub CategoryClicked(sender As Object, e As EventArgs)
        Dim btn As Button = CType(sender, Button)
        selectedCategory = btn.Tag.ToString()

        For Each ctrl As Control In flpCategories.Controls
            Dim b As Button = CType(ctrl, Button)
            If b.Tag.ToString() = selectedCategory Then
                b.BackColor = modTheme.cOcean
                b.ForeColor = modTheme.cWhite
            Else
                b.BackColor = modTheme.cWhite
                b.ForeColor = modTheme.cText
            End If
        Next
        LoadListings()
    End Sub

    Private Sub DoSearch(sender As Object, e As EventArgs)
        LoadListings()
    End Sub

    Private Sub BuildProductGrid()
        flpProducts.Location = New Point(0, 180)
        flpProducts.Size = New Size(Me.Width, Me.Height - 200)
        flpProducts.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        flpProducts.FlowDirection = FlowDirection.LeftToRight
        flpProducts.WrapContents = True
        flpProducts.AutoScroll = True
        Me.Controls.Add(flpProducts)
    End Sub

    Private Async Sub LoadListings()
        If String.IsNullOrEmpty(Token) Then Return

        Try
            Dim searchText As String = txtSearch.Text.Trim()
            Dim msg As String = $"BROWSE|6|{Token}|{selectedCategory}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(16384) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()

            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
            flpProducts.Controls.Clear()

            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)
                If p.Length >= 9 AndAlso p(0) = "LISTING" Then
                    ' Apply local search filter
                    Dim itemName As String = p(2)
                    Dim traderName As String = p(7)
                    If Not String.IsNullOrEmpty(searchText) Then
                        If Not itemName.ToLower().Contains(searchText.ToLower()) AndAlso
                           Not traderName.ToLower().Contains(searchText.ToLower()) Then
                            Continue For
                        End If
                    End If
                    flpProducts.Controls.Add(CreateProductCard(p))
                End If
            Next

            If flpProducts.Controls.Count = 0 Then
                Dim lblEmpty As New Label()
                lblEmpty.Text = "No produce found for this category."
                lblEmpty.Font = modTheme.fBody
                lblEmpty.ForeColor = modTheme.cMuted
                lblEmpty.AutoSize = True
                lblEmpty.Margin = New Padding(0, 20, 0, 0)
                flpProducts.Controls.Add(lblEmpty)
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading listings: " & ex.Message)
        End Try
    End Sub

    Private Function CreateProductCard(data() As String) As Panel
        Dim card As New Panel()
        card.BackColor = modTheme.cWhite
        card.Size = New Size(260, 140)
        card.Margin = New Padding(8)
        card.BorderStyle = BorderStyle.FixedSingle
        card.Cursor = Cursors.Hand
        card.Tag = data(1) ' ItemID

        Dim lblName As New Label()
        lblName.Text = data(2)
        lblName.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblName.ForeColor = modTheme.cOcean
        lblName.Location = New Point(12, 12)
        lblName.AutoSize = True
        card.Controls.Add(lblName)

        Dim lblTrader As New Label()
        lblTrader.Text = "By: " & data(7)
        lblTrader.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        lblTrader.ForeColor = modTheme.cMuted
        lblTrader.Location = New Point(12, 40)
        lblTrader.AutoSize = True
        card.Controls.Add(lblTrader)

        Dim lblPrice As New Label()
        lblPrice.Text = "UGX " & data(6) & " / " & data(5)
        lblPrice.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        lblPrice.ForeColor = modTheme.cText
        lblPrice.Location = New Point(12, 65)
        lblPrice.AutoSize = True
        card.Controls.Add(lblPrice)

        Dim lblQty As New Label()
        lblQty.Text = data(4) & " " & data(5) & " available"
        lblQty.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        lblQty.ForeColor = modTheme.cMuted
        lblQty.Location = New Point(12, 90)
        lblQty.AutoSize = True
        card.Controls.Add(lblQty)

        Dim lblFresh As New Label()
        Dim daysLeft As Integer = Integer.Parse(data(8))
        lblFresh.Text = daysLeft.ToString() & " days left"
        lblFresh.Font = New Font("Segoe UI", 8, FontStyle.Bold)
        lblFresh.AutoSize = True
        lblFresh.Location = New Point(180, 12)
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
        card.Controls.Add(lblFresh)

        AddHandler card.Click, AddressOf ProductCardClicked
        Return card
    End Function

    Private Sub ProductCardClicked(sender As Object, e As EventArgs)
        Dim card As Panel = CType(sender, Panel)
        Dim itemId As Integer = Integer.Parse(card.Tag.ToString())
        Dim detailForm As New frmProductDetail()
        detailForm.ItemID = itemId
        detailForm.Token = Token
        detailForm.BuyerID = BuyerID
        detailForm.ShowDialog()
    End Sub
End Class
