using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;//For IMultiValueConverter
using System.Collections.ObjectModel;
using OrgHierarchy.Models;

namespace OrgHierarchy.Converters
{
    public class FilteredRoleConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            //We expect two values.
            //Value[0] should be List of all roles
            //Value[1] should be RoleIdToIgnore
            //Value[2] should be FilterBy_DepartmentId
            //Value[3] should be Bool to let us know if we are enabling cross dept or not (to consider dept filter or not).
            //Value[4] should be in Int (for minimum level of roles)

            if (values == null) return null;
            if (values.Length == 1) return values[0]; //Just send all roles.

            ObservableCollection<Role> allroles = null;
            string roleId_toignore = string.Empty;
            string deptID_tofilter = string.Empty;
            //bool filterbyDept = false; //If cross reference is false, then we should filterby department.
            int min_level = 1;

            for (int i = 0; i < values.Length; i++) {
                switch (i) {
                    case 0:
                        allroles = values[i] as ObservableCollection<Role>;
                        break;
                    case 1:
                        roleId_toignore = values[i] as string;
                        break;
                    case 2:
                        deptID_tofilter = values[i] as string;
                        break;                    
                    case 3:
                        if (values[i] is int level) {
                            min_level = level;
                        }
                        break;
                }
            }
            if (allroles == null || allroles.Count == 0) return null;
            IEnumerable<Role> result;
           
            result = allroles.Where(p=> p.Id != roleId_toignore);

            if (min_level > 1) {
                result = result.Where(p=> p.Level > min_level);
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
