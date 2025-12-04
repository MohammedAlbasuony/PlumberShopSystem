using B_B.BLL.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.Service.Abstraction
{
    public interface IReportService
    {
        Task<ReportSummaryVM> GetOutReceiptsReportAsync(int year = 0);
        Task<ReportSummaryVM> GetInReceiptsReportAsync(int year = 0);
    }
}
