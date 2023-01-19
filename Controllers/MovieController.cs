using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MovieFinder.Controllers.Messages;
using MovieFinder.Domain.Services;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("movie")]
public class MovieController : ControllerBase
{
    /// <summary>
    /// Serviço que provê operações relacionadas aos atores da plataforma.
    /// </summary>
    private readonly AtorService _atorService;

    /// <summary>
    /// Serviço que provê operações relacionadas aos filmes da plataforma.
    /// </summary>
    private readonly FilmeService _filmeService;

    private string _urlBase = null!;

    private string _apiKey = null!;

    /// <summary>
    /// Construtor padrão com parâmetros.
    /// </summary>
    /// <param name="atorService">Serviço que provê operações relacionadas aos atores da plataforma.</param>
    /// <param name="filmeService">Serviço que provê operações relacionadas aos filmes da plataforma.</param>
    public MovieController(AtorService atorService, FilmeService filmeService)
    {
        _atorService = atorService;
        _filmeService = filmeService;
    }

    [HttpPost("carregar-dados")]
    public async Task CarregarDados(IEnumerable<string> titulosFilmes)
    {
        await RecuperarFilme(titulosFilmes.First());
    }

    private async Task RecuperarFilme(string titulo)
    {
        _urlBase = "https://api.themoviedb.org/3/";
        _apiKey = "e5842c7912765a272ea1feaece9eabf0";

        var client = new HttpClient();
        var result = await client.GetAsync(_urlBase + $"search/movie?api_key={_apiKey}&language=en-US&page=1&include_adult=false&query={titulo}");

        var resJson = await result.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<SearchMovieResponseMessage>(resJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
