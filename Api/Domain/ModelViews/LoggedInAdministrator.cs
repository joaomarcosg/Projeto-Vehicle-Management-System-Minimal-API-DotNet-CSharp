namespace MinimalApi.Domain.ModelViews;

public record LoggedInAdministrator
{
    public string Email { get; set; } = default!;
    public string Profile { get; set; } = default!;
    public string Token { get; set; } = default!;
}