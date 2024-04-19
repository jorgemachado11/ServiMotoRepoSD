using ServiMoto.Server.Services;
using System.Net.Sockets;
using System.Net;
using System.Text;
using ServiMoto.Server.Models;

class Program
{
    private static List<ClientAllocation> clientAllocations;

    private static Mutex fileMutex = new Mutex();
    private static CsvReaderService csvReaderService = new CsvReaderService(fileMutex);
    private static string allocationsPath = "C:\\Users\\Jorge\\Downloads\\Alocacao_Cliente_Servico.csv";

    static void Main(string[] args)
    {
        // Start the TCP server
        var listener = new TcpListener(IPAddress.Any, 8888);
        listener.Start();
        Console.WriteLine("Server is listening on port 8888...");

        clientAllocations = csvReaderService.ReadClientAllocations(allocationsPath);

        // Accept clients in a loop
        while (true)
        {
            var client = listener.AcceptTcpClient();
            var thread = new Thread(() => HandleClient(client));
            thread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        try
        {
            List<ServiMoto.Server.Models.Task> tasks = new List<ServiMoto.Server.Models.Task>();

            var stream = client.GetStream();
            var buffer = new byte[1024];
            int bytesRead;
            string clientId = "";
            string service = "";

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                var request = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                if (request.StartsWith("CONNECT,ID:"))
                {
                    clientId = request.Substring("CONNECT,ID:".Length);

                    //Check Allocations for the Client Logged in
                    var clientAllocation = clientAllocations.Where(f => f.ClientId == clientId)
                        .FirstOrDefault();

                    if (clientAllocation == null)
                    {
                        //Allocate client to a Service
                        csvReaderService.AssociateUserToService(allocationsPath, clientId, "Servico_A");
                        service = "Servico_A";
                    }
                    else
                        service = clientAllocation.ServiceId;


                    string response = "100 OK";

                    SendResponse(stream, response);
                }
                else if (request == "QUIT")
                {
                    SendResponse(stream, "400 BYE");
                    break;
                }
                else if (request.StartsWith("UPDATE_TASK,ID:"))
                {
                    fileMutex.WaitOne();
                    try
                    {
                        tasks = csvReaderService.ReadTasks(service);

                        var parts = request.Split(',');
                        var taskId = parts[1].Split(':')[1];
                        var status = parts[2].Split(':')[1];

                        var taskToUpdate = tasks.FirstOrDefault(t => t.TaskId == taskId && t.ClientId == clientId);
                        if (taskToUpdate != null)
                        {
                            taskToUpdate.Status = status;

                            csvReaderService.WriteTasks($"C:\\Users\\Jorge\\Downloads\\{service}.csv", tasks);
                            SendResponse(stream, $"UPDATE_CONFIRMED,ID:{taskId}");
                            tasks = csvReaderService.ReadTasks(service);
                        }
                        else
                        {
                            SendResponse(stream, "ERROR: Task not found or it's not assigned to you!");
                        }
                    }
                    finally
                    {
                        fileMutex.ReleaseMutex();
                    }
                }
                else if (request.StartsWith("REQUEST_TASK"))
                {
                    fileMutex.WaitOne();
                    try
                    {
                        tasks = csvReaderService.ReadTasks(service);
                        var taskToAllocate = tasks.FirstOrDefault(f => f.Status == "Nao alocado");
                        if (taskToAllocate == null)
                            SendResponse(stream, "ERROR: There are no Tasks to be allocated!");

                        taskToAllocate.Status = "em curso";
                        taskToAllocate.ClientId = clientId;
                        csvReaderService.WriteTasks($"C:\\Users\\Jorge\\Downloads\\{service}.csv", tasks);
                        SendResponse(stream, $"Task Allocated and 'Em Curso'");
                        tasks = csvReaderService.ReadTasks(service);
                    }
                    finally
                    {
                        fileMutex.ReleaseMutex();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception occurred: {ex.Message}");
        }
    }

    private static void SendResponse(NetworkStream stream, string response)
    {
        var responseData = Encoding.ASCII.GetBytes(response);
        stream.Write(responseData, 0, responseData.Length);
    }
}