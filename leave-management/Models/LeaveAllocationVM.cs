using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveAllocationVM
    {
        public int NumberOfDays { get; set; }
        public DateTime DateCreated { get; set; }

        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }

        public DetailsLeaveTypeVM LeaveType { get; set; }
        public int LeaveTypeId { get; set; }

    }
}
