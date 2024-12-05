using KursovaHomeGarden.Models;

namespace KursovaHomeGarden.Services
{
    public interface IPdfService
    {
        byte[] GenerateReportPdf(ReportData reportData);
    }
}
