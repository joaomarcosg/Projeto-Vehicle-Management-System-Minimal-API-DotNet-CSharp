namespace MinimalApi.Domain.ModelViews;

public struct Home
{
    public string Message { get => "Welcome to Vehicles API - Minimal API"; }
    public string Documentation { get => "/swagger"; }
}