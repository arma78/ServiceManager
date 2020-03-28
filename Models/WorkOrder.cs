using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceManager.Models
{
    public class WorkOrder
    {
        [Key]
        public int WorkServiceID { get; set; }
        public string Property_Address { get; set; }
        public int Floor { get; set; }
        public string Unit { get; set; }
        public string WorkServiceName { get; set; }
        public string WorkService_Description { get; set; }
        
        public string RequestedBy { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Requested_Date { get; set; }
        public string Contractor_Assigned { get; set; }
        public string Contractor_Comments { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Contractor_Start_Date { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Contractor_Completion_Date { get; set; }
        public string Service_Status { get; set; }
        public string FolderUrl { get; set; }
        public string Inspected_By { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Date_Inspected { get; set; }
        public string Inspection_Comments { get; set; }

        public List<ApplicationUser> AppUser { get; set; }
    }
}
