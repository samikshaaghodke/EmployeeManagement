using Haley.Utils;

namespace OrgHierarchy.Models
{
    public class Role : OrgBaseModel 
    {

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProp(ref _title, value); }
        }

        private string _reportsTo;
        public string ReportsTo
        {
            get { return _reportsTo; }
            set { SetProp(ref _reportsTo, value); }
        }

        private int _level = 1; //All role starts at level one
        public int Level 
        {
            get { return _level; }
            set { SetProp(ref _level, value); }
        }

        private string _aboveRole;
        public string AboveRole 
        {
            get { return _aboveRole; }
            set { SetProp(ref _aboveRole, value);}
        }

        private string _department;
        public string DepartmentId
        {
            get { return _department; }
            set { 
                SetProp(ref _department, value); 
            }
        }

        public override string ToString() {
            return Title;
        }

        public override object Clone()
        {
            //Use Map properties from Haley.Utils.
            Role clone = new Role(); //Create the object and send in as reference
            this.MapProperties<Role, Role>(clone); //Mapping a Role object to another Role object by the matching property names.
            return clone;
        }

    }
}
