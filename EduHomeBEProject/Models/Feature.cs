﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduHomeBEProject.Models
{
    public class Feature
    {
        public int Id { get; set; }
        public string FeatureText { get; set; }
        public Teacher Teacher { get; set; }
        public int TeacherId { get; set; }
    }
}
