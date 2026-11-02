Imports System.Net.Sockets
Imports System.Text
Imports Windows.Win32.System

Module Program
    Sub Main(args As String())
        Console.WriteLine("Test Client Started...")
        Try
            Dim client As New TcpClient()
            client.Connect("127.0.0.1", 5000)
            Dim stream As NetworkStream = client.GetStream()

            ' Send HELLO (Format: COMMAND | ReqID | Token | Param1)
            Dim helloMsg As String = "HELLO|1||1.0" & vbCrLf
            Dim data As Byte() = Encoding.UTF8.GetBytes(helloMsg)

            stream.Write(data, 0, data.Length)
            Console.WriteLine("Sent: HELLO|1||1.0")

            Threading.Thread.Sleep(2000)

            client.Close()
        Catch ex As Exception
            Console.WriteLine("Error: " & ex.Message)
        End Try

        Console.WriteLine("Press Enter to exit.")
        Console.ReadLine()
    End Sub
End Module
