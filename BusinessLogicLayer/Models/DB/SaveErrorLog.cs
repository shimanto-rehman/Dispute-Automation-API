using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models.DB
{
    public class SaveErrorLog
    {
        public string? ControllerName { get; set; }  
        public string? MethodName { get; set; }  
        public string? ErrorMsg { get; set; }  
        public string? StackTrace { get; set; }  
        public string? ServiceId { get; set; } 
        public string? CreatedDate { get; set; }
    }  
}
