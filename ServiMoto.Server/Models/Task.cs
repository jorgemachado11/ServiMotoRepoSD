using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiMoto.Server.Models
{
    public class Task
    {
        public string TaskId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ClientId { get; set; }
    }
}
