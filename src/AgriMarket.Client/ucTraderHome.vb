Imports AgriMarket.Shared
Imports System.Text

Public Class ucTraderHome
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Token As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderID As Integer

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderName As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderPhone As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property TraderLocation As String

    Private lblItems As New Label()
    Private lblPending As New Label()
    Private lblCompleted As New Label()
    Private lblSales As New Label()
    Private lvWatch As New ListView()

    Private Async Sub ucTraderHome_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = modTheme.cSurface
        Me.Dock = DockStyle.Fill
        BuildLayout()
        Await LoadData()
    End Sub

    Private Sub BuildLayout()
        ' 1. Profile Strip
        Dim pnlProfile As New Panel()
        pnlProfile.BackColor = modTheme.cWhite
        pnlProfile.BorderStyle = BorderStyle.FixedSingle
        pnlProfile.Location = New Point(0, 0)
        pnlProfile.Size = New Size(Me.Width - 10, 80)
        pnlProfile.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(pnlProfile)

        Dim lblName As New Label()
        lblName.Text = "Welcome back, " & TraderName
        lblName.Font = modTheme.fTitle
        lblName.ForeColor = modTheme.cOcean
        lblName.Location = New Point(15, 10)
        lblName.AutoSize = True
        pnlProfile.Controls.Add(lblName)

        Dim lblContact As New Label()
        lblContact.Text = TraderPhone & " | " & TraderLocation
        lblContact.Font = modTheme.fBody
        lblContact.ForeColor = modTheme.cMuted
        lblContact.Location = New Point(17, 45)
        lblContact.AutoSize = True
        pnlProfile.Controls.Add(lblContact)

        ' 2. Stat Cards
        Dim pnlStats As New FlowLayoutPanel()
        pnlStats.Location = New Point(0, 100)
        pnlStats.Size = New Size(Me.Width - 10, 120)
        pnlStats.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pnlStats.FlowDirection = FlowDirection.LeftToRight
        pnlStats.WrapContents = False
        Me.Controls.Add(pnlStats)

        pnlStats.Controls.Add(CreateStatCard("Items in Stock", lblItems, modTheme.cOcean))
        pnlStats.Controls.Add(CreateStatCard("Pending Orders", lblPending, modTheme.cWarning))
        pnlStats.Controls.Add(CreateStatCard("Completed Orders", lblCompleted, modTheme.cSuccess))
        pnlStats.Controls.Add(CreateStatCard("Sales This Month (UGX)", lblSales, modTheme.cTeal))

        ' 3. Batches to Watch
        Dim lblWatchTitle As New Label()
        lblWatchTitle.Text = "Batches to Watch"
        lblWatchTitle.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        lblWatchTitle.ForeColor = modTheme.cOcean
        lblWatchTitle.Location = New Point(0, 240)
        lblWatchTitle.AutoSize = True
        Me.Controls.Add(lblWatchTitle)

        lvWatch.View = View.Details
        lvWatch.FullRowSelect = True
        lvWatch.GridLines = False
        lvWatch.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        lvWatch.Location = New Point(0, 270)
        lvWatch.Size = New Size(Me.Width - 10, 200)
        lvWatch.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lvWatch.BackColor = modTheme.cWhite
        lvWatch.BorderStyle = BorderStyle.FixedSingle
        lvWatch.Columns.Add("Item", 150)
        lvWatch.Columns.Add("Batch No", 150)
        lvWatch.Columns.Add("Days Left", 100)
        Me.Controls.Add(lvWatch)
    End Sub

    Private Function CreateStatCard(title As String, lblValue As Label, accentColor As Color) As Panel
        Dim card As New Panel()
        card.BackColor = modTheme.cWhite
        card.Size = New Size(220, 100)
        card.Margin = New Padding(5)
        card.BorderStyle = BorderStyle.FixedSingle

        Dim accent As New Panel()
        accent.BackColor = accentColor
        accent.Size = New Size(8, 100)
        accent.Dock = DockStyle.Left
        card.Controls.Add(accent)

        lblValue.Text = "0"
        lblValue.Font = New Font("Segoe UI", 24, FontStyle.Bold)
        lblValue.ForeColor = accentColor
        lblValue.Location = New Point(20, 15)
        lblValue.AutoSize = True
        card.Controls.Add(lblValue)

        Dim lblTitle As New Label()
        lblTitle.Text = title
        lblTitle.Font = modTheme.fBody
        lblTitle.ForeColor = modTheme.cMuted
        lblTitle.Location = New Point(22, 60)
        lblTitle.AutoSize = True
        card.Controls.Add(lblTitle)

        Return card
    End Function

    Private Async Function LoadData() As Task
        If String.IsNullOrEmpty(Token) Then Return
        Try
            Dim msg As String = $"TRADER_HOME|8|{Token}|{TraderID}" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(msg)
            Await frmConnect.Stream.WriteAsync(data, 0, data.Length)

            Dim buffer(8192) As Byte
            Dim bytesRead As Integer = Await frmConnect.Stream.ReadAsync(buffer, 0, buffer.Length)
            Dim reply As String = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim()
            Dim lines() As String = reply.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)

            lvWatch.Items.Clear()

            For Each line As String In lines
                Dim cleanLine As String = line.Trim()
                If cleanLine.StartsWith("OK|") Then Continue For
                If cleanLine = "END" Then Exit For
                Dim p() As String = cleanLine.Split("|"c)

                If p(0) = "STATS" AndAlso p.Length >= 5 Then
                    lblItems.Text = p(1)
                    lblPending.Text = p(2)
                    lblCompleted.Text = p(3)
                    lblSales.Text = p(4)
                ElseIf p(0) = "WATCH" AndAlso p.Length >= 4 Then
                    Dim lvi As New ListViewItem(p(1)) ' Item Name
                    lvi.SubItems.Add(p(2))            ' Batch No
                    lvi.SubItems.Add(p(3) & " days")  ' Days Left
                    If Integer.Parse(p(3)) <= 0 Then
                        lvi.ForeColor = modTheme.cDanger
                    Else
                        lvi.ForeColor = modTheme.cWarning
                    End If
                    lvWatch.Items.Add(lvi)
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Error loading home data: " & ex.Message)
        End Try
    End Function
End Class