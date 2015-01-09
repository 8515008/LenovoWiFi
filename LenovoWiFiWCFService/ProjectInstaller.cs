using System.ComponentModel;
using System.Configuration.Install;

namespace Lenovo.WiFi
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
