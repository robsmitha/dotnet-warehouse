﻿using Core.Models.Dimensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    public class DimDate : CalendarDateDimension
    {
        public override int Key { get; set; }
        public string SourceKey { get; set; }
    }
}
