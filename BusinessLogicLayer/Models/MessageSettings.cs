using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Models
{
    public class MessageSettings
    {
        public BREB BREB { get; set; }
    }

    public class MessageDetail
    {
        public string StatusCode { get; set; }
        public string Msg { get; set; }
    }
    public class BREB
    {
       public Success Success { get; set; }
       public Error Error { get; set; }
    }

    public class Success {
        public MessageDetail CollectionUpdateSuccess { get; set; }
    }
    public class Error {
        public MessageDetail NoCollectionFound { get; set; }
        public MessageDetail NoBrebCollectionFound { get; set; }
        public MessageDetail TransactionIdMissing { get; set; }
        public MessageDetail PaymentSubmitLogMissing { get; set; }
        public MessageDetail CollectionUpdateFailed { get; set; }
    }
}
