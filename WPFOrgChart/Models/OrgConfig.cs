using Haley.Abstractions;
using System;
using System.Collections.Generic;


namespace OrgHierarchy.Models {
    public class OrgConfig : IConfig {
        public string Id { get; }
        public List<Role> Roles { get; set; }
        public List<Department> Departments { get; set; }
        public List<Employee> Employees { get; set; }
        public OrgConfig() {
            Id = Guid.NewGuid().ToString();
            Roles = new List<Role>();
            Departments = new List<Department>();
            Employees = new List<Employee>(); 
        }
    }
}
