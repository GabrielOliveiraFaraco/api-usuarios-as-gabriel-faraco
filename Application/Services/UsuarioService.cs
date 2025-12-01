using APIUsuarios.Application.DTOs;
using APIUsuarios.Application.Interfaces;
using APIUsuarios.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace APIUsuarios.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(IUsuarioRepository repository, ILogger<UsuarioService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<UsuarioReadDto>> ListarAsync(CancellationToken ct)
    {
        var usuarios = await _repository.GetAllAsync(ct);
        return usuarios.Select(u => MapToReadDto(u));
    }

    public async Task<UsuarioReadDto?> ObterAsync(int id, CancellationToken ct)
    {
        var usuario = await _repository.GetByIdAsync(id, ct);
        return usuario != null ? MapToReadDto(usuario) : null;
    }

    public async Task<UsuarioReadDto> CriarAsync(UsuarioCreateDto dto, CancellationToken ct)
    {
        if (DateTime.Now.Year - dto.DataNascimento.Year < 18)
            throw new ArgumentException("Usuário deve ter pelo menos 18 anos");

        var emailNormalizado = dto.Email.ToLowerInvariant();

        if (await _repository.EmailExistsAsync(emailNormalizado, ct))
            throw new InvalidOperationException("Email já cadastrado");

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = emailNormalizado,
            Senha = dto.Senha,
            DataNascimento = dto.DataNascimento,
            Telefone = dto.Telefone,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        await _repository.AddAsync(usuario, ct);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Usuário criado com ID: {Id}", usuario.Id);
        return MapToReadDto(usuario);
    }

    public async Task<UsuarioReadDto> AtualizarAsync(int id, UsuarioUpdateDto dto, CancellationToken ct)
    {
        var usuario = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado");

        if (DateTime.Now.Year - dto.DataNascimento.Year < 18)
            throw new ArgumentException("Usuário deve ter pelo menos 18 anos");

        var emailNormalizado = dto.Email.ToLowerInvariant();
        var usuarioComEmail = await _repository.GetByEmailAsync(emailNormalizado, ct);
        if (usuarioComEmail != null && usuarioComEmail.Id != id)
            throw new InvalidOperationException("Email já cadastrado por outro usuário");

        usuario.Nome = dto.Nome;
        usuario.Email = emailNormalizado;
        usuario.DataNascimento = dto.DataNascimento;
        usuario.Telefone = dto.Telefone;
        usuario.Ativo = dto.Ativo;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _repository.UpdateAsync(usuario, ct);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Usuário atualizado com ID: {Id}", usuario.Id);
        return MapToReadDto(usuario);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken ct)
    {
        var usuario = await _repository.GetByIdAsync(id, ct);
        if (usuario == null) return false;

        usuario.Ativo = false;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _repository.UpdateAsync(usuario, ct);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Usuário desativado com ID: {Id}", usuario.Id);
        return true;
    }

    public async Task<bool> EmailJaCadastradoAsync(string email, CancellationToken ct)
    {
        return await _repository.EmailExistsAsync(email.ToLowerInvariant(), ct);
    }

    private static UsuarioReadDto MapToReadDto(Usuario usuario)
    {
        return new UsuarioReadDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.DataNascimento,
            usuario.Telefone,
            usuario.Ativo,
            usuario.DataCriacao
        );
    }
}