using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(ViewHubHost.Startup))]
namespace ViewHubHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // At least FireFox doesn't connect to localhost without Cors enabled
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
}
