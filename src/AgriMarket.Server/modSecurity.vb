Imports System.Security.Cryptography
Imports System.Text

Public Module modSecurity
    ' 1. Generate a random 32-byte salt
    Public Function GenerateSalt() As String
        Dim saltBytes(31) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(saltBytes)
        End Using
        Return Convert.ToBase64String(saltBytes)
    End Function

    ' 2. Hash the password using SHA-256 (Salt + Password)
    Public Function HashPassword(password As String, salt As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim combined As String = salt & password
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(combined)
            Dim hash As Byte() = sha256.ComputeHash(bytes)
            Return Convert.ToBase64String(hash)
        End Using
    End Function
End Module
