using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Application.Events
{
    public record GenerateReportEvent(string AdminUserId, DateTime StartDate, DateTime EndDate);
}
