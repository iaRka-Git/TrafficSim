using System.Configuration;
using System.Data;
using System.Windows;
using TrafficSim.Views;

namespace TrafficSim
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            EditorWindow editorWindow = new EditorWindow();
            editorWindow.Show();
        }
    }

}
