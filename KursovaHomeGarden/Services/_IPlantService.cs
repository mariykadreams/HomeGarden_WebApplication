using KursovaHomeGarden.Models.Plant;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models.CareLevel;

namespace KursovaHomeGarden.Services
{
    public interface IPlantService
    {
        Task<(List<Plant> Plants, List<Category> Categories, List<CareLevel> CareLevels)> GetFilteredPlantsAsync(
            string searchTerm,
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId,
            int? careLevelId,
            string sortBy
        );
        Task<bool> AddPlantToUserCollectionAsync(int plantId, string userId);
    }
}