namespace MovieFinder.Domain.Entities;

/// <summary>
/// Entidade que representa um ator.
/// </summary>
public class Ator
{
    /// <summary>
    /// Código de identificação do ator.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Nome do ator.
    /// </summary>
    public string Nome { get; set; } = null!;

    /// <summary>
    /// Construtor padrão com parâmetros.
    /// </summary>
    /// <param name="id">Código de identificação do usuário.</param>
    /// <param name="nome">Nome do usuário.</param>
    public Ator(string id, string nome)
    {
        Id = id;
        Nome = nome;
    }

    /// <summary>
    /// Construtor padrão sem parâmetros.
    /// </summary>
    public Ator()
    {
    }
}