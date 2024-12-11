using Microsoft.AspNetCore.Mvc.Rendering;
using KursovaHomeGarden.Models;

namespace KursovaHomeGarden.Services
{
    public interface IActionFrequencyService
    {
        Task<List<ActionFrequency>> GetActionFrequenciesAsync(string searchTerm = null, int? plantId = null,
            int? seasonId = null, int? actionTypeId = null, int? fertTypeId = null);
        Task<(List<SelectListItem> Plants, List<SelectListItem> Seasons,
            List<SelectListItem> ActionTypes, List<SelectListItem> Fertilizers)> LoadFilterOptionsAsync();
    }
}