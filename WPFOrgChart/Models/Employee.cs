
using Haley.Utils;

namespace OrgHierarchy.Models {
    public class Employee : OrgBaseModel {
        private string _firstName;
        public string FirstName {
            get { return _firstName; }
            set { SetProp(ref _firstName, value); }
        }

        private string _lastName;
        public string LastName {
            get { return _lastName; }
            set { SetProp(ref _lastName, value); }
        }

        private string _emailID;
        public string EmailId {
            get { return _emailID; }
            set { SetProp(ref _emailID, value); }
        }

        private string _roleId;
        public string RoleId {
            get { return _roleId; }
            set { SetProp(ref _roleId, value); }
        }

        private string _deptId;
        public string DepartmentId {
            get { return _deptId; }
            set { SetProp(ref _deptId, value); }
        }

        private string _possibleManager;
        public string PossibleManager {
            get { return _possibleManager; }
            set { SetProp(ref _possibleManager, value); }
        }

        public void SetName(string firstname, string lastname) {
            FirstName = firstname;
            LastName = lastname;
        }

        public override string ToString() {
            return $@"{FirstName} {LastName}";
        }

        public override object Clone() {
            Employee clone = new Employee();
            this.MapProperties<Employee, Employee>(clone); //Mapping a Role object to another Role object by the matching property names.
            return clone;
        }

        public Employee() {
           //A Employee will have a role and he will belong to some department.
        }
    }
}
