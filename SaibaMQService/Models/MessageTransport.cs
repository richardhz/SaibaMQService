using System;
using System.Collections.Generic;
using System.Text;

namespace SaibaMQService.Models
{
    public class MessageTransport
    {
        public string Reference { get; set; }
        public DateTimeOffset MessageDate { get; set; }
        public string MessageType { get; set; }
        public byte[] Payload { get; set; }
    }
}
