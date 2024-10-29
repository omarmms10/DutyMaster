using Microsoft.AspNetCore.Mvc;
using Task_Management_System;
using Task_Management_System.Models;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Task_Management_System.Controllers
{
    [Authorize]
    public class DutyController : Controller
    {
        private readonly TaskManagementDbContext _context;

        public DutyController(TaskManagementDbContext context)
        {
            _context = context;
        }
        [AllowAnonymous]

        public IActionResult WebsiteLayout()
        {
            return View();
        }
        public IActionResult Analytics()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Retrieve and process user-specific data
            var completedDuties = _context.Duties.Count(d => d.IsCompleted && d.UserId == userId);
            var pendingDuties = _context.Duties.Count(d => !d.IsCompleted && d.UserId == userId);

            var dutiesByCategory = _context.Duties
                .Where(d => d.UserId == userId)
                .GroupBy(d => d.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();

            var dutiesByPriority = _context.Duties
                .Where(d => d.UserId == userId)
                .GroupBy(d => d.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToList();

            // Pass serialized data to the ViewBag
            ViewBag.CompletedDuties = completedDuties;
            ViewBag.PendingDuties = pendingDuties;
            ViewBag.DutiesByCategory = dutiesByCategory.Select(c => c.Category).ToArray();
            ViewBag.CategoryCounts = dutiesByCategory.Select(c => c.Count).ToArray();
            ViewBag.DutiesByPriority = dutiesByPriority.Select(p => p.Priority).ToArray();
            ViewBag.PriorityCounts = dutiesByPriority.Select(p => p.Count).ToArray();

            return View();
        }

        public IActionResult Index(string title = null, int page = 1, int pageSize = 10, string priority = null, string category = null, bool? isCompleted = null, int? year = null, int? month = null, int? day = null, int? hour = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var duties = _context.Duties.Where(d => d.UserId == userId).AsQueryable();

            // Filter by title
            if (!string.IsNullOrEmpty(title))
            {
                duties = duties.Where(d => d.Title.Contains(title));
            }

            // Filtering by priority
            if (!string.IsNullOrEmpty(priority))
            {
                duties = duties.Where(d => d.Priority == priority);
            }

            // Filtering by category
            if (!string.IsNullOrEmpty(category))
            {
                duties = duties.Where(d => d.Category == category);
            }

            // Filtering by status
            if (isCompleted.HasValue)
            {
                duties = duties.Where(d => d.IsCompleted == isCompleted.Value);
            }

            // Filtering by date parts
            if (year.HasValue)
            {
                duties = duties.Where(d => d.DueDate.Year == year.Value);
            }
            if (month.HasValue)
            {
                duties = duties.Where(d => d.DueDate.Month == month.Value);
            }
            if (day.HasValue)
            {
                duties = duties.Where(d => d.DueDate.Day == day.Value);
            }
            if (hour.HasValue)
            {
                duties = duties.Where(d => d.DueDate.Hour == hour.Value);
            }

            // Pagination
            var paginatedDuties = duties
                .OrderBy(t => t.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get distinct categories from the user's duties
            var categories = _context.Duties
                .Where(d => d.UserId == userId)
                .Select(d => d.Category)
                .Distinct()
                .ToList();

            // Pass categories to the view
            ViewBag.Categories = categories;

            return View(paginatedDuties);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Models.Duty duty)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (ModelState.IsValid)
            {
                duty.CreatedAt = DateTime.Now;
                duty.UpdatedAt = null;  // No updates yet
                duty.UserId = userId; // Assign the duty to the logged-in user
                _context.Duties.Add(duty);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Duty created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            // If ModelState is not valid, return the view with the same model to show validation errors
            return PartialView("_CreateDutyPartial", duty);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var duty = _context.Duties.FirstOrDefault(d => d.TaskId == id && d.UserId == userId);
            if (duty == null) return NotFound();

            ViewBag.Categories = _context.Duties
                .Where(d => d.UserId == userId)
                .Select(d => d.Category)
                .Distinct()
                .ToList();

            return View(duty); // If using a full page for editing, but not needed for modal
        }

        [HttpPost]
        public IActionResult Edit(Models.Duty duty)
        {
            if (ModelState.IsValid)
            {
                var existingDuty = _context.Duties.Find(duty.TaskId);
                if (existingDuty == null || existingDuty.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                {
                    TempData["ErrorMessage"] = "Duty not found or you do not have permission to edit this duty!";
                    TempData["ToastType"] = "error";
                    return NotFound();
                }

                // Update the fields of the existing duty
                existingDuty.Title = duty.Title;
                existingDuty.Description = duty.Description;
                existingDuty.DueDate = duty.DueDate;
                existingDuty.Priority = duty.Priority;
                existingDuty.Category = duty.Category;
                existingDuty.IsCompleted = duty.IsCompleted;
                existingDuty.UpdatedAt = DateTime.Now;

                _context.Duties.Update(existingDuty);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Duty updated successfully!";
                TempData["ToastType"] = "info"; // Info type for edit
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_EditDutyPartial", duty);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var duty = _context.Duties.FirstOrDefault(d => d.TaskId == id && d.UserId == userId);
            if (duty == null)
            {
                TempData["ErrorMessage"] = "Duty not found or you do not have permission to delete this duty!";
                TempData["ToastType"] = "error"; // Error type for duty not found
                return NotFound();
            }

            _context.Duties.Remove(duty);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Duty deleted successfully!";
            TempData["ToastType"] = "error"; // Error type for deletion
            return RedirectToAction(nameof(Index));
        }
    }
}
