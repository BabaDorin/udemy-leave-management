using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Data
{
    public class Employee : IdentityUser
    {
        // Employee = User.
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TaxID { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateJoined { get; set; }
    }
}
