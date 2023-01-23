using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MovieFinder.Controllers.Messages;
using MovieFinder.Domain.Entities;
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

    private HttpClient _client = null!;

    /// <summary>
    /// Construtor padrão com parâmetros.
    /// </summary>
    /// <param name="atorService">Serviço que provê operações relacionadas aos atores da plataforma.</param>
    /// <param name="filmeService">Serviço que provê operações relacionadas aos filmes da plataforma.</param>
    public MovieController(AtorService atorService, FilmeService filmeService)
    {
        _atorService = atorService;
        _filmeService = filmeService;
        _urlBase = "https://api.themoviedb.org/3/";
        _apiKey = "e5842c7912765a272ea1feaece9eabf0";
        _client = new HttpClient();
    }

    [HttpPost("carregar-dados")]
    public async Task CarregarDados(IEnumerable<string> titulosFilmes)
    {
        var filmes = new List<Filme>();
        var atores = new List<Ator>();
        var atuacoes = new Dictionary<int, IEnumerable<int>>();
        foreach (var titulo in titulosFilmes)
        {
            var filme = await RecuperarFilme(titulo);
            filmes.Add(new Filme(filme.id, filme.title, filme.popularity));

            var atoresFilme = await RecuperarAtoresFilme(filme.id);
            foreach(var atorFilme in atoresFilme)
                if (!atores.Any(a => a.Id == atorFilme.id))
                    atores.Add(new Ator(atorFilme.id, atorFilme.name));

            atuacoes.Add(filme.id, atores.Select(a => a.Id));
        }

        await _filmeService.CadastrarFilmes(filmes);
        await _atorService.SalvarAtoresAsync(atores);

        foreach (var item in atuacoes)
            await _atorService.SalvarAtuacoesFilmeAsync(item.Key, item.Value);
    }

    private async Task<MovieResponseMessage> RecuperarFilme(string titulo)
    {
        var result = await _client.GetAsync(_urlBase + $"search/movie?api_key={_apiKey}&language=en-US&page=1&include_adult=false&query={titulo}");

        var resJson = await result.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<SearchMovieResponseMessage>(resJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return searchResult.results.First();
    }

    private async Task<IEnumerable<ActorResponseMessage>> RecuperarAtoresFilme(int idFilme)
    {
        var result = await _client.GetAsync(_urlBase + $"movie/{idFilme}/credits?api_key={_apiKey}");

        var resJson = await result.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<GetMovieCreditsResponseMessage>(resJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return searchResult.cast.Take(5);
    }
}
