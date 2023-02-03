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
        var atuacoes = new Dictionary<string, IEnumerable<string>>();
        foreach (var titulo in titulosFilmes)
        {
            var filmeResponse = await RecuperarFilme(titulo);
            var filme = (Filme)filmeResponse;
            filmes.Add(filme);

            var atoresResponse = await RecuperarAtoresFilme(filmeResponse.id);
            var atoresFilme = atoresResponse.Select(a => (Ator)a);
            foreach(var response in atoresResponse)
                if (!atores.Any(a => a.Id == "a" + response.id))
                    atores.Add(response);

            atuacoes.Add(filme.Id, atoresFilme.Select(a => a.Id));
        }

        await _filmeService.CadastrarFilmes(filmes);
        await _atorService.SalvarAtoresAsync(atores);

        foreach (var item in atuacoes)
            await _atorService.SalvarAtuacoesFilmeAsync(item.Key, item.Value);
    }

    [HttpGet("listar-atores")]
    public async Task<IEnumerable<Ator>?> ListarAtores()
    {
        return await _atorService.ListarAtoresAsync();
    }

    [HttpGet("listar-filmes")]
    public async Task<IEnumerable<Filme>?> ListarFilmes()
    {
        return await _filmeService.ListarFilmesAsync();
    }

    [HttpGet("listar-coatuacoes/{idAtor}")]
    public async Task<IEnumerable<Ator>?> ListarCoatuacoes([FromRoute] string idAtor)
    {
        return await _atorService.ListarAtoresCoatuacaoAsync(idAtor);
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
