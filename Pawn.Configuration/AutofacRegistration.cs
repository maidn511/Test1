using Autofac;
using AutoMapper;
using Pawn.Core.DataAccess;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pawn.Configuration
{
    public class CoreModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.RegisterType<PawnEntities>().AsSelf();
        }
    }

    public class ServicesModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.Load("Pawn.Services"))
                   .Where(s => s.Name.EndsWith("Services"))
                   .AsImplementedInterfaces()
                   .InstancePerDependency();

            builder.RegisterType<TransactionProvider>().As<ITransactionProvider>().InstancePerRequest();
        }
    }

    public class MapperModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            builder.RegisterAssemblyTypes(assemblies)
                .Where(s => typeof(Profile).IsAssignableFrom(s) && !s.IsAbstract && s.IsPublic)
                .As<Profile>();

            builder.Register(s => new MapperConfiguration(m =>
            {
                foreach (var profile in s.Resolve<IEnumerable<Profile>>())
                {
                    m.AddProfile(profile);
                }
            }));

            builder.Register(s => s.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().SingleInstance();
        }
    }

}
