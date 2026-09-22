using System;
using Entities;

namespace DTOs
{
    public class DiaryMomentDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Emotion Emotion { get; set; }
        public DateTime MomentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasImage { get; set; }
    }

    public class DiaryMomentCreateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Emotion Emotion { get; set; } = Emotion.Neutral;
        public DateTime? MomentAt { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? Image { get; set; }
    }

    public class DiaryMomentUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Emotion? Emotion { get; set; }
        public DateTime? MomentAt { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? Image { get; set; }
        public bool? RemoveImage { get; set; }
    }
}
