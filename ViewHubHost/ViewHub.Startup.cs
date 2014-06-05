using Owin;
using Microsoft.Owin.Cors;

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
