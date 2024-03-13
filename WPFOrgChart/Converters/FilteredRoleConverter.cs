using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;//For IMultiValueConverter
using OrgHierarchy.Models;

namespace OrgHierarchy.Converters
{
    public class FilteredRoleConverter : IMultiValueConverter
    {
        //converting multiple input values into a filtered list of roles based on specified criteria. 
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // We expect four values:
            // Value[0]: All roles (ObservableCollection<Role>)
            // Value[1]: RoleId to ignore (string)
            // Value[2]: DepartmentId to filter by (string)
            // Value[3]: Minimum level of roles (int)

            if (values == null || values.Length != 4)
                return null;

            if (!(values[0] is IEnumerable<Role> allRoles))
                return null;

            string roleIdToIgnore = values[1] as string;
            string departmentIdToFilter = values[2] as string;
            //int minLevel = values[3] as int? ?? 1;
            int minLevel = 1; // Default value

            // Check if the fourth value is an integer
            if (values[3] is int level)
            {
                minLevel = level;
            }

            // Filter roles based on the provided criteria
            IEnumerable<Role> filteredRoles = allRoles.Where(role =>
                (role.DepartmentId == departmentIdToFilter || departmentIdToFilter == null) && // Filter by department if provided
                role.Id != roleIdToIgnore && // Exclude role to ignore
                role.Level >= minLevel // Filter by minimum level
            );

            return filteredRoles.ToList();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
