using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(KMBit.Startup))]
namespace KMBit
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
