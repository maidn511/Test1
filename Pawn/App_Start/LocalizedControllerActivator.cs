﻿using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Pawn.App_Start
{
    public class LocalizedControllerActivator : IControllerActivator
    {
        public IController Create(RequestContext requestContext, Type controllerType)
        {
            return DependencyResolver.Current.GetService(controllerType) as IController;
        }
    }
}