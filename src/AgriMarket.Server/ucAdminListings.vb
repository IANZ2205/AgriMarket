Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports AgriMarket.Shared

Public Class ucAdminListings
    Private lvListings As New ListView()
    Private btnHide As New Button()
    Private btnActivate As New Button()
    Private btnRefresh As New Button()

    Private Sub ucAdminListings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        LoadListings()
    End Sub

    Private Sub BuildLayout()
        Dim lblTitle As New Label()
        lblTitle.Text = "All Listings"
        lblTitle.Font = modTheme.fTitle
        lblTitle.ForeColor = modTheme.cOcean
        lblTitle.Location = New Point(0, 0)
        lblTitle.AutoSize = True
        Me.Controls.Add(lblTitle)

        btnRefresh.Text = "Refresh"
        btnRefresh.BackColor = modTheme.cWhite
        btnRefresh.ForeColor = modTheme.cOcean
        btnRefresh.FlatStyle = FlatStyle.Flat
        btnRefresh.FlatAppearance.BorderColor = modTheme.cOcean
        btnRefresh.FlatAppearance.BorderSize = 1
        btnRefresh.Location = New Point(Me.Width - 130, 20)
        btnRefresh.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRefresh.Size = New Size(110, 40)
        btnRefresh.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnRefresh.Click, Sub(s, ev) LoadListings()
        Me.Controls.Add(btnRefresh)

        lvListings.View = View.Details
        lvListings.FullRowSelect = True
        lvListings.GridLines = False
        lvListings.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvListings.Location = New Point(0, 70)
        lvListings.Size = New Size(Me.Width, Me.Height - 160)
        lvListings.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvListings.BackColor = modTheme.cWhite
        lvListings.BorderStyle = BorderStyle.FixedSingle

        lvListings.Columns.Add("Item", 180)
        lvListings.Columns.Add("Category", 120)
        lvListings.Columns.Add("Trader", 150)
        lvListings.Columns.Add("Available", 100)
        lvListings.Columns.Add("Price (UGX)", 100)
        lvListings.Columns.Add("Status", 100)

        Me.Controls.Add(lvListings)

        btnHide.Text = "Hide Listing"
        btnHide.BackColor = modTheme.cDanger
        btnHide.ForeColor = modTheme.cWhite
        btnHide.FlatStyle = FlatStyle.Flat
        btnHide.FlatAppearance.BorderSize = 0
        btnHide.Location = New Point(0, Me.Height - 70)
        btnHide.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnHide.Size = New Size(150, 45)
        btnHide.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnHide.Click, AddressOf btnHide_Click
        Me.Controls.Add(btnHide)

        btnActivate.Text = "Activate Listing"
        btnActivate.BackColor = modTheme.cSuccess
        btnActivate.ForeColor = modTheme.cWhite
        btnActivate.FlatStyle = FlatStyle.Flat
        btnActivate.FlatAppearance.BorderSize = 0
        btnActivate.Location = New Point(160, Me.Height - 70)
        btnActivate.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnActivate.Size = New Size(150, 45)
        btnActivate.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnActivate.Click, AddressOf btnActivate_Click
        Me.Controls.Add(btnActivate)
    End Sub

    Private Sub LoadListings()
        lvListings.Items.Clear()
        Dim dt As DataTable = modDB.GetAllListings()
        For Each row As DataRow In dt.Rows
            Dim lvi As New ListViewItem(row("EnglishName").ToString())
            lvi.SubItems.Add(row("Category").ToString())
            lvi.SubItems.Add(row("TraderName").ToString())
            lvi.SubItems.Add(Convert.ToDouble(row("AvailableQty")).ToString("F2"))
            lvi.SubItems.Add(Convert.ToDecimal(row("UnitPrice")).ToString("F0"))
            lvi.SubItems.Add(row("Status").ToString())

            If row("Status").ToString() = "Active" Then
                lvi.ForeColor = modTheme.cSuccess
            Else
                lvi.ForeColor = modTheme.cDanger
            End If

            lvi.Tag = row("ItemID")
            lvListings.Items.Add(lvi)
        Next
    End Sub

    Private Sub btnHide_Click(sender As Object, e As EventArgs)
        If lvListings.SelectedItems.Count = 0 Then Return
        Dim itemId As Integer = Integer.Parse(lvListings.SelectedItems(0).Tag.ToString())
        If modDB.UpdateListingStatus(itemId, "Hidden") Then
            MessageBox.Show("Listing hidden.")
            LoadListings()
        End If
    End Sub

    Private Sub btnActivate_Click(sender As Object, e As EventArgs)
        If lvListings.SelectedItems.Count = 0 Then Return
        Dim itemId As Integer = Integer.Parse(lvListings.SelectedItems(0).Tag.ToString())
        If modDB.UpdateListingStatus(itemId, "Active") Then
            MessageBox.Show("Listing activated.")
            LoadListings()
        End If
    End Sub
End Class