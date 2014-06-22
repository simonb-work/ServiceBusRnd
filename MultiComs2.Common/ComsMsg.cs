﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.Common
{
    public class ComsMsg
    {
        public int ReqSeq { get; set; }
        public int ReqProcCount { get; set; }
        public Guid RequestId { get; set; }
        public DateTime ReqTimestampUtc { get; set; }
        public Guid OrigReqId { get; set; }
        public DateTime OrigReqTimestampUtc { get; set; }    
    }
}
