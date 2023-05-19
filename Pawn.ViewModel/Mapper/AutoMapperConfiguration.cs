using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.ViewModel.Mapper
{
    public class AutoMapperConfiguration
    {
        public static MapperConfiguration Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            config.CreateMapper();
            return config;
        }
    }
}
