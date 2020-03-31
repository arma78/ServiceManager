using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceManager.Models
{
    public class MetaData
    {

        [Key]
        public int Id { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public DateTime? ModifiedStatusDate { get; set; }

        [Column("StatusModifiedBy")]
        [Display(Name = "StatusModifiedBy")]
        public string StatusModifiedBy { get; set; }

        [Column("CreatedBy")]
        [Display(Name = "Creator")]
        public string CreatedBy { get; set; }

        [Column("ModifiedBy")]
        [Display(Name = "Modifier")]
        public string ModifiedBy { get; set; }
        [ForeignKey("WorkOrder")]
        public int WorkServiceID { get; set; }
        public WorkOrder WorkOrder { get; set; }

    }
}
