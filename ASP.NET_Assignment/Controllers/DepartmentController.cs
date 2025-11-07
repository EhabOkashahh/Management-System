using ASP.NET.Assignment.PL.DTOs;
using ASP.NET_Assignment.BLL.Interfaces;
using ASP.NET_Assignment.BLL.Repositories;
using ASP.NET_Assignment.DAL.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace ASP.NET.Assignment.PL.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentController(IUnitOfWork unitOfWork , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departments = await _unitOfWork.DepartmentRepository.Value.GetAllAsync();
            ////Dictionary: => Transfer Extra Information From Controller to view
            //// 1.ViewData
            //ViewData["Message"] = "Hello From ViewData";
            //// 2.ViewBag
            //ViewBag.Message = "Message from View Bag";
            //// 2.TempData
            return View(departments);
        }
        [HttpGet]
        public IActionResult Create() {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentDto createDepartmentDto) {

            if (ModelState.IsValid)
            {
                var department = _mapper.Map<Department>(createDepartmentDto);
                await _unitOfWork.DepartmentRepository.Value.AddAsync(department);
                var state = _unitOfWork.ApplyToDB();
                if (state.Result > 0)
                {
                    TempData["Message"] = $"{department.Name} Department is Successfully Created ";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(createDepartmentDto);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return BadRequest("Invalid Id");

            var department = await _unitOfWork.DepartmentRepository.Value.GetAsync(id.Value);
            var empleoyees = await _unitOfWork.EmployeeRepositroy.Value.GetAllAsync();
            ViewData["Employees"] = empleoyees;
            if (department is null) return NotFound(new {StatusCode = 404 , message = $"Dpeartment With Id {id} not found"});

            var createDepartmentDto = _mapper.Map<CreateDepartmentDto>(department);

            ViewBag.Id = id.Value;
            return View(createDepartmentDto);
        }

        [HttpGet]
        public Task<IActionResult> Edit(int? id)
        {
            return Details(id.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit([FromRoute]int? id , CreateDepartmentDto createDepartmentDto) {
            if (ModelState.IsValid) {
                var olddept = await _unitOfWork.DepartmentRepository.Value.GetAsync(id.Value);
                var department = _mapper.Map(createDepartmentDto, olddept);
                await _unitOfWork.DepartmentRepository.Value.Update(department);
                var state = _unitOfWork.ApplyToDB();
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int? id) {
            var department = await _unitOfWork.DepartmentRepository.Value.GetAsync(id.Value);
            if(department is null)
            {
                return View("Models/DeletionUnSuccess");
            }
            _unitOfWork.DepartmentRepository.Value.Delete(department);
            var res = _unitOfWork.ApplyToDB();
            if (res.Result > 0)
            {
                return View("Models/DeletionSuccess");
            }
            return View("Models/DeletionUnSuccess");
        }
    }  
}
