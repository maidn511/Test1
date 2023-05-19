using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Pawn.Logger;

[assembly: OwinStartup(typeof(Pawn.Startup))]
namespace Pawn
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            PawnLog.Instance();
        }
    }
}
