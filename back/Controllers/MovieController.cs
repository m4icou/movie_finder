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

    [HttpGet("listar-atores")]
    public async Task<IEnumerable<Ator>?> ListarAtores()
    {
        return await _atorService.ListarAsync();
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

    [HttpGet("recuperar-distancia/{idAtorOrigem}/{idAtorDestino}")]
    public async Task<string> RecuperarDistancia([FromRoute] string idAtorOrigem, [FromRoute] string idAtorDestino)
    {
        var atores = await _atorService.RecuperarDistancia(idAtorOrigem, idAtorDestino);

        string caminho = null!;
        if (atores != null && atores.Any())
        {
            caminho = atores!.First().Nome;
            for (var x = 1; x < atores!.Count(); x++)
                caminho += " -> " + atores!.ElementAt(x).Nome;
        }

        return caminho;
    }

    [HttpGet("listar-seguidores/{idAtor}/{passos}")]
    public async Task<IEnumerable<Ator>?> ListarSeguidores([FromRoute] string idAtor, [FromRoute] int passos)
    {
        return await _atorService.ListarSeguidores(idAtor, passos);
    }

    [HttpPost("carregar-dados")]
    public async Task CarregarDados(IEnumerable<string> titulosFilmes)
    {
        var filmes = new List<Filme>();
        var atores = new List<Ator>();
        var atuacoes = new Dictionary<string, IEnumerable<string>>();
        foreach (var titulo in titulosFilmes)
        {
            // Recupera o filme.
            var filmeResponse = await RecuperarFilme(titulo);
            var filme = (Filme)filmeResponse;
            filmes.Add(filme);

            // Recupera os atores do filme e salva na lista de atuações.
            var atoresResponse = await RecuperarAtoresFilme(filmeResponse.id);
            var atoresFilme = atoresResponse.Select(a => (Ator)a);
            atuacoes.Add(filme.Id, atoresFilme.Select(a => a.Id));

            // Relaciona atores seguidos por cada ator.
            await _atorService.SalvarAsync(atoresFilme);
            /*foreach (var ator in atoresFilme)
            {
                var idsAtores = atoresFilme.Select(a => a.Id).Where(i => i != ator.Id);
                await _atorService.SeguirAsync(ator.Id, idsAtores, DateTime.Now);
            }*/
        }

        // Cadastra o filme e salva as atuações.
        await _filmeService.CadastrarFilmes(filmes);
        foreach (var item in atuacoes)
            await _atorService.SalvarAtuacoesFilmeAsync(item.Key, item.Value);
    }

    [HttpPut("limpar-filmes")]
    public async Task LimparFilmes()
    {
        await _filmeService.LimparAsync();
    }

    [HttpPut("limpar-atores")]
    public async Task LimparAtores()
    {
        await _atorService.LimparAsync();
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
