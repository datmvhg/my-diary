using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DBConnect;
using Entities;
using DTOs;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DiaryMomentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DiaryMomentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: api/DiaryMoments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = CurrentUserId;

            var list = await _db.DiaryMoments
                .AsNoTracking()
                .Where(d => d.UserId == userId || d.UserId == null)
                .Select(d => new DiaryMomentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    Emotion = d.Emotion,
                    MomentAt = d.MomentAt,
                    CreatedAt = d.CreatedAt,
                    HasImage = d.ImageData != null && d.ImageData.Length > 0
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/DiaryMoments/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = CurrentUserId;
            var d = await _db.DiaryMoments.FirstOrDefaultAsync(x => x.Id == id && (x.UserId == userId || x.UserId == null));
            if (d == null) return NotFound();

            var dto = new DiaryMomentDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                Emotion = d.Emotion,
                MomentAt = d.MomentAt,
                CreatedAt = d.CreatedAt,
                HasImage = d.ImageData != null && d.ImageData.Length > 0
            };

            return Ok(dto);
        }

        // GET: api/DiaryMoments/{id}/image
        // Allow anonymous so <img> tags in HTML can load images directly
        [HttpGet("{id:int}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImage(int id)
        {
            var d = await _db.DiaryMoments
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.ImageData, x.ImageMimeType })
                .FirstOrDefaultAsync();

            if (d == null || d.ImageData == null || d.ImageData.Length == 0)
                return NotFound();

            return File(d.ImageData, d.ImageMimeType ?? "application/octet-stream");
        }

        // POST: api/DiaryMoments
        // Accepts multipart/form-data. Use fields: Title, Description, Emotion (name or number), MomentAt, Image (file)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] DiaryMomentCreateDto dto)
        {
            var userId = CurrentUserId;

            var moment = new DiaryMoment
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Emotion = dto.Emotion,
                MomentAt = dto.MomentAt ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Image != null)
            {
                await moment.SetImageAsync(dto.Image);
            }

            _db.DiaryMoments.Add(moment);
            await _db.SaveChangesAsync();

            var resultDto = new DiaryMomentDto
            {
                Id = moment.Id,
                Title = moment.Title,
                Description = moment.Description,
                Emotion = moment.Emotion,
                MomentAt = moment.MomentAt,
                CreatedAt = moment.CreatedAt,
                HasImage = moment.ImageData != null && moment.ImageData.Length > 0
            };

            return CreatedAtAction(nameof(GetById), new { id = moment.Id }, resultDto);
        }

        // PUT: api/DiaryMoments/{id}
        // Accepts multipart/form-data. Fields are optional; Image replaces existing when provided.
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] DiaryMomentUpdateDto dto)
        {
            var userId = CurrentUserId;
            var moment = await _db.DiaryMoments.FirstOrDefaultAsync(x => x.Id == id && (x.UserId == userId || x.UserId == null));
            if (moment == null) return NotFound();

            if (moment.UserId == null && userId != null)
            {
                moment.UserId = userId;
            }

            if (dto.Title != null) moment.Title = dto.Title;
            if (dto.Description != null) moment.Description = dto.Description;
            if (dto.Emotion.HasValue) moment.Emotion = dto.Emotion.Value;
            if (dto.MomentAt.HasValue) moment.MomentAt = dto.MomentAt.Value;

            if (dto.Image != null)
            {
                await moment.SetImageAsync(dto.Image);
            }
            else if (dto.RemoveImage == true)
            {
                moment.ImageData = null;
                moment.ImageMimeType = null;
            }

            _db.DiaryMoments.Update(moment);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/DiaryMoments/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = CurrentUserId;
            var moment = await _db.DiaryMoments.FirstOrDefaultAsync(x => x.Id == id && (x.UserId == userId || x.UserId == null));
            if (moment == null) return NotFound();

            _db.DiaryMoments.Remove(moment);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
