using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RendevumVar.Application.DTOs;
using RendevumVar.Core.Entities;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContentController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContentPageDto>>> GetContentPages()
    {
        var pages = await _context.ContentPages
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new ContentPageDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                MetaDescription = p.MetaDescription,
                MetaKeywords = p.MetaKeywords,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                ImageUrl = p.ImageUrl,
                ButtonText = p.ButtonText,
                ButtonUrl = p.ButtonUrl,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Ok(pages);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ContentPageDto>> GetContentPage(string slug)
    {
        var page = await _context.ContentPages
            .Where(p => p.Slug == slug && p.IsActive)
            .Select(p => new ContentPageDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                MetaDescription = p.MetaDescription,
                MetaKeywords = p.MetaKeywords,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                ImageUrl = p.ImageUrl,
                ButtonText = p.ButtonText,
                ButtonUrl = p.ButtonUrl,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (page == null)
            return NotFound();

        return Ok(page);
    }

    [HttpGet("by-id/{id:guid}")]
    public async Task<ActionResult<ContentPageDto>> GetContentPageById(Guid id)
    {
        var page = await _context.ContentPages
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => new ContentPageDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                MetaDescription = p.MetaDescription,
                MetaKeywords = p.MetaKeywords,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                ImageUrl = p.ImageUrl,
                ButtonText = p.ButtonText,
                ButtonUrl = p.ButtonUrl,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (page == null)
            return NotFound();

        return Ok(page);
    }

    [HttpPost]
    public async Task<ActionResult<ContentPageDto>> CreateContentPage(CreateContentPageDto createDto)
    {
        var page = new ContentPage
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            Slug = createDto.Slug,
            Content = createDto.Content,
            MetaDescription = createDto.MetaDescription,
            MetaKeywords = createDto.MetaKeywords,
            IsActive = createDto.IsActive,
            SortOrder = createDto.SortOrder,
            ImageUrl = createDto.ImageUrl,
            ButtonText = createDto.ButtonText,
            ButtonUrl = createDto.ButtonUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ContentPages.Add(page);
        await _context.SaveChangesAsync();

        var result = new ContentPageDto
        {
            Id = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            Content = page.Content,
            MetaDescription = page.MetaDescription,
            MetaKeywords = page.MetaKeywords,
            IsActive = page.IsActive,
            SortOrder = page.SortOrder,
            ImageUrl = page.ImageUrl,
            ButtonText = page.ButtonText,
            ButtonUrl = page.ButtonUrl,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };

        return CreatedAtAction(nameof(GetContentPageById), new { id = page.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateContentPage(Guid id, CreateContentPageDto updateDto)
    {
        var page = await _context.ContentPages.FindAsync(id);
        if (page == null)
            return NotFound();

        page.Title = updateDto.Title;
        page.Slug = updateDto.Slug;
        page.Content = updateDto.Content;
        page.MetaDescription = updateDto.MetaDescription;
        page.MetaKeywords = updateDto.MetaKeywords;
        page.IsActive = updateDto.IsActive;
        page.SortOrder = updateDto.SortOrder;
        page.ImageUrl = updateDto.ImageUrl;
        page.ButtonText = updateDto.ButtonText;
        page.ButtonUrl = updateDto.ButtonUrl;
        page.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ContentPageExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteContentPage(Guid id)
    {
        var page = await _context.ContentPages.FindAsync(id);
        if (page == null)
            return NotFound();

        _context.ContentPages.Remove(page);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ContentPageExists(Guid id)
    {
        return _context.ContentPages.Any(e => e.Id == id);
    }
}