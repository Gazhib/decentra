using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace DecentraApi.DTOs;

public class PhotoUploadRequest
{
    [Required]
    public IFormFile LeftSide { get; set; } = null!;
        
    [Required]
    public IFormFile RightSide { get; set; } = null!;
        
    [Required]
    public IFormFile Front { get; set; } = null!;
        
    [Required]
    public IFormFile Back { get; set; } = null!;
}

public class PhotoUploadResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int[]? PhotoIds { get; set; }
}

public class PhotoResponse
{
    public int Id { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Rust { get; set; } = string.Empty;
    public string Dent { get; set; } = string.Empty;
    public string Scratch { get; set; } = string.Empty;
    public string Dust { get; set; } = string.Empty;
    
    public string Image { get; set; } = string.Empty;
}

public class GetPhotosResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<PhotoResponse> Photos { get; set; } = new();
}

public class PythonAnalysisResponse
{
    public string Rust { get; set; } = string.Empty;
    public string Dent { get; set; } = string.Empty;
    public string Scratch { get; set; } = string.Empty;
    public string Dust { get; set; } = string.Empty;
}