using Haley.Utils;

namespace OrgHierarchy.Models {
    public class Department : OrgBaseModel {

        private string _title;
        public string Title {
            get { return _title; }
            set { SetProp(ref _title, value); }
        }

        private string _description;
        public string Description {
            get { return _description; }
            set { SetProp(ref _description, value); }
        }

        public override string ToString() 
        {
            return Title;
        }

        public override object Clone()
        {
            Department clone = new Department();
            this.MapProperties<Department, Department>(clone); //Mapping a Department object to another Department object by the matching property names.
            return clone;
        }

    }
}
