namespace TodoApp.Api.Models;

/// <summary>
/// View model used to present diagnostic information on the shared error page.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Identifier that ties the error response back to the originating request.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Indicates whether the request identifier should be rendered for the user.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
