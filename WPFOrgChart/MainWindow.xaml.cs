using System.Windows;
using OrgHierarchy.ViewModels;

namespace OrgHierarchy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() 
        {
            InitializeComponent();
            //Add datacontext is a dependency property for data binding of source
            this.DataContext = new MainViewModel(); 
        }
    }
}
