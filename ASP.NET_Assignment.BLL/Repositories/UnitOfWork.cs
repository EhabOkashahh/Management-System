using ASP.NET_Assignment.BLL.Interfaces;
using ASP.NET_Assignment.DAL.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP.NET_Assignment.BLL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public Lazy<IDepartmentRepository> DepartmentRepository { get; }

        public Lazy<IEmployeeRepositroy> EmployeeRepositroy { get; }

        private readonly AssignmentDbContext _context;

        public UnitOfWork(AssignmentDbContext Context)
        {

            _context = Context;
            DepartmentRepository = new Lazy<IDepartmentRepository>(() => new DepartmentRepository(_context));
            EmployeeRepositroy = new Lazy<IEmployeeRepositroy>(() => new EmployeeRepository(_context));

            //EmployeeRepository = new EmployeeRepository(_context);
         //Lazy<DepartmentRepository> lazyobj = new Lazy<DepartmentRepository>(() => new DepartmentRepository(_context)); => this is used inside to tell the CLR Don't use the object until its called 
         // to avoid this scenario => when i call "UnitOfWork" to use DepartmentRepo the EmployeeRepo will call also but i will not use so it will UnderTheHood create an LazyObject
         // but the data assined to it's CTOR didn't intialize yet until its used
            
        }

        public async Task<int> ApplyToDB()
        {
            return _context.SaveChanges();
        }
    }
}
