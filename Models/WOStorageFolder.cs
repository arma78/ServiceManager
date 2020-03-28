using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServiceManager.Models
{
    public class WOStorageFolder
    {
        public SelectList StrageFolder { get; set; }
        public string FolderString { get; set; }
        public List<ApplicationUser> appContractors { get; set; }
    }
}
