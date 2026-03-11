using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogicLayer.Models.DB;

namespace BusinessLogicLayer.Manager
{
    public interface IDBLogManager
    {
        Task<bool> SaveErrorLog(SaveErrorLog saveErrorLog);
        Task<bool> SaveReqRespLog(MwApiReqRespLog reqRespLog);
    }
}
