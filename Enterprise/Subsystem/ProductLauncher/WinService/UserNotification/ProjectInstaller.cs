using System.ComponentModel;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
