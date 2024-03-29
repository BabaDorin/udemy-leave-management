﻿using leave_management.Data;
using leave_management.Data.Migrations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveRequestVM
    {
        public int Id { get; set; }

        public Employee RequestingEmployee { get; set; }
        public string RequestingEmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public LeaveType LeaveType { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime DateRequested { get; set; }
        public DateTime DateActioned { get; set; }

        public bool? Approved { get; set; }
        public bool Canceled { get; set; }
        public EmployeeVM ApprovedBy { get; set; }
        public string ApprovedById { get; set; }
        public string Comments { get; set; }
    }

    public class AdminLeaveRequestVM
    {
        [Display(Name = "Total Requests")]
        public int TotalRequests { get; set; }

        [Display(Name = "Approved Requests")]
        public int ApprovedRequests { get; set; }

        [Display(Name = "Rejected Requests")]
        public int RejectedRequests { get; set; }

        [Display(Name = "Pending Requests")]
        public int PendingRequests { get; set; }
       
        public List<LeaveRequestVM> LeaveRequests { get; set; }
    }

    public class CreateLeaveRequestVM
    {
        [Display(Name = "Start Date")]
        [Required]
        public string StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required]
        public string EndDate { get; set; }

        public IEnumerable<SelectListItem> LeaveTypes { get; set; }

        [Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }

        public string Comments { get; set; }
    }

    public class EmployeeLeaveRequestsViewVM
    {
        public IEnumerable<LeaveAllocationVM> LeaveAllocations { get; set; }
        public IEnumerable<LeaveRequestVM> LeaveRequests { get; set; }
    }
}
