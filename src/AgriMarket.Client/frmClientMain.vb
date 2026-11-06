Imports System.Text
Imports AgriMarket.Shared
Imports AgriMarket.[Shared].AgriMarket.[Shared]

Public Class frmClientMain
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UserRole As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UserName As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property UserID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    Private contentPanel As Panel
    Private menuLabels As New List(Of Label)
    Private _loggingOut As Boolean = False

    Public Sub New(role As String, name As String)
        InitializeComponent()
        UserRole = role
        UserName = name
    End Sub

    Private Sub frmClientMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "AgriMarket - " & UserRole & " Dashboard"
        Me.WindowState = FormWindowState.Maximized
        Me.BackColor = modTheme.cSurface
        Me.Font = modTheme.fBody

        BuildContentArea()
        BuildTopBar()
        LoadScreen("Home")
    End Sub

    Private Sub BuildTopBar()
        Dim topBar As New Panel()
        topBar.BackColor = modTheme.cOcean
        topBar.Dock = DockStyle.Top
        topBar.Height = 64
        Me.Controls.Add(topBar)

        Dim lblLogo As New Label()
        lblLogo.Text = "AgriMarket"
        lblLogo.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        lblLogo.ForeColor = modTheme.cWhite
        lblLogo.Location = New Point(20, 15)
        lblLogo.AutoSize = True
        topBar.Controls.Add(lblLogo)

        ' --- Right side: user name + role + Log Out ---
        Dim pnlUser As New FlowLayoutPanel()
        pnlUser.Dock = DockStyle.Right
        pnlUser.Width = 400
        pnlUser.FlowDirection = FlowDirection.LeftToRight
        pnlUser.WrapContents = False
        pnlUser.BackColor = Color.Transparent
        pnlUser.Padding = New Padding(0, 18, 20, 0)
        topBar.Controls.Add(pnlUser)

        Dim lblUser As New Label()
        lblUser.Text = UserName & " (" & UserRole & ")"
        lblUser.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblUser.ForeColor = modTheme.cWhite
        lblUser.AutoSize = True
        lblUser.Margin = New Padding(0, 2, 15, 0)
        pnlUser.Controls.Add(lblUser)

        Dim lnkLogout As New LinkLabel()
        lnkLogout.Text = "Log Out"
        lnkLogout.Font = New Font("Segoe UI", 9, FontStyle.Underline)
        lnkLogout.LinkColor = modTheme.cTeal
        lnkLogout.ActiveLinkColor = modTheme.cWhite
        lnkLogout.VisitedLinkColor = modTheme.cTeal
        lnkLogout.AutoSize = True
        lnkLogout.Margin = New Padding(0, 4, 0, 0)
        AddHandler lnkLogout.LinkClicked, AddressOf Logout_Click
        pnlUser.Controls.Add(lnkLogout)

        ' --- Menu items ---
        Dim pnlMenu As New FlowLayoutPanel()
        pnlMenu.Dock = DockStyle.Fill
        pnlMenu.WrapContents = False
        pnlMenu.FlowDirection = FlowDirection.LeftToRight
        pnlMenu.Padding = New Padding(180, 22, 0, 0)
        pnlMenu.BackColor = Color.Transparent
        topBar.Controls.Add(pnlMenu)
        pnlMenu.BringToFront()

        Dim menus As New List(Of String)
        menus.Add("Home")
        If UserRole = "Trader" Then
            menus.Add("My Stock")
            menus.Add("Orders")
        Else
            menus.Add("My Orders")
        End If
        menus.Add("Chats")
        menus.Add("Notifications")
        menus.Add("Profile")

        For Each menuText In menus
            Dim lblMenu As New Label()
            lblMenu.Text = menuText
            lblMenu.Font = New Font("Segoe UI", 10, FontStyle.Regular)
            lblMenu.ForeColor = modTheme.cWhite
            lblMenu.AutoSize = True
            lblMenu.Cursor = Cursors.Hand
            lblMenu.Margin = New Padding(0, 0, 30, 0)
            AddHandler lblMenu.Click, Sub(s, ev) LoadScreen(menuText)
            pnlMenu.Controls.Add(lblMenu)
            menuLabels.Add(lblMenu)
        Next
    End Sub

    Private Sub BuildContentArea()
        contentPanel = New Panel()
        contentPanel.Dock = DockStyle.Fill
        contentPanel.BackColor = modTheme.cSurface
        contentPanel.Padding = New Padding(20)
        Me.Controls.Add(contentPanel)
    End Sub

    Private Sub LoadScreen(menuText As String)
        contentPanel.Controls.Clear()

        For Each lbl In menuLabels
            lbl.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        Next
        For Each lbl In menuLabels
            If lbl.Text = menuText Then
                lbl.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            End If
        Next

        Select Case menuText
            Case "Home"
                If UserRole = "Trader" Then
                    Dim lblTitle As New Label()
                    lblTitle.Text = "Welcome, " & UserName
                    lblTitle.Font = modTheme.fTitle
                    lblTitle.ForeColor = modTheme.cOcean
                    lblTitle.Location = New Point(20, 20)
                    lblTitle.AutoSize = True
                    contentPanel.Controls.Add(lblTitle)
                Else
                    Dim browse As New ucBrowse()
                    browse.Token = Token
                    browse.BuyerID = UserID
                    contentPanel.Controls.Add(browse)
                End If

            Case "My Stock"
                Dim stockScreen As New ucMyStock()
                stockScreen.TraderID = UserID
                stockScreen.Token = Token
                contentPanel.Controls.Add(stockScreen)

            Case "Orders"
                Dim ordersScreen As New ucOrders()
                ordersScreen.TraderID = UserID
                ordersScreen.Token = Token
                contentPanel.Controls.Add(ordersScreen)

            Case "Notifications"
                Dim notifs As New ucNotifications()
                notifs.Token = Token
                notifs.UserID = UserID
                contentPanel.Controls.Add(notifs)

            Case "Chats"
                Dim chats As New ucChats()
                chats.Token = Token
                chats.UserID = UserID
                contentPanel.Controls.Add(chats)

            Case "My Orders"
                Dim buyerOrders As New ucBuyerOrders()
                buyerOrders.Token = Token
                buyerOrders.BuyerID = UserID
                contentPanel.Controls.Add(buyerOrders)

            Case Else
                Dim lblPlaceholder As New Label()
                lblPlaceholder.Text = menuText & " Screen Coming Soon"
                lblPlaceholder.Font = modTheme.fTitle
                lblPlaceholder.ForeColor = modTheme.cMuted
                lblPlaceholder.Location = New Point(20, 20)
                lblPlaceholder.AutoSize = True
                contentPanel.Controls.Add(lblPlaceholder)
        End Select
    End Sub

    ' ---------- LOGOUT ----------

    Private Async Sub Logout_Click(sender As Object, e As LinkLabelLinkClickedEventArgs)
        Dim confirm = MessageBox.Show("Are you sure you want to log out?", "Log Out",
                                      MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If confirm <> DialogResult.Yes Then Return

        ' 1. Send LOGOUT command (best effort)
        Try
            If frmConnect.Stream IsNot Nothing Then
                Dim msg As String = "LOGOUT" & vbCrLf
                Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
                Await frmConnect.Stream.WriteAsync(data, 0, data.Length)
            End If
        Catch
        End Try

        ' 2. Dispose the connection completely
        frmConnect.DisposeConnection()

        ' 3. Return to the Connect screen (fresh start)
        Dim connectForm As New frmConnect()
        connectForm.Show()
        Me.Close()
    End Sub


    Private Sub frmClientMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Best-effort cleanup if the user closes the window with the X
        Try
            If frmConnect.Stream IsNot Nothing Then
                Dim msg As String = "LOGOUT" & vbCrLf
                Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
                frmConnect.Stream.Write(data, 0, data.Length)
            End If
        Catch
        End Try
        frmConnect.DisposeConnection()
    End Sub


End Class