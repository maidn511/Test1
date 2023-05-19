using Autofac;
using Autofac.Integration.Mvc;
using AutoMapper;
using Pawn.App_Start;
using Pawn.Configuration;
using Pawn.Libraries;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Pawn.ViewModel.Mapper;

namespace Pawn
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            #region Configuration Autofac

            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinderProvider();

            builder.RegisterModule<AutofacWebTypesModule>();
            builder.RegisterSource(new ViewRegistrationSource());
            builder.RegisterFilterProvider();
            builder.RegisterModule(new CoreModule());
            builder.RegisterModule(new ServicesModule());
            builder.RegisterModule(new MapperModule());

            builder.Register(s => new ActionFilterConfig(s.Resolve<Services.ITransactionProvider>()))
                .AsActionFilterFor<IController>().InstancePerRequest();
            var container = builder.Build();
            AutoMapperConfiguration.Configure();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            #endregion

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_EndRequest()
        {
            var session = HttpContext.Current.Session;
            var context = new HttpContextWrapper(Context);

            if (session != null && context != null && session[Constants.userInfo] == null && context.Request.IsAjaxRequest())
            {
                Context.Response.Clear();
                Context.Response.StatusCode = 401;
            }
        }
    }
}
