using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(kmbit.Startup))]
namespace kmbit
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
