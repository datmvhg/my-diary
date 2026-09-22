using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Entities
{
    // Simple emotion enum for diary entries
    public enum Emotion
    {
        Neutral = 0,
        Happy,
        Sad,
        Excited,
        Angry,
        Anxious,
        Grateful
    }

    public class DiaryMoment
    {
        public int Id { get; set; }

        // Owner user identifier
        public string? UserId { get; set; }

        // Optional title for the moment
        public string? Title { get; set; }

        // Description / notes about the moment
        public string? Description { get; set; }

        // Emotion associated with the moment
        public Emotion Emotion { get; set; } = Emotion.Neutral;

        // When the moment happened (can be different from CreatedAt)
        public DateTime MomentAt { get; set; } = DateTime.UtcNow;

        // Image binary data (stored in database as varbinary)
        public byte[]? ImageData { get; set; }

        // MIME type for the image (e.g., image/jpeg)
        public string? ImageMimeType { get; set; }

        // When this record was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Helper to populate ImageData and ImageMimeType from an uploaded file
        // This keeps the entity responsible for simple mapping; controllers/services
        // should call this when receiving an IFormFile upload.
        public async Task SetImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return;

            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ImageData = ms.ToArray();
            ImageMimeType = file.ContentType;
        }
    }
}
