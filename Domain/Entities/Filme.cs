namespace MovieFinder.Domain.Entities;

/// <summary>
/// Entidade que representa um filme.
/// </summary>
public class Filme
{
    /// <summary>
    /// Código de identificação do filme.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Título do filme.
    /// </summary>
    public string Titulo { get; set; } = null!;

    /// <summary>
    /// Popularidade percentual do filme.
    /// </summary>
    public decimal Popularidade { get; set; }

    /// <summary>
    /// Construtor padrão com parâmetros.
    /// </summary>
    /// <param name="id">Código de identificação do filme.</param>
    /// <param name="titulo">Título do filme.</param>
    /// <param name="popularidade">Popularidade percentual do filme.</param>
    public Filme(string id, string titulo, decimal popularidade)
    {
        Id = id;
        Titulo = titulo;
        Popularidade = popularidade;
    }

    /// <summary>
    /// Construtor padrão sem parâmetros.
    /// </summary>
    public Filme()
    {
    }
}