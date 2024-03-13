using System.Windows;

namespace OrgHierarchy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) 
        {
            Initialize();
            //Finally call the Main Window.
            MainWindow mw = new MainWindow();
            mw.ShowDialog();
        }

        void Initialize()
        {
            //to initialize services or classes or objects as needed.

        }
    }
}
