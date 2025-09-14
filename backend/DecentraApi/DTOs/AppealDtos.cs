// DTOs/MakeAppealRequest.cs
namespace DecentraApi.DTOs;

public class MakeAppealRequest
{
    public List<int> PhotoIds { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

// DTOs/MakeAppealResponse.cs
public class MakeAppealResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AppealId { get; set; }
}

// DTOs/GetAppealsResponse.cs
public class GetAppealsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AppealSummary> Appeals { get; set; } = new();
}

// DTOs/AppealSummary.cs
public class AppealSummary
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public List<int> PhotoIds { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public bool Appealed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// DTOs/GetAppealResponse.cs
public class GetAppealResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AppealDetail? Appeal { get; set; }
}

// DTOs/AppealDetail.cs
public class AppealDetail
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Appealed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AppealPhotoDetail> Photos { get; set; } = new();
}

// DTOs/AppealPhotoDetail.cs
public class AppealPhotoDetail
{
    public int PhotoId { get; set; }
    public int Rust { get; set; }
    public int Dent { get; set; }
    public int Scratch { get; set; }
    public int Dust { get; set; }
    public DateTime LastUpdated { get; set; }
}

// DTOs/UpdateAppealStatusRequest.cs
public class UpdateAppealStatusRequest
{
    public bool Appealed { get; set; }
}

// DTOs/UpdateAppealStatusResponse.cs
public class UpdateAppealStatusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AppealId { get; set; }
    public bool NewStatus { get; set; }
}