Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Convert
Imports System.Text.RegularExpressions

Module ServerlistHandler
    ' Settings
    Private Const MASTER_SERVER_ADDRESS As String = "updater.fivereborn.com"
    Private Const MASTER_SERVER_PORT As Integer = 30110
    Private Const MASTER_SERVER_LIST As String = "https://servers.fivereborn.com/json"

    ''' -----------------------------------------------------------------------------
    ''' <summary>Get serverlist from server api</summary>
    ''' <returns>A list of ServerInfo objects</returns>
    ''' -----------------------------------------------------------------------------
    Public Function getServerList() As List(Of ServerInfo)
        Dim result As String = New WebClient().DownloadString(MASTER_SERVER_LIST)
        Return JsonToServerObjects(result)
    End Function

    Private Function JsonToServerObjects(json As String) As List(Of ServerInfo)
        Dim result As New List(Of ServerInfo)()

        Dim pattern As String = """(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{1,5})"":{""sv_maxclients"":""(\d+)"",""clients"":""(\d+)"",""challenge"":""(.*?)"",""gamename"":""(.*?)"",""protocol"":""(\d+)"",""hostname"":""(.*?)"",""gametype"":""(.*?)"",""mapname"":""(.*?)""}"

        For Each match As Match In Regex.Matches(json, pattern, RegexOptions.IgnoreCase)
            If match.Groups.Count <= 1 Then
                Continue For
            End If

            result.Add(New ServerInfo(match.Groups(1).Value,
                                      match.Groups(2).Value,
                                      match.Groups(3).Value,
                                      match.Groups(4).Value,
                                      match.Groups(5).Value,
                                      match.Groups(6).Value,
                                      match.Groups(7).Value,
                                      match.Groups(8).Value,
                                      match.Groups(9).Value))
        Next
        Return result
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>Get serverips from masterserver</summary>
    ''' <returns>A list of IPs as string</returns>
    ''' -----------------------------------------------------------------------------
    Public Function getLiveServerIps(Optional leadingZeros As Boolean = False) As List(Of [String])
        Dim data As Byte() = New Byte(1023) {}

        ' resolve address
        Dim addresses As IPAddress() = Dns.GetHostAddresses(MASTER_SERVER_ADDRESS)
        If addresses.Length <> 1 Then
            Throw New Exception("Server is a invalid address or has mutliple addresses")
        End If

        Dim ip As New IPEndPoint(addresses(0), MASTER_SERVER_PORT)

        ' setup socket
        Dim server As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)

        ' prepare message
        Dim outString As [String] = "....getservers GTA5 3 full empty"
        Dim outMessage As Byte() = Encoding.UTF8.GetBytes(outString)
        outMessage(0) = &HFF
        outMessage(1) = &HFF
        outMessage(2) = &HFF
        outMessage(3) = &HFF

        ' send message
        server.SendTo(outMessage, ip)

        ' receive response
        Dim sender As New IPEndPoint(IPAddress.Any, 0)
        Dim Remote As EndPoint = DirectCast(sender, EndPoint)
        Dim len As Integer = server.ReceiveFrom(data, Remote)

        ' close socket
        server.Close()

        ' generate list from response
        Return HandleServersResponse(data, len, leadingZeros)
    End Function

    Private Function HandleServersResponse(response As Byte(), len As Integer, leadingZeros As Boolean) As List(Of [String])
        Dim result As New List(Of [String])()

        Dim server As [String] = ""
        Dim i As Integer = 0

        While i < len
            Do
                If ToChar(response(PostIncrement(i))) = "\"c Then
                    Exit Do
                End If
            Loop While i < len

            If i >= len - 8 Then
                Exit While
            End If

            ' get IP
            server = ""
            If leadingZeros Then
                server += response(PostIncrement(i)).ToString("D3")
                server += "." & response(PostIncrement(i)).ToString("D3")
                server += "." & response(PostIncrement(i)).ToString("D3")
                server += "." & response(PostIncrement(i)).ToString("D3")
            Else
                server += response(PostIncrement(i)).ToString()
                server += "." & response(PostIncrement(i)).ToString()
                server += "." & response(PostIncrement(i)).ToString()
                server += "." & response(PostIncrement(i)).ToString()
            End If

            ' get port
            Dim port As Integer = CInt(response(PostIncrement(i))) << 8
            port += (CInt(response(PostIncrement(i))) And &HFF)
            server += ":" & port.ToString()

            If ToChar(response(i)) <> "\"c Then
                Exit While
            End If

            result.Add(server)
            server = ""

            ' get EOT
            If ToChar(response(i + 1)) = "E"c AndAlso ToChar(response(i + 2)) = "O"c AndAlso ToChar(response(i + 3)) = "T"c Then
                Exit While
            End If
        End While
        Return result
    End Function

    Private Function PostIncrement(ByRef i As Integer) As Integer
        Dim a As Integer = i
        i += 1
        Return a
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>Information container of a server</summary>
    ''' -----------------------------------------------------------------------------
    Public Class ServerInfo
        Private m_ip As String
        ReadOnly Property ip() As String
            Get
                ip = m_ip
            End Get
        End Property

        Private m_maxclients As Integer
        ReadOnly Property maxclients() As Integer
            Get
                maxclients = m_maxclients
            End Get
        End Property

        Private m_clients As Integer
        ReadOnly Property clients() As Integer
            Get
                clients = m_clients
            End Get
        End Property

        Private m_challenge As String
        ReadOnly Property challenge() As String
            Get
                challenge = m_challenge
            End Get
        End Property

        Private m_gamename As String
        ReadOnly Property gamename() As String
            Get
                gamename = m_gamename
            End Get
        End Property

        Private m_protocol As Integer
        ReadOnly Property protocol() As Integer
            Get
                protocol = m_protocol
            End Get
        End Property

        Private m_hostname As String
        ReadOnly Property hostname() As String
            Get
                hostname = m_hostname
            End Get
        End Property

        Private m_gametype As String
        ReadOnly Property gametype() As String
            Get
                gametype = m_gametype
            End Get
        End Property

        Private m_mapname As String
        ReadOnly Property mapname() As String
            Get
                mapname = m_mapname
            End Get
        End Property

        Sub New(
        ip As String,
        maxclients As Integer,
        clients As Integer,
        challenge As String,
        gamename As String,
        protocol As Integer,
        hostname As String,
        gametype As String,
        mapname As String
        )
            m_ip = ip
            m_maxclients = maxclients
            m_clients = clients
            m_challenge = challenge
            m_gamename = gamename
            m_protocol = protocol
            m_hostname = hostname
            m_gametype = gametype
            m_mapname = mapname
        End Sub

        Public Overrides Function ToString() As String
            Return " Then ThenServerInfo(ip:    " + m_ip.ToString() +
                     ", maxclients: " + m_maxclients.ToString() +
                     ", clients: " + m_clients.ToString() +
                     ", challenge: " + m_challenge.ToString() +
                     ", gamename: " + m_gamename.ToString() +
                     ", hostname: " + m_hostname.ToString() +
                     ", gametype: " + m_gametype.ToString() +
                     ", mapname: " + m_mapname.ToString() + ")"
        End Function
    End Class
End Module