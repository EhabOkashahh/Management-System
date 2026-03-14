using ASP.NET.Assignment.PL.DTOs;
using ASP.NET.Assignment.PL.Helpers;
using ASP.NET_Assignment.BLL.Interfaces;
using ASP.NET_Assignment.BLL.Repositories;
using ASP.NET_Assignment.DAL.Models;
using AspNetCoreGeneratedDocument;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.CodeDom;
using System.Threading.Tasks;

namespace ASP.NET.Assignment.PL.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _Mapper;

        public EmployeesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _Mapper = mapper;
        }

        public async Task<IActionResult> Index(string? SearchText)
        {
            IEnumerable<Employee> Employees;

            if (String.IsNullOrEmpty(SearchText)) Employees = await _unitOfWork.EmployeeRepositroy.Value.GetAllAsync();
            else Employees = await _unitOfWork.EmployeeRepositroy.Value.GetByNameAsync(SearchText);
            return View(Employees);

        }
        public async Task<IActionResult> Search(string SearchText) 
        {
            try
            {
                IEnumerable<Employee> Employees;

                if (string.IsNullOrEmpty(SearchText))
                    Employees = await _unitOfWork.EmployeeRepositroy.Value.GetAllAsync();
                else
                    Employees = await _unitOfWork.EmployeeRepositroy.Value.GetByNameAsync(SearchText);

                return PartialView("_EmployeesTablePartialView", Employees);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // Logs to console
                return StatusCode(500, ex.Message); // So you can see error in browser
            }
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var Departments = await _unitOfWork.DepartmentRepository.Value.GetAllAsync();
            ViewData["Departments"] = Departments;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeDTO createEmployeeDTO)
        {

            if (ModelState.IsValid)
            {

                if (createEmployeeDTO.Image is not null)
                {
                    createEmployeeDTO.ImageName = AttachmentsSettings.Upload(createEmployeeDTO.Image);
                }
                else if(createEmployeeDTO.Image is null) createEmployeeDTO.ImageName = "DefaultPFP.png";

                    var employee = _Mapper.Map<Employee>(createEmployeeDTO);
                await _unitOfWork.EmployeeRepositroy.Value.AddAsync(employee);
                var count = _unitOfWork.ApplyToDB();
                if (count.Result > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(createEmployeeDTO);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var Departments = await _unitOfWork.DepartmentRepository.Value.GetAllAsync();
            ViewData["Departments"] = Departments;
            var Employee = await _unitOfWork.EmployeeRepositroy.Value.GetAsync(id.Value);
            var employee = _Mapper.Map<CreateEmployeeDTO>(Employee);

            ViewData["Id"] = id.Value;
            return View(employee);
        }
        [HttpGet]
        public Task<IActionResult> Edit(int? id)
        {
            return Details(id.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromRoute] int? id, CreateEmployeeDTO createEmployeeDTO)
        {
            if (ModelState.IsValid)
            {
                if (createEmployeeDTO.ImageName is not null)
                {
                    AttachmentsSettings.Delete(createEmployeeDTO.ImageName);
                }
                if (createEmployeeDTO.Image is not null)
                {

                    createEmployeeDTO.ImageName = AttachmentsSettings.Upload(createEmployeeDTO.Image);
                }

                var Departments = await _unitOfWork.DepartmentRepository.Value.GetAllAsync();
                ViewData["Departments"] = Departments;
                var oldemp = await _unitOfWork.EmployeeRepositroy.Value.GetAsync(id.Value);

                var employee = _Mapper.Map(createEmployeeDTO, oldemp);

                await _unitOfWork.EmployeeRepositroy.Value.Update(employee);
                await _unitOfWork.ApplyToDB();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete([FromRoute] int? id)
        {
            var employee = await _unitOfWork.EmployeeRepositroy.Value.GetAsync(id.Value);
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string deleteOption)
        {


            var employee = await _unitOfWork.EmployeeRepositroy.Value.GetAsync(id.Value);


            if (deleteOption == "1")
            {
                if (employee.IsActive)
                {
                    employee.IsActive = false;
                    await _unitOfWork.EmployeeRepositroy.Value.Update(employee);
                    var count = _unitOfWork.ApplyToDB();
                    if (count.Result > 0) return RedirectToAction(nameof(Index));
                    {
                        ViewBag.ErrorMessage = "Something Wrong Happend";
                        return await Delete(id);
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "This employee is already deactivated.";
                }
            }
            else if (deleteOption == "2")
            {
                if (!employee.IsDeleted)
                {
                    employee.IsActive = false;
                    _unitOfWork.EmployeeRepositroy.Value.Delete(employee);
                    var count = _unitOfWork.ApplyToDB();
                    if (count.Result > 0) return RedirectToAction(nameof(Index));
                    {
                        ViewBag.ErrorMessage = "Something Wrong Happend";
                        return await Delete(id);
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "This employee is already deleted.";
                }
            }
            else ViewBag.ErrorMessage = "Please Choose Deletion Method.";

            return View("Delete");
        }

        public IActionResult DeleteImage([FromRoute] int? id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage([FromRoute] int? id, string? imageName)
        {
            var employee = await _unitOfWork.EmployeeRepositroy.Value.GetAsync(id.Value);
            if (employee is null) return View("Error");
            if (employee.ImageName is null) return View("Error");
            AttachmentsSettings.Delete(employee.ImageName);
            employee.ImageName = "DefaultPFP.png";
            await _unitOfWork.EmployeeRepositroy.Value.Update(employee);
            await _unitOfWork.ApplyToDB();
            return RedirectToAction(nameof(Index));
        }

    }  
}