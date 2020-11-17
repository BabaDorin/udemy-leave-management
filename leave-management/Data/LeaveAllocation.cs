using leave_management.Data.Migrations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Data
{
    public class LeaveAllocation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int NumberOfDays { get; set; }
        public DateTime DateCreated { get; set; }

        public Employee Employee { get; set; }
        public string EmployeeId { get; set; }
        
        public LeaveType LeaveType{ get; set; }
        public int LeaveTypeId { get; set; }
        public int Period { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Employees { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> LeaveTypes { get; set; }
    }
}
