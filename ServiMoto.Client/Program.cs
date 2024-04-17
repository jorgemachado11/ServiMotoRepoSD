using System;
using System.Net.Sockets;
using System.Text;

namespace ServiMoto.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the server IP address:");
            string serverIp = Console.ReadLine();

            Console.WriteLine("Enter your client Id:");
            string clientId = Console.ReadLine();

            // Create a TCP client and connect to the server
            using (var client = new TcpClient(serverIp, 8888))
            {
                var stream = client.GetStream();

                // Connect to the server
                SendRequest(stream, $"CONNECT,ID:{clientId}");
                // Read server response for the connect request
                ReadResponse(stream);

                bool running = true;
                while (running)
                {
                    // Display menu to user
                    Console.WriteLine("\nChoose an option:");
                    Console.WriteLine("1: Update Task");
                    Console.WriteLine("2: Request New Task");
                    Console.WriteLine("3: Quit");
                    Console.Write("Enter your choice: ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            // Update Task
                            Console.WriteLine("Enter Task ID:");
                            string taskId = Console.ReadLine();
                            Console.WriteLine("Enter Task Status:");
                            string status = Console.ReadLine();
                            SendRequest(stream, $"UPDATE_TASK,ID:{taskId},STATUS:{status}");
                            ReadResponse(stream);
                            break;
                        case "2":
                            // Request New Task
                            Console.WriteLine("Enter Task ID:");
                            string taskToRequestId = Console.ReadLine();
                            SendRequest(stream, $"REQUEST_TASK,ID:{taskToRequestId}");
                            ReadResponse(stream);
                            break;
                        case "3":
                            // Quit
                            SendRequest(stream, "QUIT");
                            ReadResponse(stream);
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Invalid choice, please try again.");
                            break;
                    }
                }
            }
        }

        // Helper method to send requests
        static void SendRequest(NetworkStream stream, string request)
        {
            var requestData = Encoding.ASCII.GetBytes(request);
            stream.Write(requestData, 0, requestData.Length);
        }

        // Helper method to read responses
        static void ReadResponse(NetworkStream stream)
        {
            var buffer = new byte[1024];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Server response: {response}");
        }
    }
}
