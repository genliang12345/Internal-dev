using Nop.Core;
using Nop.Data;
using Nop.Plugin.Widgets.StoreLocator.Domain;

namespace Nop.Plugin.Widgets.StoreLocator.Services
{
    public interface IStoreLocatorService
    {
        Task<IPagedList<StoreLocation>> GetAllStoreLocatorsAsync(int pageSize, int pageIndex);
        Task DeleteStoreLocationAsync(int[] ints);
        Task InsertStoreLocation(StoreLocation storeLocation);
        Task<StoreLocation> GetStoreLocationById(int id);
        Task UpdateStoreLocationAsync(StoreLocation storeLocation);
        Task<List<StoreLocation>> GetPublicStoreLocations();
    }
    public class StoreLocatorService : IStoreLocatorService
    {
        private readonly IRepository<StoreLocation> _storeLocationRepository;

        public StoreLocatorService(IRepository<StoreLocation> storeLocationRepository)
        {
            _storeLocationRepository = storeLocationRepository;
        }



        public async Task<IPagedList<StoreLocation>> GetAllStoreLocatorsAsync(int pageSize, int pageIndex)
        {
            return await _storeLocationRepository.Table.ToPagedListAsync(pageIndex - 1, pageSize);
        }
        public async Task DeleteStoreLocationAsync(int[] ids)
        {
            await _storeLocationRepository.DeleteAsync(logItem => ids.Contains(logItem.Id));
        }

        public async Task InsertStoreLocation(StoreLocation storeLocation)
        {
            await this._storeLocationRepository.InsertAsync(storeLocation);
        }

        public async Task<StoreLocation> GetStoreLocationById(int id)
        {
            return await _storeLocationRepository.GetByIdAsync(id);
        }

        public async Task UpdateStoreLocationAsync(StoreLocation storeLocation)
        {
            await this._storeLocationRepository.UpdateAsync(storeLocation);
        }

        public async Task<List<StoreLocation>> GetPublicStoreLocations()
        {
            return await _storeLocationRepository.Table.Where(x => x.IsActive).ToListAsync();
        }
    }
}
