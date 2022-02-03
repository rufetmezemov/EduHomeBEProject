﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EduHomeBEProject.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength: 600)]
        public string Text { get; set; }
        public string Subject { get; set; }
        public DateTime WriteTime { get; set; }
        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
