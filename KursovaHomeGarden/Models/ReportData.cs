namespace KursovaHomeGarden.Models
{
    public class ReportData
    {
        public List<PlantPopularityReport> PlantPopularity { get; set; }
        public List<CategoryStatisticsReport> CategoryStatistics { get; set; }
        public List<ActionFrequencyReport> ActionFrequencies { get; set; }
        public List<UserReport> Users { get; set; }
        public DateTime GeneratedAt { get; set; }

        public ReportData()
        {
            PlantPopularity = new List<PlantPopularityReport>();
            CategoryStatistics = new List<CategoryStatisticsReport>();
            ActionFrequencies = new List<ActionFrequencyReport>();
            Users = new List<UserReport>();
            GeneratedAt = DateTime.Now;
        }
    }

    public class PlantPopularityReport
    {
        public string PlantName { get; set; }
        public int Popularity { get; set; }
    }

    public class CategoryStatisticsReport
    {
        public string CategoryName { get; set; }
        public decimal AveragePrice { get; set; }
        public int UserCount { get; set; }
    }

    public class ActionFrequencyReport
    {
        public string ActionType { get; set; }
        public int FrequencyCount { get; set; }
    }

    public class UserReport
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public decimal? AmountOfMoney { get; set; }
    }
}
