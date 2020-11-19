using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController(
            IMapper mapper,
            UserManager<Employee> userManager,
            IUnitOfWork unitOfWork
            )
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize(Roles = "Administrator")]
        // GET: LeaveRequest
        public async Task<ActionResult> Index()
        {
            var leaveRequests = await _unitOfWork.LeaveRequests.FindAll(
                includes: new List<string> { "RequestingEmployee", "LeaveType"});
            var leaveRequestsModels = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);
            var model = new AdminLeaveRequestVM
            {
                TotalRequests = leaveRequestsModels.Count,
                ApprovedRequests = leaveRequestsModels.Count(q => q.Approved == true),
                PendingRequests = leaveRequestsModels.Count(q => q.Approved == null),
                RejectedRequests = leaveRequestsModels.Count(q => q.Approved == false),
                LeaveRequests = leaveRequestsModels
            };

            return View(model);
        }

        // GET: LeaveRequest/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id,
                includes: new List<string> { "ApprovedBy", "RequestingEmployee", "LeaveType" });
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);

            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id, bool flag)
        {
            try
            {
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id); 
                var employeeId = leaveRequest.RequestingEmployeeId;
                var period = DateTime.Now.Year;
                var allocation = await _unitOfWork.LeaveAllocations.Find(q => q.EmployeeId == employeeId 
                                                                            && q.LeaveTypeId == leaveRequest.LeaveTypeId
                                                                            && q.Period == period);
                var daysRequeste = (int)(leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).TotalDays;
                allocation.NumberOfDays -= daysRequeste;

                leaveRequest.Approved = flag;
                var user = _userManager.GetUserAsync(User).Result;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                _unitOfWork.LeaveRequests.Update(leaveRequest);
                _unitOfWork.LeaveAllocations.Update(allocation);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<ActionResult> MyLeave()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var myLeaveAllocations = _mapper.Map<List<LeaveAllocationVM>>(await _unitOfWork.LeaveAllocations.FindAll(q => q.EmployeeId == currentUser.Id, 
                includes: new List<string> { "LeaveType " }));
            var myLeaveRequests = _mapper.Map<List<LeaveRequestVM>>(await _unitOfWork.LeaveRequests.FindAll(q => q.RequestingEmployeeId == currentUser.Id));

            EmployeeLeaveRequestsViewVM myLeave = new EmployeeLeaveRequestsViewVM
            {
                LeaveAllocations = myLeaveAllocations,
                LeaveRequests = myLeaveRequests
            };

            return View(myLeave);
        }

        // GET: LeaveRequest/Create 
        public async Task<ActionResult> Create()
        {
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
            var leaveTypeItems = leaveTypes.Select(q => new SelectListItem { Text = q.Name, Value = q.Id.ToString() });
            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypeItems
            };

            return View(model);
        }

        // POST: LeaveRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem { Text = q.Name, Value = q.Id.ToString() });
                model.LeaveTypes = leaveTypeItems;

                // Validation #1
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                
                // Validation #2
                if(DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "Start date cannot be futher in the future than the End Date");
                    return View(model);
                }
                
                // Validation #3
                // Retrieve the current user
                var employee = await _userManager.GetUserAsync(User);
                var allocation = await _unitOfWork.LeaveAllocations.Find(q => q.EmployeeId == employee.Id
                                                                            && q.LeaveTypeId == model.LeaveTypeId
                                                                            && q.Period == DateTime.Now.Year);
                int daysRequested = (int)(endDate.Date - startDate.Date).TotalDays;
                if(daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficient days for this allocation type");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId,
                    Comments = model.Comments
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                await _unitOfWork.LeaveRequests.Create(leaveRequest);
                await _unitOfWork.Save();
                
                return RedirectToAction(nameof(MyLeave));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        // GET: LeaveRequest/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> CancelRequest(int id)
        {
            try
            {
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id);
                leaveRequest.Canceled = true;
                _unitOfWork.LeaveRequests.Update(leaveRequest);
                await _unitOfWork.Save();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong. The request has not been canceled");
            }
             
            return RedirectToAction(nameof(MyLeave));
        }

        // GET: LeaveRequest/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequest/Delete/5
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