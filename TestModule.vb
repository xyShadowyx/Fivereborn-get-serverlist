Module TestModule

    Sub Main()
        '' List servers (with all nessesary information) from serverlist api (servers.fivereborn.com)
        Dim serverList As List(Of ServerInfo) = getServerList()
        For Each server As ServerInfo In serverList
            Console.WriteLine(server.ip)
        Next

        '' List serverips (ips only) from masterserver 
        Dim serverIps As List(Of String) = getLiveServerIps()
        For Each ip As String In serverIps
            Console.WriteLine(ip)
        Next
        Console.Read()
    End Sub

End Module
