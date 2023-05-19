using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pawn
{
    [Microsoft.AspNet.SignalR.Hubs.HubName("notificationHub")]
    public class Connection : Hub
    {
        public void send(string content)
        {
            Clients.All.addMessage(content);
        }
    }
}