﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Models
{
    public class Sport
    {
        public int ProviderId { get; set; }
        public int SourceId { get; set; }
        public int SportId { get; set; }
        public List<Names> Names { get; set; }
    }
}
