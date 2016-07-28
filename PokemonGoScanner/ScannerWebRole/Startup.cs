using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ScannerWebRole.Startup))]
namespace ScannerWebRole
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
