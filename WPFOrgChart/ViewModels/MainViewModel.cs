using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections; //For adding the generic IEnumberable
using System.Windows.Input;//For ICommand
using System.Collections.ObjectModel;
using OrgHierarchy.Enums;
using OrgHierarchy.Models;
using System.Windows.Threading;
using System.Data;
using Haley.Services;

namespace OrgHierarchy.ViewModels {
    public class MainViewModel : BaseVM, IConfigHandler
    {
        #region Attributes
        IDialogService _ds = new DialogService(); // to show dialog box if you select reporting to lower level
        IConfigManager _cfgMgr = new ConfigManagerService() { FileExtension = "org" };
        OrgConfig _configCache = new OrgConfig(); //cache variable
        TargetType CurrentTab = TargetType.Employee;
        //Setup a timer to autoclear the textblock validation message in 2 seconds.
        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2.5) };
        //Since this is a simple application, directly initiating a config manager. For complex/enterprise apps, consider using a dependency injection and reuse config manager.
        #endregion

        #region Properties
        public Guid UniqueId { get; set; }

        #region Current Elements
        //An object for the display/editable value and an IEnumerable for the grid results

        private object _current = new Employee();

        private IEnumerable _currentValues;

        private CurrentView _displayView = CurrentView.DataEntry;

        private string _message;

        private bool _messageVisible = false;

        public object Current {
            get { return _current; }
            set { SetProp(ref _current, value); }
        }
        public IEnumerable CurrentValues {
            get { return _currentValues; }
            set { _currentValues = value; OnPropertyChanged(); } //notifying the UI about the change.
        }
        public CurrentView DisplayView {
            get { return _displayView; }
            set { SetProp(ref _displayView, value); }
        }
        public string Message {
            get { return _message; }
            set { SetProp(ref _message, value); }
        }
        public bool MessageVisible {
            get { return _messageVisible; }
            set { SetProp(ref _messageVisible, value); }
        }
        #endregion

        #region Collections
        private ObservableCollection<Department> _departments = new ObservableCollection<Department>();
        private ObservableCollection<Employee> _employees = new ObservableCollection<Employee>();
        private ObservableCollection<Role> _roles = new ObservableCollection<Role>();

        public ObservableCollection<Department> Departments {
            get { return _departments; }
            set { SetProp(ref _departments, value); }
        }

        public ObservableCollection<Employee> Employees {
            get { return _employees; }
            set { SetProp(ref _employees, value); }
        }
        public ObservableCollection<Role> Roles {
            get { return _roles; }
            set { SetProp(ref _roles, value); }
        }
        #endregion

        #region Commands
        public ICommand CMDAddUpdate => new DelegateCommand(addUpdate);
        public ICommand CMDChangeView => new DelegateCommand<CurrentView>((p) => { DisplayView = p; });
        public ICommand CMDDeleteSelectedItems => new DelegateCommand<object>(DeleteSelected);
        public ICommand CMDEditCurrent => new DelegateCommand<object>(EditSelected);
        public ICommand CMDKeyDown => new DelegateCommand<object>(HandleKeyDown);
        public ICommand CMDReset => new DelegateCommand(reset);
        public ICommand CMDResetReportings => new DelegateCommand(resetReportings);
        public ICommand CMDTabChanged => new DelegateCommand<object>(tabChanged);
        #endregion

        #endregion

        public MainViewModel()
        {
            Initialize();
        }

        #region Config Management
        //These methods interact with the IConfigHandler interface to manage application configurations.
        public Task OnConfigLoaded(IConfig config)
        {
            //When config is loaded from file
            _configCache = config as OrgConfig; 
            //On first load,We go with overwrite (assuming we only load during the startup of the application)
            if (_configCache == null) return null;
           
            Departments = new ObservableCollection<Department>(_configCache.Departments?.ToList());
            Roles = new ObservableCollection<Role>(_configCache.Roles?.ToList());
            Employees = new ObservableCollection<Employee>(_configCache.Employees?.ToList());

            ProcessRoleLevels();
            ProcessEmployeeManagers();

            return Task.FromResult(true);
        }
        public IConfig OnConfigSaving() {
            //send in a value that will be saved to the file.
            //Fill the config file 
            _configCache.Departments = Departments.ToList();
            _configCache.Roles = Roles.ToList();
            _configCache.Employees = Employees.ToList();

            //Ids are chaning on loading.
            return _configCache;
        }

        public IConfig PrepareDefaultConfig()
        {
            //when creating new config.
            return new OrgConfig(); 
        }
        #endregion

        #region Core Processing
        void addUpdate() 
        {            
            // validate within each block.
            //Based on the current tab type, fetch the current object and add to the collection. 
            switch (CurrentTab) 
            {
                case TargetType.Employee:
                    if (!ProcessEmployee()) return;
                    break;
                case TargetType.Role:
                    if (!ProcessRole()) return;
                    break;
                case TargetType.Department:
                    if (!ProcessDepartment()) return;
                    break;
            }
            //After adding new value, also reset
            reset();
        }
        bool ProcessDepartment() {
            var target = Current as Department;

            //Validate
            if (target == null || string.IsNullOrWhiteSpace(target.Title))
            {
                SetMessage();
                return false;
            }
            var existingObj = Departments.FirstOrDefault(p => p.Id == target.Id);
            //Add or update
            if (existingObj != null)
            {
                //Exists already, now get the element and replace it.
                var index = Departments.IndexOf(existingObj);
                Departments[index] = target; //replaced.
            } 
            else 
            {
                Departments.Add(target); //added
            }
            return true;
        }
        bool ProcessEmployee() {
            var target = Current as Employee;

            //Validate
            if (target == null || string.IsNullOrWhiteSpace(target.FirstName) || string.IsNullOrWhiteSpace(target.LastName))
            {
                SetMessage();
                return false;
            }
            var existingObj = Employees.FirstOrDefault(p => p.Id == target.Id);
            //Add or update
            if (existingObj != null) 
            {
               //Exists already, now get the element and replace it.
                var index = Employees.IndexOf(existingObj);
                Employees[index] = target; //replaced.
            } 
            else
            {
                Employees.Add(target); //added
            }
            ProcessEmployeeManagers();
            return true;
        }
        bool ProcessRole() {
            var target = Current as Role;

            //Validate
            if (target == null || string.IsNullOrWhiteSpace(target.Title) || string.IsNullOrWhiteSpace(target.DepartmentId))
            {
                SetMessage();
                return false;
            }

            var existingObj = Roles.FirstOrDefault(p => p.Id == target.Id);
            //Add or update
            if (existingObj != null) 
            {
                //We need to find if the new item is reporting to. (not the exsiting item)
                if (!string.IsNullOrWhiteSpace(target.ReportsTo)) {
                    // we are planning to report to another role.
                    //Is this another role, above our role ?
                    var possible_supervisor = Roles.FirstOrDefault(p => p.Id == target.ReportsTo);
                    if (possible_supervisor != null && possible_supervisor.Level < target.Level)
                    {                        
                        var  confirmation = _ds.ShowDialog("Conflict", $@"You are trying to report to a Role which is at a lower level than the existing one. {Environment.NewLine}This might result in breaking the reporting structure of the target role.  {Environment.NewLine} Do you wish to proceed?", NotificationIcon.Warning, DialogMode.Confirmation, blurOtherWindows: true);
                        if (!confirmation.DialogResult.HasValue || !confirmation.DialogResult.Value) 
                            return false;
                      
                        possible_supervisor.ReportsTo = string.Empty; //Now we remove the reporting head.
                    }
                }

                //Exists already, now get the element and replace it.
                var index = Roles.IndexOf(existingObj);
                Roles[index] = target; //replaced.   
            } 
            else
            {
                    Roles.Add(target); //added
            }
            //Whenever we process a role (add or edit), we need to process their levels as well
            ProcessRoleLevels();
            ProcessEmployeeManagers();
            return true;
        }
        private void ProcessEmployeeManagers()
        {
            foreach (var employee in Employees) 
            {
                //If he/she has a role id, get the role id and Role is mandatory, so every employee will have one role 
                var thisRole = Roles.FirstOrDefault(p => p.Id == employee.RoleId); //This role will be based on a department as well.
                if (string.IsNullOrWhiteSpace(thisRole?.ReportsTo)) continue; //This role doesn't report to anyone, yet.

                //If any employee has this role, then he/she becomes the manager.               
                var possible_mgr = Employees.FirstOrDefault(q => q.RoleId == thisRole?.ReportsTo);
                if (possible_mgr != null)
                {
                    employee.PossibleManager = $@"{possible_mgr.FirstName} {possible_mgr.LastName}";
                }
            }
        }
      
        void IncrementRoleLevel(Role target)
        {
            //If we increase the level of a target, we need to inrease its parent too.
            target.Level += 1 ;

            if (!string.IsNullOrWhiteSpace(target.ReportsTo))
            {
                var parent = Roles.FirstOrDefault(p => p.Id == target.ReportsTo);

                if (parent != null)
                {
                    IncrementRoleLevel(parent);
                }
            }
        }
        void ProcessRoleLevels()
        {
            //We can reset all levels (just in case some reporting strucutre got changed)
            foreach (var role in Roles) 
            {
                role.Level = 1;               
            }
            //Whenever an object is added, we need to process all levels.
            //For each role, we define their level based on above and report roles.

            ////Handle Direct reportings (in separate loop)
            foreach (var role in Roles)
            {
                if (!string.IsNullOrWhiteSpace(role.ReportsTo))
                {
                    var supervising_role = Roles.FirstOrDefault(p => p.Id == role.ReportsTo);
                    if (supervising_role == null)
                    {
                        role.ReportsTo = string.Empty; //reportsto role is empty
                        continue;
                    }

                    //Now based on our level, we are going to increase the reporting level.
                    if (supervising_role.Level <= role.Level)
                    {
                        //Only if the supervising level is also equal to this level or less than this level, increase it.
                        supervising_role.Level = role.Level; //increase the supervising level.
                        IncrementRoleLevel(supervising_role);
                    }
                }
            }


            //Handle Above Levels (in separate loop)
            foreach (var role in Roles)
            { 
                if (!string.IsNullOrWhiteSpace(role.AboveRole)) 
                {
                    //this role is above some other role.
                    //get the aboveRole
                    var above_role = Roles.FirstOrDefault(p => p.Id == role.AboveRole);
                    if (above_role == null)
                    {
                        //May be this role is removed.
                        role.AboveRole = string.Empty;
                        continue;
                    }

                    //If above role also exits, get it's level and increment this level
                    //if we are not already above the specified level, then we increase
                    if (role.Level <= above_role.Level)
                    {
                        //only when this role is less or equal to the above role
                        //First bring the role to the level of the above role.
                        role.Level = above_role.Level;
                        IncrementRoleLevel(role);//we need to recursively do this.
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        void clear() {
            Current = null;
        }

        private void DeleteSelected(object obj) 
        {            
            //As per MSDN, SelectedItemCollection is implementing ObservableCollection<Object>
            if (obj == null || !(obj is ObservableCollection<object> col) || col.Count == 0) return;

            //Now, get the first object of the col and try to cast it based on the current tab.
            switch (CurrentTab)
            {
                case TargetType.Employee:
                    if (!(col.First() is Employee)) return;
                    var empoyees_todel = col.Cast<Employee>().ToList();
                    foreach (var employee in empoyees_todel)
                    {
                        Employees.Remove(employee);
                    }
                    break;
                case TargetType.Role:
                    if (!(col.First() is Role)) return;
                    var roles_todel = col.Cast<Role>().ToList();
                    foreach (var role in roles_todel) {
                        Roles.Remove(role);
                    }
                    break;
                case TargetType.Department:
                    if (!(col.First() is Department)) return;
                    var depts_todel = col.Cast<Department>().ToList();
                    foreach (var dept in depts_todel) {
                        Departments.Remove(dept);
                    }
                    break;
                default:
                    break;
            }
            reset(); //so that the new collection is also updated in the UI.
        }

        private void EditSelected(object obj)
        {
            //Incoming obj should be one of the currentValues.
            if (obj == null || obj.GetType() != GetCurrentType() || !(obj is OrgBaseModel obj_base)) return;
            
            Current = obj_base.Clone();
        }

        Type GetCurrentType() 
        {
            switch (CurrentTab)
            {
                case TargetType.Employee:
                    return typeof(Employee);
                case TargetType.Role:
                    return typeof(Role);
                case TargetType.Department:
                    return typeof(Department);
            }
            return null;
        }

        private void HandleKeyDown(object obj)
        {
            //This is monitoring all key downs. 
            if (!(obj is HotKeyArgs hkargs)) return;

            if (hkargs.PressedKeys.First() == Key.Escape) 
            {
                //reset everything if we press escape
                reset();
            } else if (hkargs.PressedKeys.First() == Key.Enter) 
            {
                //Add or update value
                addUpdate();
            } 
            else if (hkargs.PressedKeys.Count() == 2) 
            {
                if (hkargs.PressedKeys.Contains(Key.LeftCtrl) && hkargs.PressedKeys.Contains(Key.S)) {
                    //We need to save the file.
                    _cfgMgr.SaveAll();
                }
            }
          
        }

        private void HandlerTimer(object? sender, EventArgs e) {
            timer.Stop();
            SetMessage(null); //this will remove and hide.
        }

        private void Initialize() {
            timer.Tick += HandlerTimer;
            //Using config manager from haley to store text file locally and MainViewModel is properly initialized with a unique identifierand registered with a configuration manager
            UniqueId = Guid.NewGuid();           
            _cfgMgr.TryRegister(nameof(MainViewModel), typeof(OrgConfig), _configCache, this, out _);
        }

        private void reset() {            
            switch (CurrentTab) {
                case TargetType.Employee:
                    Current = new Employee();
                    CurrentValues = new ObservableCollection<Employee>(Employees.ToList());
                    break;
                case TargetType.Role:
                    Current = new Role();
                    //If we directly set the value, then same observable collection gets shared by two places and thus, "NewPlaceHolderItem" shows up (in the combo box). to avoid that, we initiate a new list.
                    CurrentValues = new ObservableCollection<Role>(Roles.ToList());
                    break;
                case TargetType.Department:
                    Current = new Department();
                    CurrentValues = new ObservableCollection<Department>(Departments.ToList());
                    break;
                default:
                    break;
            }
        }

        private void resetReportings()
        {
            if (Current is Role role)
            {
                role.AboveRole = String.Empty;
                role.ReportsTo = String.Empty;
            }
        }
        void SetMessage(string msg = "Please enter all mandatory values")
        {
            Message = msg;
            if (!string.IsNullOrEmpty(msg)) 
            {
                //if message is not null, initiate the timer.
                timer.Stop(); //incase we are trying to set multiple messages in succession.
                timer.Start();
                MessageVisible = true;
            } 
            else 
            {
                MessageVisible = false;
            }
        }
      
        private void tabChanged(object obj) 
        {
            //Even when we move focus to another control, it is getting triggered
            if (!(obj is TargetType targetEnum)) return;

            //Now it is persistent
            if (CurrentTab == targetEnum) return; //We didn't change the current tab.

            //Whenever tab changed, let us reset the values irrespective of what has been changed.
            clear(); //Still need to identify the first tab on initialization. 
            //receive the tag of the selected item.
            //Associate an enum with the tag, so that it will be easy for us to identify.
            //Whenever the tabselection changed, we initialize or change or reset the current object and also the current enumerable values.
            CurrentTab = targetEnum;
            reset();
        }
        #endregion
        
    }
}
