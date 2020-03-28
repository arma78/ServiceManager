using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceManager.Models
{
    public class Profession
    {
        [Key]
        public int Id { get; set; }
        public string Skill { get; set; }
    }
}
