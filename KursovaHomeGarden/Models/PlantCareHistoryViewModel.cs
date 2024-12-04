namespace KursovaHomeGarden.Models
{
    public class PlantCareHistoryViewModel
    {
        public int CareId { get; set; }
        public DateTime ActionDate { get; set; }
        public DateTime NextCareDate { get; set; }
        public string ActionType { get; set; }
    }
}
