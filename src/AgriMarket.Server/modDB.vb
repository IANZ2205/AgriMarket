Imports Microsoft.Data.Sqlite
Imports System.IO

Public Module modDB
    Public ReadOnly DbPath As String = Path.Combine(Application.StartupPath, "agrimarket.db")
    Public ReadOnly ConnString As String = $"Data Source={DbPath};Default Timeout=30;Pooling=False;Foreign Keys=False;"

    Public Function InitializeDatabase() As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Using cmd As New SqliteCommand("PRAGMA journal_mode=WAL;", conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlUsers As String = "
                    CREATE TABLE IF NOT EXISTS Users (
                        UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Email TEXT(100) UNIQUE NOT NULL,
                        PasswordHash TEXT(64) NOT NULL,
                        Salt TEXT(32) NOT NULL,
                        Role TEXT(12) NOT NULL,
                        DisplayName TEXT(50) NOT NULL,
                        FirstName TEXT(40),
                        LastName TEXT(40),
                        Phone TEXT(20),
                        LocationID INTEGER,
                        LocationDetail TEXT(120),
                        ProfilePic TEXT(60),
                        SecurityQuestionID INTEGER,
                        SecurityAnswerHash TEXT(64),
                        Status TEXT(12) DEFAULT 'Pending',
                        FailedLogins INTEGER DEFAULT 0,
                        LockedUntil TEXT,
                        ApprovedBy INTEGER,
                        ApprovedAt TEXT,
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        LastLoginAt TEXT
                    );"
                Using cmd As New SqliteCommand(sqlUsers, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlItems As String = "
                    CREATE TABLE IF NOT EXISTS Items (
                        ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                        TraderID INTEGER NOT NULL,
                        Category TEXT(40) NOT NULL,
                        ItemCode TEXT(5) NOT NULL,
                        EnglishName TEXT(60) NOT NULL,
                        Description TEXT,
                        UnitPrice NUMERIC NOT NULL,
                        Unit TEXT(10) NOT NULL,
                        LowStockLevel INTEGER DEFAULT 10,
                        Status TEXT(10) DEFAULT 'Active',
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(TraderID) REFERENCES Users(UserID)
                    );"
                Using cmd As New SqliteCommand(sqlItems, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlBatches As String = "
                    CREATE TABLE IF NOT EXISTS Batches (
                        BatchID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemID INTEGER NOT NULL,
                        BatchNo TEXT(25) UNIQUE NOT NULL,
                        QtyAdded REAL NOT NULL,
                        QtyRemaining REAL NOT NULL,
                        QtyReserved REAL DEFAULT 0,
                        ShelfLifeDays INTEGER NOT NULL,
                        DateAdded TEXT DEFAULT CURRENT_TIMESTAMP,
                        ExpiryDate TEXT NOT NULL,
                        Status TEXT(10) DEFAULT 'Active',
                        FOREIGN KEY(ItemID) REFERENCES Items(ItemID)
                    );"
                Using cmd As New SqliteCommand(sqlBatches, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlLocalNames As String = "
                    CREATE TABLE IF NOT EXISTS ItemLocalNames (
                        ItemID INTEGER NOT NULL,
                        Language TEXT(20) NOT NULL,
                        LocalName TEXT(60) NOT NULL,
                        PRIMARY KEY (ItemID, Language),
                        FOREIGN KEY(ItemID) REFERENCES Items(ItemID)
                    );"
                Using cmd As New SqliteCommand(sqlLocalNames, conn)
                    cmd.ExecuteNonQuery()
                End Using

                ' ServerLog table (required by AdjustBatch)
                Dim sqlLog As String = "
                    CREATE TABLE IF NOT EXISTS ServerLog (
                        LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                        LoggedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        Level TEXT(8),
                        UserID INTEGER,
                        EventType TEXT(20),
                        Details TEXT(250)
                    );"
                Using cmd As New SqliteCommand(sqlLog, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlOrders As String = "
                    CREATE TABLE IF NOT EXISTS Orders (
                        OrderID INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderNo TEXT(15) UNIQUE NOT NULL,
                        ItemID INTEGER NOT NULL,
                        TraderID INTEGER NOT NULL,
                        BuyerID INTEGER NOT NULL,
                        Qty REAL NOT NULL,
                        UnitPriceAtOrder NUMERIC NOT NULL,
                        TotalAmount NUMERIC NOT NULL,
                        NeededBy TEXT,
                        DeliveryMode TEXT(10),
                        DeliveryNote TEXT(200),
                        BuyerNote TEXT(200),
                        Status TEXT(10) DEFAULT 'Pending',
                        ReasonID INTEGER,
                        ReasonComment TEXT(200),
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        ConfirmedAt TEXT,
                        ClosedAt TEXT
                    );"
                Using cmd As New SqliteCommand(sqlOrders, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlOrderEvents As String = "
                    CREATE TABLE IF NOT EXISTS OrderEvents (
                        EventID INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderID INTEGER NOT NULL,
                        Status TEXT(10),
                        ByUserID INTEGER,
                        EventAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        Comment TEXT(200)
                    );"
                Using cmd As New SqliteCommand(sqlOrderEvents, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlAlloc As String = "
                    CREATE TABLE IF NOT EXISTS OrderAllocations (
                        AllocID INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderID INTEGER NOT NULL,
                        BatchID INTEGER NOT NULL,
                        Qty REAL NOT NULL
                    );"
                Using cmd As New SqliteCommand(sqlAlloc, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlNotif As String = "
                    CREATE TABLE IF NOT EXISTS Notifications (
                        NotifID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserID INTEGER NOT NULL,
                        NotifType TEXT(20),
                        Title TEXT(80),
                        Body TEXT(250),
                        RelatedType TEXT(10),
                        RelatedID INTEGER,
                        IsRead INTEGER DEFAULT 0,
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
                    );"
                Using cmd As New SqliteCommand(sqlNotif, conn)
                    cmd.ExecuteNonQuery()
                End Using


                Dim sqlConv As String = "
                    CREATE TABLE IF NOT EXISTS Conversations (
                        ConvID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserLowID INTEGER NOT NULL,
                        UserHighID INTEGER NOT NULL,
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        LastMessageAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        UNIQUE (UserLowID, UserHighID)
                    );"
                Using cmd As New SqliteCommand(sqlConv, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlMsgs As String = "
                    CREATE TABLE IF NOT EXISTS Messages (
                        MessageID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ConvID INTEGER NOT NULL,
                        SenderID INTEGER,
                        MsgType TEXT(10) DEFAULT 'Text',
                        Body TEXT,
                        FileName TEXT(60),
                        OriginalName TEXT(100),
                        FileSize INTEGER,
                        LocationText TEXT(200),
                        ItemID INTEGER,
                        OrderID INTEGER,
                        SentAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        DeliveredAt TEXT,
                        ReadAt TEXT
                    );"
                Using cmd As New SqliteCommand(sqlMsgs, conn)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlSessions As String = "
                    CREATE TABLE IF NOT EXISTS Sessions (
                        SessionID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserID INTEGER NOT NULL,
                        Token TEXT(40),
                        IPAddress TEXT(45),
                        LoginAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        LastActivityAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        LogoutAt TEXT,
                        EndReason TEXT(15)
                    );"
                Using cmd As New SqliteCommand(sqlSessions, conn)
                    cmd.ExecuteNonQuery()
                End Using

                SeedAdmin(conn)
                Return True
            End Using
        Catch ex As Exception
            MessageBox.Show("Database Error: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub SeedAdmin(conn As SqliteConnection)
        Dim checkSql As String = "SELECT COUNT(*) FROM Users"
        Using cmd As New SqliteCommand(checkSql, conn)
            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then Return
        End Using
        Dim insertSql As String = "INSERT INTO Users (UserID, Email, PasswordHash, Salt, Role, DisplayName, Status) VALUES (1, @Email, @Hash, @Salt, 'Admin', 'System Admin', 'Active')"
        Dim salt As String = modSecurity.GenerateSalt()
        Dim hash As String = modSecurity.HashPassword("admin123", salt)
        Using cmd As New SqliteCommand(insertSql, conn)
            cmd.Parameters.AddWithValue("@Email", "admin@agrimarket.com")
            cmd.Parameters.AddWithValue("@Hash", hash)
            cmd.Parameters.AddWithValue("@Salt", salt)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function AuthenticateUser(email As String, password As String) As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT PasswordHash, Salt FROM Users WHERE Email = @Email AND Status = 'Active'"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@Email", email)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim storedHash As String = reader("PasswordHash").ToString()
                            Dim salt As String = reader("Salt").ToString()
                            Return modSecurity.HashPassword(password, salt) = storedHash
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Login Error: " & ex.Message)
        End Try
        Return False
    End Function

    Public Function GetUserDetails(email As String, password As String) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT UserID, Role, DisplayName, PasswordHash, Salt FROM Users WHERE Email = @Email AND Status = 'Active'"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@Email", email)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim storedHash As String = reader("PasswordHash").ToString()
                            Dim salt As String = reader("Salt").ToString()
                            If modSecurity.HashPassword(password, salt) = storedHash Then
                                dt.Columns.Add("UserID", GetType(Integer))
                                dt.Columns.Add("Role", GetType(String))
                                dt.Columns.Add("DisplayName", GetType(String))
                                dt.Rows.Add(Convert.ToInt32(reader("UserID")), reader("Role").ToString(), reader("DisplayName").ToString())
                            End If
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return dt
    End Function

    Public Function RegisterUser(role As String, displayName As String, firstName As String, lastName As String, email As String, phone As String, district As String, password As String) As Integer
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim checkSql As String = "SELECT COUNT(*) FROM Users WHERE Email = @email"
                Using cmd As New SqliteCommand(checkSql, conn)
                    cmd.Parameters.AddWithValue("@email", email)
                    If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then Return -1
                End Using
                Dim salt As String = modSecurity.GenerateSalt()
                Dim hash As String = modSecurity.HashPassword(password, salt)
                Dim sql As String = "INSERT INTO Users (Email, PasswordHash, Salt, Role, DisplayName, FirstName, LastName, Phone, Status) VALUES (@email, @hash, @salt, @role, @name, @fname, @lname, @phone, 'Active'); SELECT last_insert_rowid();"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@email", email)
                    cmd.Parameters.AddWithValue("@hash", hash)
                    cmd.Parameters.AddWithValue("@salt", salt)
                    cmd.Parameters.AddWithValue("@role", role)
                    cmd.Parameters.AddWithValue("@name", displayName)
                    cmd.Parameters.AddWithValue("@fname", firstName)
                    cmd.Parameters.AddWithValue("@lname", lastName)
                    cmd.Parameters.AddWithValue("@phone", phone)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Register Error: " & ex.Message)
            Return -2
        End Try
    End Function

    Public Function AddItem(traderId As Integer, category As String, itemCode As String, name As String, description As String, unitPrice As Decimal, unit As String, lowStock As Integer, qty As Double, shelfLifeDays As Integer, localNamesData As String) As String
        Dim attempts As Integer = 0
        While attempts < 10
            Try
                Using conn As New SqliteConnection(ConnString)
                    conn.Open()
                    Using transaction = conn.BeginTransaction()
                        Try
                            Dim sqlItem As String = "INSERT INTO Items (TraderID, Category, ItemCode, EnglishName, Description, UnitPrice, Unit, LowStockLevel) VALUES (@tid, @cat, @code, @name, @desc, @price, @unit, @low); SELECT last_insert_rowid();"
                            Dim itemId As Integer
                            Using cmd As New SqliteCommand(sqlItem, conn, transaction)
                                cmd.Parameters.AddWithValue("@tid", traderId)
                                cmd.Parameters.AddWithValue("@cat", category)
                                cmd.Parameters.AddWithValue("@code", itemCode)
                                cmd.Parameters.AddWithValue("@name", name)
                                cmd.Parameters.AddWithValue("@desc", description)
                                cmd.Parameters.AddWithValue("@price", unitPrice)
                                cmd.Parameters.AddWithValue("@unit", unit)
                                cmd.Parameters.AddWithValue("@low", lowStock)
                                itemId = Convert.ToInt32(cmd.ExecuteScalar())
                            End Using

                            If Not String.IsNullOrEmpty(localNamesData) Then
                                Dim names() As String = localNamesData.Split("|"c)
                                For Each n In names
                                    Dim parts() As String = n.Split(":"c)
                                    If parts.Length = 2 Then
                                        Dim sqlLocal As String = "INSERT INTO ItemLocalNames (ItemID, Language, LocalName) VALUES (@id, @lang, @name)"
                                        Using cmd As New SqliteCommand(sqlLocal, conn, transaction)
                                            cmd.Parameters.AddWithValue("@id", itemId)
                                            cmd.Parameters.AddWithValue("@lang", parts(0))
                                            cmd.Parameters.AddWithValue("@name", parts(1))
                                            cmd.ExecuteNonQuery()
                                        End Using
                                    End If
                                Next
                            End If

                            Dim code As String = itemCode.ToUpper()
                            Dim datePart As String = DateTime.Now.ToString("yyyyMMdd")
                            Dim batchPrefix As String = $"{code}-{datePart}-"
                            Dim countSql As String = "SELECT COUNT(*) FROM Batches WHERE BatchNo LIKE @prefix || '%'"
                            Dim batchCount As Integer
                            Using cmd As New SqliteCommand(countSql, conn, transaction)
                                cmd.Parameters.AddWithValue("@prefix", batchPrefix)
                                batchCount = Convert.ToInt32(cmd.ExecuteScalar())
                            End Using
                            Dim batchNo As String = $"{batchPrefix}{(batchCount + 1).ToString("D2")}"
                            Dim expiryDate As String = DateTime.Now.AddDays(shelfLifeDays).ToString("yyyy-MM-dd HH:mm:ss")
                            Dim sqlBatch As String = "INSERT INTO Batches (ItemID, BatchNo, QtyAdded, QtyRemaining, ShelfLifeDays, ExpiryDate) VALUES (@id, @bno, @qty, @qty, @days, @exp)"
                            Using cmd As New SqliteCommand(sqlBatch, conn, transaction)
                                cmd.Parameters.AddWithValue("@id", itemId)
                                cmd.Parameters.AddWithValue("@bno", batchNo)
                                cmd.Parameters.AddWithValue("@qty", qty)
                                cmd.Parameters.AddWithValue("@days", shelfLifeDays)
                                cmd.Parameters.AddWithValue("@exp", expiryDate)
                                cmd.ExecuteNonQuery()
                            End Using
                            transaction.Commit()
                            Return batchNo
                        Catch ex As Exception
                            transaction.Rollback()
                            Throw
                        End Try
                    End Using
                End Using
            Catch ex As Exception
                If ex.Message.Contains("locked") AndAlso attempts < 9 Then
                    attempts += 1
                    Threading.Thread.Sleep(200)
                Else
                    MessageBox.Show("Error saving item: " & ex.Message)
                    Return ""
                End If
            End Try
        End While
        Return ""
    End Function

    Public Function GetTraderStock(traderId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        i.ItemID,
                        i.EnglishName,
                        i.Category,
                        IFNULL(SUM(CASE WHEN b.ExpiryDate > datetime('now') THEN (b.QtyRemaining - b.QtyReserved) ELSE 0 END), 0) AS AvailableQty,
                        i.Unit,
                        i.UnitPrice,
                        COUNT(CASE WHEN b.ExpiryDate > datetime('now') THEN b.BatchID END) AS BatchCount
                    FROM Items i
                    LEFT JOIN Batches b ON i.ItemID = b.ItemID AND b.Status = 'Active'
                    WHERE i.TraderID = @tid AND i.Status = 'Active'
                    GROUP BY i.ItemID, i.EnglishName, i.Category, i.Unit, i.UnitPrice
                    ORDER BY i.EnglishName ASC"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@tid", traderId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching stock: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function TopUpItem(itemId As Integer, qty As Double, shelfLifeDays As Integer, newPrice As Decimal) As String
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Using transaction = conn.BeginTransaction()
                    Try
                        Dim code As String = ""
                        Dim sqlGet As String = "SELECT ItemCode FROM Items WHERE ItemID = @id AND Status = 'Active'"
                        Using cmd As New SqliteCommand(sqlGet, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", itemId)
                            Dim result = cmd.ExecuteScalar()
                            If result Is Nothing Then
                                transaction.Rollback()
                                Return ""
                            End If
                            code = result.ToString()
                        End Using

                        Dim datePart As String = DateTime.Now.ToString("yyyyMMdd")
                        Dim batchPrefix As String = $"{code.ToUpper()}-{datePart}-"
                        Dim countSql As String = "SELECT COUNT(*) FROM Batches WHERE BatchNo LIKE @prefix || '%'"
                        Dim batchCount As Integer
                        Using cmd As New SqliteCommand(countSql, conn, transaction)
                            cmd.Parameters.AddWithValue("@prefix", batchPrefix)
                            batchCount = Convert.ToInt32(cmd.ExecuteScalar())
                        End Using
                        Dim batchNo As String = $"{batchPrefix}{(batchCount + 1).ToString("D2")}"
                        Dim expiryDate As String = DateTime.Now.AddDays(shelfLifeDays).ToString("yyyy-MM-dd HH:mm:ss")

                        Dim sqlBatch As String = "INSERT INTO Batches (ItemID, BatchNo, QtyAdded, QtyRemaining, ShelfLifeDays, ExpiryDate) VALUES (@id, @bno, @qty, @qty, @days, @exp)"
                        Using cmd As New SqliteCommand(sqlBatch, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", itemId)
                            cmd.Parameters.AddWithValue("@bno", batchNo)
                            cmd.Parameters.AddWithValue("@qty", qty)
                            cmd.Parameters.AddWithValue("@days", shelfLifeDays)
                            cmd.Parameters.AddWithValue("@exp", expiryDate)
                            cmd.ExecuteNonQuery()
                        End Using

                        If newPrice > 0 Then
                            Dim sqlPrice As String = "UPDATE Items SET UnitPrice = @price WHERE ItemID = @id"
                            Using cmd As New SqliteCommand(sqlPrice, conn, transaction)
                                cmd.Parameters.AddWithValue("@price", newPrice)
                                cmd.Parameters.AddWithValue("@id", itemId)
                                cmd.ExecuteNonQuery()
                            End Using
                        End If

                        transaction.Commit()
                        Return batchNo
                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error topping up: " & ex.Message)
            Return ""
        End Try
    End Function

    Public Function GetItemBatches(itemId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        BatchID,
                        BatchNo,
                        DateAdded,
                        ExpiryDate,
                        QtyRemaining,
                        QtyReserved,
                        Status,
                        CAST(julianday(ExpiryDate) - julianday('now') AS INTEGER) AS DaysLeft
                    FROM Batches 
                    WHERE ItemID = @id AND Status = 'Active'
                    ORDER BY ExpiryDate ASC"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@id", itemId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching batches: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function UpdateItem(itemId As Integer, name As String, description As String, unitPrice As Decimal, lowStock As Integer) As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Items SET EnglishName = @name, Description = @desc, UnitPrice = @price, LowStockLevel = @low WHERE ItemID = @id"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@name", name)
                    cmd.Parameters.AddWithValue("@desc", description)
                    cmd.Parameters.AddWithValue("@price", unitPrice)
                    cmd.Parameters.AddWithValue("@low", lowStock)
                    cmd.Parameters.AddWithValue("@id", itemId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error updating item: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function AdjustBatch(batchId As Integer, newQty As Double, newShelfLife As Integer, reason As String) As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim dateAdded As String = ""
                Dim sqlGet As String = "SELECT DateAdded FROM Batches WHERE BatchID = @id"
                Using cmd As New SqliteCommand(sqlGet, conn)
                    cmd.Parameters.AddWithValue("@id", batchId)
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing Then dateAdded = result.ToString()
                End Using

                Dim expiryDate As String = ""
                If Not String.IsNullOrEmpty(dateAdded) Then
                    Dim dtAdded As DateTime = DateTime.Parse(dateAdded)
                    expiryDate = dtAdded.AddDays(newShelfLife).ToString("yyyy-MM-dd HH:mm:ss")
                End If

                Dim sql As String = "UPDATE Batches SET QtyRemaining = @qty, ShelfLifeDays = @days, ExpiryDate = @exp WHERE BatchID = @id"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@qty", newQty)
                    cmd.Parameters.AddWithValue("@days", newShelfLife)
                    cmd.Parameters.AddWithValue("@exp", expiryDate)
                    cmd.Parameters.AddWithValue("@id", batchId)
                    cmd.ExecuteNonQuery()
                End Using

                Dim sqlLog As String = "INSERT INTO ServerLog (LoggedAt, Level, EventType, Details) VALUES (datetime('now'), 'Info', 'BatchAdjust', @details)"
                Using cmd As New SqliteCommand(sqlLog, conn)
                    cmd.Parameters.AddWithValue("@details", $"Batch {batchId} adjusted: Qty={newQty}, Shelf={newShelfLife}, Reason={reason}")
                    cmd.ExecuteNonQuery()
                End Using

                Return True
            End Using
        Catch ex As Exception
            MessageBox.Show("Error adjusting batch: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function GetBrowseListings(category As String) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        i.ItemID,
                        i.EnglishName,
                        i.Category,
                        i.Unit,
                        i.UnitPrice,
                        u.DisplayName AS TraderName,
                        u.UserID AS TraderID,
                        IFNULL(SUM(CASE WHEN b.ExpiryDate > datetime('now') THEN (b.QtyRemaining - b.QtyReserved) ELSE 0 END), 0) AS AvailableQty,
                        MIN(CASE WHEN b.ExpiryDate > datetime('now') THEN CAST(julianday(b.ExpiryDate) - julianday('now') AS INTEGER) END) AS DaysLeft
                    FROM Items i
                    JOIN Users u ON i.TraderID = u.UserID
                    LEFT JOIN Batches b ON i.ItemID = b.ItemID AND b.Status = 'Active'
                    WHERE i.Status = 'Active' AND u.Status = 'Active'
                    GROUP BY i.ItemID, i.EnglishName, i.Category, i.Unit, i.UnitPrice, u.DisplayName, u.UserID
                    HAVING AvailableQty > 0"
                If Not String.IsNullOrEmpty(category) AndAlso category <> "All" Then
                    sql &= " AND i.Category = @cat"
                End If
                sql &= " ORDER BY i.EnglishName ASC"

                Using cmd As New SqliteCommand(sql, conn)
                    If Not String.IsNullOrEmpty(category) AndAlso category <> "All" Then
                        cmd.Parameters.AddWithValue("@cat", category)
                    End If
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching listings: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetItemDetail(itemId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        i.ItemID,
                        i.EnglishName,
                        i.Category,
                        i.Description,
                        i.Unit,
                        i.UnitPrice,
                        u.DisplayName AS TraderName,
                        u.Phone AS TraderPhone,
                        u.UserID AS TraderID,
                        u.LocationDetail AS TraderLocation,
                        IFNULL(SUM(CASE WHEN b.ExpiryDate > datetime('now') THEN (b.QtyRemaining - b.QtyReserved) ELSE 0 END), 0) AS AvailableQty,
                        MIN(CASE WHEN b.ExpiryDate > datetime('now') THEN CAST(julianday(b.ExpiryDate) - julianday('now') AS INTEGER) END) AS DaysLeft,
                        (SELECT LocalName FROM ItemLocalNames WHERE ItemID = i.ItemID AND Language = 'Luganda') AS LocalName
                    FROM Items i
                    JOIN Users u ON i.TraderID = u.UserID
                    LEFT JOIN Batches b ON i.ItemID = b.ItemID AND b.Status = 'Active'
                    WHERE i.ItemID = @id
                    GROUP BY i.ItemID, i.EnglishName, i.Category, i.Description, i.Unit, i.UnitPrice, 
                             u.DisplayName, u.Phone, u.UserID, u.LocationDetail"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@id", itemId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching item: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function PlaceOrder(buyerId As Integer, itemId As Integer, qty As Double, neededBy As String, delivery As String, note As String, unitPrice As Decimal, traderId As Integer) As String
        Dim attempts As Integer = 0
        While attempts < 10
            Try
                Using conn As New SqliteConnection(ConnString)
                    conn.Open()
                    Using transaction = conn.BeginTransaction()
                        Try
                            Dim orderNo As String = "ORD-" & DateTime.Now.ToString("yyyyMMdd") & "-" & (DateTime.Now.Ticks Mod 10000).ToString("D4")
                            Dim total As Decimal = CDec(qty) * unitPrice
                            Dim sql As String = "INSERT INTO Orders (OrderNo, ItemID, TraderID, BuyerID, Qty, UnitPriceAtOrder, TotalAmount, NeededBy, DeliveryMode, BuyerNote, Status, CreatedAt) VALUES (@no, @item, @trader, @buyer, @qty, @price, @total, @needed, @delivery, @note, 'Pending', datetime('now')); SELECT last_insert_rowid();"
                            Dim orderId As Integer
                            Using cmd As New SqliteCommand(sql, conn, transaction)
                                cmd.Parameters.AddWithValue("@no", orderNo)
                                cmd.Parameters.AddWithValue("@item", itemId)
                                cmd.Parameters.AddWithValue("@trader", traderId)
                                cmd.Parameters.AddWithValue("@buyer", buyerId)
                                cmd.Parameters.AddWithValue("@qty", qty)
                                cmd.Parameters.AddWithValue("@price", unitPrice)
                                cmd.Parameters.AddWithValue("@total", total)
                                cmd.Parameters.AddWithValue("@needed", neededBy)
                                cmd.Parameters.AddWithValue("@delivery", delivery)
                                cmd.Parameters.AddWithValue("@note", note)
                                orderId = Convert.ToInt32(cmd.ExecuteScalar())
                            End Using

                            Dim sqlEvent As String = "INSERT INTO OrderEvents (OrderID, Status, ByUserID, EventAt, Comment) VALUES (@id, 'Pending', @by, datetime('now'), 'Order placed')"
                            Using cmd As New SqliteCommand(sqlEvent, conn, transaction)
                                cmd.Parameters.AddWithValue("@id", orderId)
                                cmd.Parameters.AddWithValue("@by", buyerId)
                                cmd.ExecuteNonQuery()
                            End Using

                            Dim notifSql As String = "INSERT INTO Notifications (UserID, NotifType, Title, Body, RelatedType, RelatedID, CreatedAt) VALUES (@uid, 'NEW_ORDER', 'New Order', 'You have a new order: " & orderNo & "', 'Order', @oid, datetime('now'))"
                            Using cmd As New SqliteCommand(notifSql, conn, transaction)
                                cmd.Parameters.AddWithValue("@uid", traderId)
                                cmd.Parameters.AddWithValue("@oid", orderId)
                                cmd.ExecuteNonQuery()
                            End Using

                            transaction.Commit()
                            Return orderNo
                        Catch ex As Exception
                            transaction.Rollback()
                            Throw
                        End Try
                    End Using
                End Using
            Catch ex As Exception
                If ex.Message.Contains("locked") AndAlso attempts < 9 Then
                    attempts += 1
                    Threading.Thread.Sleep(200)
                Else
                    MessageBox.Show("Error placing order: " & ex.Message)
                    Return ""
                End If
            End Try
        End While
        Return ""
    End Function


    Public Function GetTraderOrders(traderId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        o.OrderID,
                        o.OrderNo,
                        o.Status,
                        o.Qty,
                        o.TotalAmount,
                        o.CreatedAt,
                        i.EnglishName AS ItemName,
                        u.DisplayName AS BuyerName
                    FROM Orders o
                    JOIN Items i ON o.ItemID = i.ItemID
                    JOIN Users u ON o.BuyerID = u.UserID
                    WHERE o.TraderID = @tid
                    ORDER BY o.CreatedAt DESC"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@tid", traderId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching orders: " & ex.Message)
        End Try
        Return dt
    End Function


    Public Function ConfirmOrder(orderId As Integer) As String
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Using transaction = conn.BeginTransaction()
                    Try
                        ' 1. Get order details
                        Dim itemId As Integer = 0
                        Dim qty As Double = 0
                        Dim sqlGet As String = "SELECT ItemID, Qty FROM Orders WHERE OrderID = @id AND Status = 'Pending'"
                        Using cmd As New SqliteCommand(sqlGet, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            Using reader As SqliteDataReader = cmd.ExecuteReader()
                                If Not reader.Read() Then
                                    transaction.Rollback()
                                    Return "ERR|INVALID_STATE|Order not found or not pending"
                                End If
                                itemId = Convert.ToInt32(reader("ItemID"))
                                qty = Convert.ToDouble(reader("Qty"))
                            End Using
                        End Using

                        ' 2. Check available stock
                        Dim available As Double = 0
                        Dim sqlAvail As String = "SELECT IFNULL(SUM(QtyRemaining - QtyReserved), 0) FROM Batches WHERE ItemID = @id AND Status = 'Active' AND ExpiryDate > datetime('now')"
                        Using cmd As New SqliteCommand(sqlAvail, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", itemId)
                            available = Convert.ToDouble(cmd.ExecuteScalar())
                        End Using

                        If available < qty Then
                            transaction.Rollback()
                            Return $"ERR|NOT_ENOUGH_STOCK|Only {available} available"
                        End If

                        ' 3. Allocate stock using FEFO (First Expired, First Out)
                        Dim batches As New List(Of Tuple(Of Integer, Double))
                        Dim sqlBatches As String = "SELECT BatchID, QtyRemaining - QtyReserved FROM Batches WHERE ItemID = @id AND Status = 'Active' AND ExpiryDate > datetime('now') AND (QtyRemaining - QtyReserved) > 0 ORDER BY ExpiryDate ASC"
                        Using cmd As New SqliteCommand(sqlBatches, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", itemId)
                            Using reader As SqliteDataReader = cmd.ExecuteReader()
                                While reader.Read()
                                    batches.Add(New Tuple(Of Integer, Double)(reader.GetInt32(0), reader.GetDouble(1)))
                                End While
                            End Using
                        End Using

                        Dim remainingToAllocate As Double = qty
                        For Each b In batches
                            If remainingToAllocate <= 0 Then Exit For
                            Dim allocQty As Double = Math.Min(b.Item2, remainingToAllocate)

                            ' Reserve the quantity
                            Dim sqlReserve As String = "UPDATE Batches SET QtyReserved = QtyReserved + @qty WHERE BatchID = @id"
                            Using cmd As New SqliteCommand(sqlReserve, conn, transaction)
                                cmd.Parameters.AddWithValue("@qty", allocQty)
                                cmd.Parameters.AddWithValue("@id", b.Item1)
                                cmd.ExecuteNonQuery()
                            End Using

                            ' Log the allocation
                            Dim sqlAlloc As String = "INSERT INTO OrderAllocations (OrderID, BatchID, Qty) VALUES (@oid, @bid, @qty)"
                            Using cmd As New SqliteCommand(sqlAlloc, conn, transaction)
                                cmd.Parameters.AddWithValue("@oid", orderId)
                                cmd.Parameters.AddWithValue("@bid", b.Item1)
                                cmd.Parameters.AddWithValue("@qty", allocQty)
                                cmd.ExecuteNonQuery()
                            End Using

                            remainingToAllocate -= allocQty
                        Next

                        ' 4. Update order status
                        Dim sqlUpdate As String = "UPDATE Orders SET Status = 'Confirmed', ConfirmedAt = datetime('now') WHERE OrderID = @id"
                        Using cmd As New SqliteCommand(sqlUpdate, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' 5. Log event
                        Dim sqlEvent As String = "INSERT INTO OrderEvents (OrderID, Status, EventAt, Comment) VALUES (@id, 'Confirmed', datetime('now'), 'Order confirmed')"
                        Using cmd As New SqliteCommand(sqlEvent, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            cmd.ExecuteNonQuery()
                        End Using

                        transaction.Commit()
                        Return "OK"
                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error confirming order: " & ex.Message)
            Return "ERR|VALIDATION|Database error"
        End Try
    End Function

    Public Function RejectOrder(orderId As Integer, reason As String) As String
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Orders SET Status = 'Rejected', ReasonComment = @reason, ClosedAt = datetime('now') WHERE OrderID = @id AND Status = 'Pending'"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@reason", reason)
                    cmd.Parameters.AddWithValue("@id", orderId)
                    If cmd.ExecuteNonQuery() = 0 Then Return "ERR|INVALID_STATE|Order not found or not pending"
                End Using

                Dim sqlEvent As String = "INSERT INTO OrderEvents (OrderID, Status, EventAt, Comment) VALUES (@id, 'Rejected', datetime('now'), @comment)"
                Using cmd As New SqliteCommand(sqlEvent, conn)
                    cmd.Parameters.AddWithValue("@id", orderId)
                    cmd.Parameters.AddWithValue("@comment", "Rejected: " & reason)
                    cmd.ExecuteNonQuery()
                End Using

                Return "OK"
            End Using
        Catch ex As Exception
            MessageBox.Show("Error rejecting order: " & ex.Message)
            Return "ERR|VALIDATION|Database error"
        End Try
    End Function

    Public Function CompleteOrder(orderId As Integer) As String
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Using transaction = conn.BeginTransaction()
                    Try
                        ' 1. Get allocations
                        Dim allocations As New List(Of Tuple(Of Integer, Double))
                        Dim sqlAlloc As String = "SELECT BatchID, Qty FROM OrderAllocations WHERE OrderID = @id"
                        Using cmd As New SqliteCommand(sqlAlloc, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            Using reader As SqliteDataReader = cmd.ExecuteReader()
                                While reader.Read()
                                    allocations.Add(New Tuple(Of Integer, Double)(reader.GetInt32(0), reader.GetDouble(1)))
                                End While
                            End Using
                        End Using

                        ' 2. Deduct from batches
                        For Each a In allocations
                            Dim sqlDeduct As String = "UPDATE Batches SET QtyRemaining = QtyRemaining - @qty, QtyReserved = QtyReserved - @qty WHERE BatchID = @id"
                            Using cmd As New SqliteCommand(sqlDeduct, conn, transaction)
                                cmd.Parameters.AddWithValue("@qty", a.Item2)
                                cmd.Parameters.AddWithValue("@id", a.Item1)
                                cmd.ExecuteNonQuery()
                            End Using
                        Next

                        ' 3. Update order status
                        Dim sqlUpdate As String = "UPDATE Orders SET Status = 'Completed', ClosedAt = datetime('now') WHERE OrderID = @id"
                        Using cmd As New SqliteCommand(sqlUpdate, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' 4. Log event
                        Dim sqlEvent As String = "INSERT INTO OrderEvents (OrderID, Status, EventAt, Comment) VALUES (@id, 'Completed', datetime('now'), 'Order completed')"
                        Using cmd As New SqliteCommand(sqlEvent, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            cmd.ExecuteNonQuery()
                        End Using

                        transaction.Commit()
                        Return "OK"
                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error completing order: " & ex.Message)
            Return "ERR|VALIDATION|Database error"
        End Try
    End Function


    Public Function GetNotifications(userId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT NotifID, NotifType, Title, Body, RelatedType, RelatedID, IsRead, CreatedAt FROM Notifications WHERE UserID = @uid ORDER BY CreatedAt DESC"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@uid", userId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching notifications: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Sub MarkNotificationRead(notifId As Integer)
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Notifications SET IsRead = 1 WHERE NotifID = @id"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@id", notifId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error marking notification read: " & ex.Message)
        End Try
    End Sub

    Public Function GetUnreadNotificationCount(userId As Integer) As Integer
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT COUNT(*) FROM Notifications WHERE UserID = @uid AND IsRead = 0"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@uid", userId)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch ex As Exception
            Return 0
        End Try
    End Function


    Public Function GetOrCreateConversation(userA As Integer, userB As Integer) As Integer
        Dim lowId As Integer = Math.Min(userA, userB)
        Dim highId As Integer = Math.Max(userA, userB)
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim checkSql As String = "SELECT ConvID FROM Conversations WHERE UserLowID = @low AND UserHighID = @high"
                Using cmd As New SqliteCommand(checkSql, conn)
                    cmd.Parameters.AddWithValue("@low", lowId)
                    cmd.Parameters.AddWithValue("@high", highId)
                    Dim res = cmd.ExecuteScalar()
                    If res IsNot Nothing Then Return Convert.ToInt32(res)
                End Using
                Dim insertSql As String = "INSERT INTO Conversations (UserLowID, UserHighID) VALUES (@low, @high); SELECT last_insert_rowid();"
                Using cmd As New SqliteCommand(insertSql, conn)
                    cmd.Parameters.AddWithValue("@low", lowId)
                    cmd.Parameters.AddWithValue("@high", highId)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error opening conversation: " & ex.Message)
            Return -1
        End Try
    End Function

    Public Function GetChatList(userId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        c.ConvID,
                        CASE WHEN c.UserLowID = @uid THEN c.UserHighID ELSE c.UserLowID END AS OtherUserID,
                        u.DisplayName AS OtherUserName,
                        u.Role AS OtherUserRole,
                        (SELECT Body FROM Messages WHERE ConvID = c.ConvID ORDER BY MessageID DESC LIMIT 1) AS LastMessage,
                        c.LastMessageAt,
                        (SELECT COUNT(*) FROM Messages WHERE ConvID = c.ConvID AND SenderID <> @uid AND ReadAt IS NULL) AS UnreadCount
                    FROM Conversations c
                    JOIN Users u ON (CASE WHEN c.UserLowID = @uid THEN c.UserHighID ELSE c.UserLowID END) = u.UserID
                    WHERE c.UserLowID = @uid OR c.UserHighID = @uid
                    ORDER BY c.LastMessageAt DESC"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@uid", userId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading chats: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetMessages(convId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT MessageID, SenderID, MsgType, Body, SentAt, DeliveredAt, ReadAt FROM Messages WHERE ConvID = @id ORDER BY MessageID ASC"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@id", convId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading messages: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function SendMessage(convId As Integer, senderId As Integer, body As String) As Integer
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "INSERT INTO Messages (ConvID, SenderID, MsgType, Body) VALUES (@cid, @sid, 'Text', @body); SELECT last_insert_rowid();"
                Dim msgId As Integer
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@cid", convId)
                    cmd.Parameters.AddWithValue("@sid", senderId)
                    cmd.Parameters.AddWithValue("@body", body)
                    msgId = Convert.ToInt32(cmd.ExecuteScalar())
                End Using
                Dim sqlUpd As String = "UPDATE Conversations SET LastMessageAt = datetime('now') WHERE ConvID = @id"
                Using cmd As New SqliteCommand(sqlUpd, conn)
                    cmd.Parameters.AddWithValue("@id", convId)
                    cmd.ExecuteNonQuery()
                End Using
                Return msgId
            End Using
        Catch ex As Exception
            MessageBox.Show("Error sending message: " & ex.Message)
            Return -1
        End Try
    End Function

    Public Sub MarkMessagesRead(convId As Integer, userId As Integer)
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Messages SET ReadAt = datetime('now') WHERE ConvID = @cid AND SenderID <> @uid AND ReadAt IS NULL"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@cid", convId)
                    cmd.Parameters.AddWithValue("@uid", userId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
        End Try
    End Sub


    Public Function GetBuyerOrders(buyerId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        o.OrderID,
                        o.OrderNo,
                        o.Status,
                        o.Qty,
                        o.TotalAmount,
                        o.CreatedAt,
                        i.EnglishName AS ItemName,
                        i.Unit AS ItemUnit,
                        u.DisplayName AS TraderName,
                        u.Phone AS TraderPhone,
                        o.ReasonComment
                    FROM Orders o
                    JOIN Items i ON o.ItemID = i.ItemID
                    JOIN Users u ON o.TraderID = u.UserID
                    WHERE o.BuyerID = @bid
                    ORDER BY o.CreatedAt DESC"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@bid", buyerId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching buyer orders: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function CancelOrder(orderId As Integer, byUser As String) As String
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Using transaction = conn.BeginTransaction()
                    Try
                        ' Get current status
                        Dim status As String = ""
                        Dim sqlGet As String = "SELECT Status FROM Orders WHERE OrderID = @id"
                        Using cmd As New SqliteCommand(sqlGet, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            Dim res = cmd.ExecuteScalar()
                            If res IsNot Nothing Then status = res.ToString()
                        End Using

                        If status <> "Pending" AndAlso status <> "Confirmed" Then
                            transaction.Rollback()
                            Return "ERR|INVALID_STATE|Cannot cancel in this state"
                        End If

                        ' If it was Confirmed, release the reserved stock
                        If status = "Confirmed" Then
                            Dim allocations As New List(Of Tuple(Of Integer, Double))
                            Dim sqlAlloc As String = "SELECT BatchID, Qty FROM OrderAllocations WHERE OrderID = @id"
                            Using cmd As New SqliteCommand(sqlAlloc, conn, transaction)
                                cmd.Parameters.AddWithValue("@id", orderId)
                                Using reader As SqliteDataReader = cmd.ExecuteReader()
                                    While reader.Read()
                                        allocations.Add(New Tuple(Of Integer, Double)(reader.GetInt32(0), reader.GetDouble(1)))
                                    End While
                                End Using
                            End Using

                            For Each a In allocations
                                Dim sqlRelease As String = "UPDATE Batches SET QtyReserved = QtyReserved - @qty WHERE BatchID = @id"
                                Using cmd As New SqliteCommand(sqlRelease, conn, transaction)
                                    cmd.Parameters.AddWithValue("@qty", a.Item2)
                                    cmd.Parameters.AddWithValue("@id", a.Item1)
                                    cmd.ExecuteNonQuery()
                                End Using
                            Next
                        End If

                        ' Update order status
                        Dim sqlUpdate As String = "UPDATE Orders SET Status = 'Cancelled', ClosedAt = datetime('now'), ReasonComment = @reason WHERE OrderID = @id"
                        Using cmd As New SqliteCommand(sqlUpdate, conn, transaction)
                            cmd.Parameters.AddWithValue("@reason", "Cancelled by " & byUser)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' Log event
                        Dim sqlEvent As String = "INSERT INTO OrderEvents (OrderID, Status, EventAt, Comment) VALUES (@id, 'Cancelled', datetime('now'), @comment)"
                        Using cmd As New SqliteCommand(sqlEvent, conn, transaction)
                            cmd.Parameters.AddWithValue("@id", orderId)
                            cmd.Parameters.AddWithValue("@comment", "Cancelled by " & byUser)
                            cmd.ExecuteNonQuery()
                        End Using

                        transaction.Commit()
                        Return "OK"
                    Catch ex As Exception
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error cancelling order: " & ex.Message)
            Return "ERR|VALIDATION|Database error"
        End Try
    End Function


    Public Function GetAdminStats() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        (SELECT COUNT(*) FROM Users WHERE Role = 'Trader' AND Status = 'Active') AS TotalTraders,
                        (SELECT COUNT(*) FROM Users WHERE Role IN ('Retailer', 'Wholesaler') AND Status = 'Active') AS TotalBuyers,
                        (SELECT COUNT(*) FROM Items WHERE Status = 'Active') AS ActiveListings,
                        (SELECT COUNT(*) FROM Orders WHERE DATE(CreatedAt) = DATE('now')) AS OrdersToday,
                        (SELECT COUNT(*) FROM Users WHERE Role = 'Trader' AND Status = 'Pending') AS PendingTraders,
                        (SELECT COUNT(*) FROM Batches WHERE Status = 'Active' AND ExpiryDate <= datetime('now')) AS ExpiredBatches"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching admin stats: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetPendingTraders() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT UserID, DisplayName, Email, Phone, FirstName, LastName, CreatedAt FROM Users WHERE Role = 'Trader' AND Status = 'Pending' ORDER BY CreatedAt ASC"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching pending traders: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function ApproveUser(userId As Integer, adminId As Integer) As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Users SET Status = 'Active', ApprovedBy = @aid, ApprovedAt = datetime('now') WHERE UserID = @uid AND Status = 'Pending'"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@aid", adminId)
                    cmd.Parameters.AddWithValue("@uid", userId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error approving user: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function RejectUser(userId As Integer) As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Users SET Status = 'Deactivated' WHERE UserID = @uid AND Status = 'Pending'"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@uid", userId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error rejecting user: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function GetSalesReport(fromDate As String, toDate As String) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        o.OrderNo,
                        o.CreatedAt,
                        u.DisplayName AS BuyerName,
                        i.EnglishName AS ItemName,
                        o.Qty,
                        o.UnitPriceAtOrder,
                        o.TotalAmount,
                        o.Status
                    FROM Orders o
                    JOIN Items i ON o.ItemID = i.ItemID
                    JOIN Users u ON o.BuyerID = u.UserID
                    WHERE o.Status = 'Completed' 
                      AND DATE(o.CreatedAt) BETWEEN @from AND @to
                    ORDER BY o.CreatedAt DESC"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@from", fromDate)
                    cmd.Parameters.AddWithValue("@to", toDate)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error generating sales report: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetStockReport() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        i.EnglishName,
                        i.Category,
                        i.Unit,
                        u.DisplayName AS TraderName,
                        IFNULL(SUM(CASE WHEN b.ExpiryDate > datetime('now') THEN (b.QtyRemaining - b.QtyReserved) ELSE 0 END), 0) AS AvailableQty,
                        i.UnitPrice,
                        COUNT(CASE WHEN b.ExpiryDate > datetime('now') THEN b.BatchID END) AS ActiveBatches
                    FROM Items i
                    JOIN Users u ON i.TraderID = u.UserID
                    LEFT JOIN Batches b ON i.ItemID = b.ItemID AND b.Status = 'Active'
                    WHERE i.Status = 'Active'
                    GROUP BY i.ItemID, i.EnglishName, i.Category, i.Unit, u.DisplayName, i.UnitPrice
                    ORDER BY i.EnglishName ASC"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error generating stock report: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetUsersReport() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT DisplayName, Email, Role, Status, Phone, CreatedAt FROM Users ORDER BY Role ASC, DisplayName ASC"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error generating users report: " & ex.Message)
        End Try
        Return dt
    End Function


    Public Function GetServerLogs(level As String, search As String) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT LoggedAt, Level, EventType, Details FROM ServerLog WHERE 1=1"
                If Not String.IsNullOrEmpty(level) AndAlso level <> "All" Then
                    sql &= " AND Level = @level"
                End If
                If Not String.IsNullOrEmpty(search) Then
                    sql &= " AND (Details LIKE @search OR EventType LIKE @search)"
                End If
                sql &= " ORDER BY LoggedAt DESC LIMIT 100"

                Using cmd As New SqliteCommand(sql, conn)
                    If Not String.IsNullOrEmpty(level) AndAlso level <> "All" Then
                        cmd.Parameters.AddWithValue("@level", level)
                    End If
                    If Not String.IsNullOrEmpty(search) Then
                        cmd.Parameters.AddWithValue("@search", "%" & search & "%")
                    End If
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching logs: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetConnectedUsers() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                ' For now, we'll just return all active users who have logged in recently
                Dim sql As String = "SELECT DisplayName, Role, Email, LastLoginAt FROM Users WHERE Status = 'Active' AND LastLoginAt IS NOT NULL ORDER BY LastLoginAt DESC"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching connected users: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Sub CreateSession(userId As Integer, token As String, ip As String)
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "INSERT INTO Sessions (UserID, Token, IPAddress) VALUES (@uid, @token, @ip)"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@uid", userId)
                    cmd.Parameters.AddWithValue("@token", token)
                    cmd.Parameters.AddWithValue("@ip", ip)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Public Sub CloseSession(token As String, reason As String)
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Sessions SET LogoutAt = datetime('now'), EndReason = @reason WHERE Token = @token AND LogoutAt IS NULL"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@reason", reason)
                    cmd.Parameters.AddWithValue("@token", token)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Public Function GetActiveSessions() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT u.DisplayName, u.Role, u.Email, s.LoginAt, s.IPAddress 
                    FROM Sessions s
                    JOIN Users u ON s.UserID = u.UserID
                    WHERE s.LogoutAt IS NULL"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return dt
    End Function

    Public Sub WriteLog(level As String, eventType As String, details As String, Optional userId As Integer = 0)
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "INSERT INTO ServerLog (LoggedAt, Level, UserID, EventType, Details) VALUES (datetime('now'), @lvl, @uid, @etype, @det)"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@lvl", level)
                    cmd.Parameters.AddWithValue("@uid", If(userId = 0, DBNull.Value, userId))
                    cmd.Parameters.AddWithValue("@etype", eventType)
                    cmd.Parameters.AddWithValue("@det", details)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
        End Try
    End Sub

    Public Function GetAllListings() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT i.ItemID, i.EnglishName, i.Category, i.Unit, i.UnitPrice, u.DisplayName AS TraderName, i.Status, IFNULL(SUM(CASE WHEN b.ExpiryDate > datetime('now') THEN (b.QtyRemaining - b.QtyReserved) ELSE 0 END), 0) AS AvailableQty FROM Items i JOIN Users u ON i.TraderID = u.UserID LEFT JOIN Batches b ON i.ItemID = b.ItemID AND b.Status = 'Active' GROUP BY i.ItemID, i.EnglishName, i.Category, i.Unit, i.UnitPrice, u.DisplayName, i.Status ORDER BY i.EnglishName ASC"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return dt
    End Function

    Public Function UpdateListingStatus(itemId As Integer, newStatus As String) As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Items SET Status = @status WHERE ItemID = @id"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@status", newStatus)
                    cmd.Parameters.AddWithValue("@id", itemId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function UpdateUserProfile(userId As Integer, phone As String, location As String, profilePic As String) As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Users SET Phone = @phone, LocationDetail = @loc, ProfilePic = @pic WHERE UserID = @id"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@phone", phone)
                    cmd.Parameters.AddWithValue("@loc", location)
                    cmd.Parameters.AddWithValue("@pic", If(String.IsNullOrEmpty(profilePic), DBNull.Value, profilePic))
                    cmd.Parameters.AddWithValue("@id", userId)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetUserProfile(userId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "SELECT DisplayName, Role, Phone, LocationDetail, ProfilePic FROM Users WHERE UserID = @id"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@id", userId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return dt
    End Function

    Public Sub RunExpiryCheck()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()

                ' 1. Mark expired batches
                Dim sqlExpire As String = "
                    UPDATE Batches 
                    SET Status = 'Expired' 
                    WHERE Status = 'Active' 
                      AND ExpiryDate <= datetime('now')"
                Using cmd As New SqliteCommand(sqlExpire, conn)
                    cmd.ExecuteNonQuery()
                End Using

                ' 2. Find batches expiring soon (within 2 days) that haven't been notified yet
                Dim sqlSoon As String = "
                    SELECT b.BatchID, b.ItemID, i.EnglishName, i.TraderID, b.ExpiryDate
                    FROM Batches b
                    JOIN Items i ON b.ItemID = i.ItemID
                    WHERE b.Status = 'Active' 
                      AND b.ExpiryDate > datetime('now') 
                      AND b.ExpiryDate <= datetime('now', '+2 days')"

                Using cmd As New SqliteCommand(sqlSoon, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim traderId As Integer = Convert.ToInt32(reader("TraderID"))
                            Dim itemName As String = reader("EnglishName").ToString()

                            ' Create notification for the trader
                            Dim notifSql As String = "
                                INSERT INTO Notifications (UserID, NotifType, Title, Body, RelatedType, RelatedID, CreatedAt) 
                                VALUES (@uid, 'EXPIRY_WARNING', 'Batch Expiring Soon', @body, 'Batch', @bid, datetime('now'))"

                            Using notifCmd As New SqliteCommand(notifSql, conn)
                                notifCmd.Parameters.AddWithValue("@uid", traderId)
                                notifCmd.Parameters.AddWithValue("@body", $"Your batch of {itemName} is expiring soon.")
                                notifCmd.Parameters.AddWithValue("@bid", reader("BatchID"))
                                notifCmd.ExecuteNonQuery()
                            End Using
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error running expiry check: " & ex.Message)
        End Try
    End Sub

    Public Function GetTraderHomeStats(traderId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT 
                        (SELECT COUNT(*) FROM Items WHERE TraderID = @tid AND Status = 'Active') AS ItemsInStock,
                        (SELECT COUNT(*) FROM Orders WHERE TraderID = @tid AND Status = 'Pending') AS PendingOrders,
                        (SELECT COUNT(*) FROM Orders WHERE TraderID = @tid AND Status = 'Completed') AS CompletedOrders,
                        (SELECT IFNULL(SUM(TotalAmount), 0) FROM Orders WHERE TraderID = @tid AND Status = 'Completed' AND CreatedAt >= datetime('now', 'start of month')) AS SalesThisMonth"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@tid", traderId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching trader stats: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetBatchesToWatch(traderId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT b.BatchNo, i.EnglishName, b.ExpiryDate, 
                           CAST(julianday(b.ExpiryDate) - julianday('now') AS INTEGER) AS DaysLeft
                    FROM Batches b
                    JOIN Items i ON b.ItemID = i.ItemID
                    WHERE i.TraderID = @tid 
                      AND b.Status = 'Active'
                      AND (b.ExpiryDate <= datetime('now', '+3 days') OR (b.QtyRemaining - b.QtyReserved) <= i.LowStockLevel)
                    ORDER BY b.ExpiryDate ASC
                    LIMIT 5"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@tid", traderId)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return dt
    End Function

    Public Function GetAllOrders() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                    SELECT o.OrderNo, o.CreatedAt, o.Status, o.TotalAmount, 
                           i.EnglishName AS ItemName, 
                           uBuyer.DisplayName AS BuyerName, 
                           uTrader.DisplayName AS TraderName
                    FROM Orders o
                    JOIN Items i ON o.ItemID = i.ItemID
                    JOIN Users uBuyer ON o.BuyerID = uBuyer.UserID
                    JOIN Users uTrader ON o.TraderID = uTrader.UserID
                    ORDER BY o.CreatedAt DESC"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error fetching orders: " & ex.Message)
        End Try
        Return dt
    End Function

    Public Function GetSettings() As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim createSql As String = "CREATE TABLE IF NOT EXISTS Settings (SettingKey TEXT(40) PRIMARY KEY, SettingValue TEXT(100), Description TEXT(120));"
                Using cmd As New SqliteCommand(createSql, conn)
                    cmd.ExecuteNonQuery()
                End Using
                Dim countSql As String = "SELECT COUNT(*) FROM Settings"
                Using cmd As New SqliteCommand(countSql, conn)
                    If Convert.ToInt32(cmd.ExecuteScalar()) = 0 Then
                        Dim seedSql As String = "
                            INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES 
                            ('ExpirySoonDays', '2', 'Days before expiry to show warning'),
                            ('DefaultLowStock', '10', 'Default low stock threshold'),
                            ('SessionTimeoutSeconds', '120', 'Seconds of inactivity before timeout'),
                            ('MaxFailedLogins', '5', 'Max failed login attempts'),
                            ('ServerPort', '5000', 'Server port');"
                        Using cmdSeed As New SqliteCommand(seedSql, conn)
                            cmdSeed.ExecuteNonQuery()
                        End Using
                    End If
                End Using
                Dim sql As String = "SELECT SettingKey, SettingValue, Description FROM Settings ORDER BY SettingKey"
                Using cmd As New SqliteCommand(sql, conn)
                    Using reader As SqliteDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return dt
    End Function

    Public Function UpdateSetting(key As String, value As String) As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "UPDATE Settings SET SettingValue = @val WHERE SettingKey = @key"
                Using cmd As New SqliteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@val", value)
                    cmd.Parameters.AddWithValue("@key", key)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

End Module