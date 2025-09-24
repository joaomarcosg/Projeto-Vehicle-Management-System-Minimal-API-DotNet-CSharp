var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Password == "123456")
        return Results.Ok("Logged in sucessfully");
    else
        return Results.Unauthorized();
});


app.Run();

public class LoginDTO
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}