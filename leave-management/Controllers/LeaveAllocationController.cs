using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Data.Migrations;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace leave_management.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveAllocationController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<Employee> userManager
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // GET: LeaveAllocation
        public async Task<ActionResult> Index(int? numberUpdated)
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
            var mappedLeaveTypes = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveTypes.ToList());
            var model = new CreateLeaveAllocationVM
            {
                LeaveTypes = mappedLeaveTypes,
                NumberUpdated = numberUpdated ?? 0
            };

            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            var leaveType = await _unitOfWork.LeaveTypes.Find(q => q.Id == id);
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            int nrUpdated = 0;
            foreach (var emp in employees)
            {
                if (await _unitOfWork.LeaveAllocations.Find(q => q.LeaveTypeId == id && q.EmployeeId == emp.Id
                                     && q.Period == DateTime.Now.Year) != null)
                    continue;

                var allocation = new LeaveAllocationVM
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = leaveType.Id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = DateTime.Now.Year
                };

                var leaveAllocation = _mapper.Map<LeaveAllocation>(allocation);
                await _unitOfWork.LeaveAllocations.Create(leaveAllocation);
                await _unitOfWork.Save();
                nrUpdated++;
            }
            return RedirectToAction(nameof(Index), new { numberUpdated = nrUpdated });
        }

        public async Task<ActionResult> ListEmployees()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var model = _mapper.Map<List<EmployeeVM>>(employees);
            return View(model);
        }

        // GET: LeaveAllocation/Details/5
        public ActionResult Details(string id)
        {
            var employee = _mapper.Map<EmployeeVM>(_userManager.FindByIdAsync(id).Result);
            var period = DateTime.Now.Year;
            var allocations = _mapper.Map<List<LeaveAllocationVM>>(_unitOfWork.LeaveAllocations.FindAll(
                expression: q => q.EmployeeId == id && q.Period == DateTime.Now.Year,
                includes: new List<string> { "LeaveType" } ));
            var model = new ViewAllocationsVM
            {
                Employee = employee,
                LeaveAllocations = allocations
            };

            return View(model);
        }

        // GET: LeaveAllocation/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocation/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var leaveAllocation = await _unitOfWork.LeaveAllocations.Find(q => q.Id == id);
            var model = _mapper.Map<EditLeaveAllocationVM>(leaveAllocation);
            return View(model);
        }

        // POST: LeaveAllocation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationVM model)
        {
            try
            {
                // TODO: Add update logic here
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var record = await _unitOfWork.LeaveAllocations.Find(q => q.Id == model.Id);
                record.NumberOfDays = model.NumberOfDays;
                _unitOfWork.LeaveAllocations.Update(record);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Details), new { id = model.EmployeeId });
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocation/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
} 