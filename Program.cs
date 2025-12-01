using APIUsuarios.Application.Interfaces;
using APIUsuarios.Application.Services;
using APIUsuarios.Application.Validators;
using APIUsuarios.Application.DTOs;
using APIUsuarios.Infrastructure.Persistence;
using APIUsuarios.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Validators
builder.Services.AddScoped<IValidator<UsuarioCreateDto>, UsuarioCreateDtoValidator>();
builder.Services.AddScoped<IValidator<UsuarioUpdateDto>, UsuarioUpdateDtoValidator>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Endpoints
app.MapGet("/usuarios", async (IUsuarioService service, CancellationToken ct) =>
{
    var usuarios = await service.ListarAsync(ct);
    return Results.Ok(usuarios);
})
.WithName("GetUsuarios")
.Produces<IEnumerable<UsuarioReadDto>>(200);

app.MapGet("/usuarios/{id}", async (int id, IUsuarioService service, CancellationToken ct) =>
{
    var usuario = await service.ObterAsync(id, ct);
    return usuario != null ? Results.Ok(usuario) : Results.NotFound();
})
.WithName("GetUsuarioById")
.Produces<UsuarioReadDto>(200)
.Produces(404);

app.MapPost("/usuarios", async (UsuarioCreateDto dto, IUsuarioService service, 
    IValidator<UsuarioCreateDto> validator, CancellationToken ct) =>
{
    var validationResult = await validator.ValidateAsync(dto, ct);
    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());

    try
    {
        var usuario = await service.CriarAsync(dto, ct);
        return Results.Created($"/usuarios/{usuario.Id}", usuario);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Email já cadastrado"))
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex) when (ex.Message.Contains("18 anos"))
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateUsuario")
.Produces<UsuarioReadDto>(201)
.Produces(400)
.Produces(409);

app.MapPut("/usuarios/{id}", async (int id, UsuarioUpdateDto dto, IUsuarioService service,
    IValidator<UsuarioUpdateDto> validator, CancellationToken ct) =>
{
    var validationResult = await validator.ValidateAsync(dto, ct);
    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());

    try
    {
        var usuario = await service.AtualizarAsync(id, dto, ct);
        return Results.Ok(usuario);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Email já cadastrado"))
    {
        return Results.Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex) when (ex.Message.Contains("18 anos"))
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateUsuario")
.Produces<UsuarioReadDto>(200)
.Produces(400)
.Produces(404)
.Produces(409);

app.MapDelete("/usuarios/{id}", async (int id, IUsuarioService service, CancellationToken ct) =>
{
    var removido = await service.RemoverAsync(id, ct);
    return removido ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteUsuario")
.Produces(204)
.Produces(404);

// Migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();