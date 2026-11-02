Imports Microsoft.Data.Sqlite
Imports System.IO

Public Module modDB
    Public ReadOnly DbPath As String = Path.Combine(Application.StartupPath, "agrimarket.db")
    Public ReadOnly ConnString As String = $"Data Source={DbPath};Default Timeout=30;Pooling=False;Foreign Keys=False;"
    Public Function InitializeDatabase() As Boolean
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()

                ' Enable WAL mode for better concurrency
                Using cmd As New SqliteCommand("PRAGMA journal_mode=WAL;", conn)
                    cmd.ExecuteNonQuery()
                End Using

                ' Users
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

                ' Items
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

                ' Batches
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

                ' ItemLocalNames
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

        Dim adminEmail As String = "admin@agrimarket.com"
        Dim adminPass As String = "admin123"
        Dim salt As String = modSecurity.GenerateSalt()
        Dim hash As String = modSecurity.HashPassword(adminPass, salt)

        Dim insertSql As String = "INSERT INTO Users (Email, PasswordHash, Salt, Role, DisplayName, Status) " &
                                  "VALUES (@Email, @Hash, @Salt, 'Admin', 'System Admin', 'Active')"
        Using cmd As New SqliteCommand(insertSql, conn)
            cmd.Parameters.AddWithValue("@Email", adminEmail)
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

    Public Function AddItem(traderId As Integer, category As String, itemCode As String, name As String, description As String, unitPrice As Decimal, unit As String, lowStock As Integer, qty As Double, shelfLifeDays As Integer, localNamesData As String) As String
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
            MessageBox.Show("Error saving item: " & ex.Message)
            Return ""
        End Try
    End Function

    Public Function GetTraderStock(traderId As Integer) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As New SqliteConnection(ConnString)
                conn.Open()
                Dim sql As String = "
                SELECT i.ItemID, i.EnglishName, i.Unit, i.UnitPrice, 
                       IFNULL(SUM(CASE WHEN b.ExpiryDate > datetime('now') THEN (b.QtyRemaining - b.QtyReserved) ELSE 0 END), 0) as AvailableQty,
                       COUNT(CASE WHEN b.ExpiryDate > datetime('now') THEN b.BatchID END) as BatchCount
                FROM Items i
                LEFT JOIN Batches b ON i.ItemID = b.ItemID AND b.Status = 'Active'
                WHERE i.TraderID = @tid AND i.Status = 'Active'
                GROUP BY i.ItemID, i.EnglishName, i.Unit, i.UnitPrice"
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
End Module