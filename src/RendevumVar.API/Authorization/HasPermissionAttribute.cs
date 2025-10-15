using Microsoft.AspNetCore.Authorization;

namespace RendevumVar.API.Authorization;

/// <summary>
/// Custom authorization attribute for permission-based access control
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(permission)
    {
    }
}
