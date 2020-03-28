using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceManager
{
    public class StorageAccountOptions
    {
        public string apiKey { get; set; }
        public string bucket { get; set; }
        public string AuthEmail { get; set; }
        public string AuthPassword { get; set; }
        public string authDomain { get; set; }
    }
}
