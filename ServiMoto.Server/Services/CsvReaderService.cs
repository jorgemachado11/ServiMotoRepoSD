using ServiMoto.Server.Models;
using System.Threading.Tasks;

namespace ServiMoto.Server.Services
{
    public class CsvReaderService
    {
        private Mutex fileMutex;

        public CsvReaderService(Mutex fileMutex)
        {
            this.fileMutex = fileMutex;
        }

        public List<Models.Task> ReadTasks(string service)
        {
            return File.ReadAllLines($"C:\\Users\\Jorge\\Downloads\\{service}.csv")
                .Skip(1) // Skip the header line
                .Select(line => line.Split(','))
                .Select(parts => new Models.Task
                {
                    TaskId = parts[0],
                    Description = parts[1],
                    Status = parts[2],
                    ClientId = parts[3]
                })
                .ToList();
        }

        public List<ClientAllocation> ReadClientAllocations(string filePath)
        {
            List<ClientAllocation> list = new List<ClientAllocation>();

            var allocations = File.ReadAllLines(filePath).Skip(1);// Skip the header line

            foreach (var a in allocations)
            {
                var splitted = a.Split(",");

                //Only if the allocations have 2 Properties
                if (splitted.Length == 2)
                    list.Add(new ClientAllocation
                    {
                        ClientId = splitted[0],
                        ServiceId = splitted[1]
                    });
            }

            return list;
        }

        public void AssociateUserToService(string filePath, string clientIdToAllocate, string serviceToBeAllocated)
        {
            var allocations = ReadClientAllocations(filePath);

            var csvLines = new List<string> { "ClienteID,ServicoID" };

            foreach(var a in allocations)
            {
                var line = $"{a.ClientId},{a.ServiceId}";
                csvLines.Add(line);
            }

            csvLines.Add($"{clientIdToAllocate},{serviceToBeAllocated}");
            File.WriteAllLines(filePath, csvLines);
        }

        public void WriteTasks(string filePath, List<Models.Task> tasks)
        {
            var csvLines = new List<string> { "TarefaID,Descricao,Estado,ClienteID" };

            foreach (var task in tasks)
            {
                var line = $"{task.TaskId},{task.Description},{task.Status},{task.ClientId}";
                csvLines.Add(line);
            }

            fileMutex.WaitOne();
            try
            {
                File.WriteAllLines(filePath, csvLines);
            }
            finally
            {
                fileMutex.ReleaseMutex();
            }
        }
    }
}
