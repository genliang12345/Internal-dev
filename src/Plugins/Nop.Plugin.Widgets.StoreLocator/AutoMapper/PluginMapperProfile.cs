using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Widgets.StoreLocator.Domain;
using Nop.Plugin.Widgets.StoreLocator.Models;

namespace Nop.Plugin.Widgets.StoreLocator.AutoMapper
{
    public class PluginMapperProfile : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public PluginMapperProfile()
        {
            CreateMap<StoreLocationModel, StoreLocation>().ReverseMap();
            CreateMap<StoreLocation, PublicStoreModel>();
        }
    }
}
