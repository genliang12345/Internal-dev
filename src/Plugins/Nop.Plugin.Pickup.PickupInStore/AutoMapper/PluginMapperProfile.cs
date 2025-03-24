using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Pickup.PickupInStore.Domain;
using Nop.Plugin.Pickup.PickupInStore.Models;

namespace Nop.Plugin.Pickup.PickupInStore.AutoMapper
{
    public class PluginMapperProfile : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public PluginMapperProfile()
        {
            CreateMap<StorePickupPoint, PublicStoreModel>();
        }
    }
}
