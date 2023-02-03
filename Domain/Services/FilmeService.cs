using MovieFinder.Domain.Entities;
using Nebula.Graph;
using NebulaNet;

namespace MovieFinder.Domain.Services;

/// <summary>
/// Classe que fornece serviços relacionados a filmes da plataforma.
/// </summary>
public class FilmeService
{
    /// <summary>
    /// Nebula Graph client.
    /// </summary>
    NebulaConnection _graphClient = new NebulaConnection();

    /// <summary>
    /// Resposta enviada após a autenticação do client.
    /// </summary>
    AuthResponse _authResponse = new AuthResponse();

    /// <summary>
    /// Endereço IP utilizado para acesso a base de dados.
    /// </summary>
    private string _iP = null!;

    /// <summary>
    /// Porta para acesso a base de dados.
    /// </summary>
    private int _port;

    private string _connUser = null!;

    private string _connPwd = null!;

    private string _cmdStr = "use movie_finder;";

    public FilmeService(IConfiguration config)
    {
        _connUser = config.GetValue<string>("BusCon:Usuario");
        _connPwd = config.GetValue<string>("BusCon:Senha");
        _iP = config.GetValue<string>("BusCon:IP");
        _port = config.GetValue<int>("BusCon:Porta");
    }

    public async Task<IEnumerable<Filme>?> ListarFilmesAsync()
    {
        string cmdStr = _cmdStr + 
                        $@"match (v:filme) 
                            return 
                                id(v) as id, 
                                v.filme.titulo as titulo,
                                toString(v.filme.popularidade) as popularidade
                            limit 100;";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var filmes = await _graphClient
            .ExecuteAsync(_authResponse.Session_id, cmdStr)
            .ToListAsync<Filme>();

        await _graphClient.SignOutAsync(_authResponse.Session_id);
        return filmes;
    }

    public async Task CadastrarFilmes(IEnumerable<Filme> filmes)
    {
        string cmdStr = _cmdStr;
        for (var x = 0; x < filmes.Count(); x++)
        {
            var popularidade = filmes.ElementAt(x).Popularidade.ToString().Replace(",", ".");
            cmdStr += _cmdStr + 
                        $@"insert vertex filme
                                (titulo, popularidade) 
                            values 
                                ""{filmes.ElementAt(x).Id}"":(""{filmes.ElementAt(x).Titulo}"", {popularidade});";
        }

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var executionResponse = await _graphClient.ExecuteAsync(_authResponse.Session_id, cmdStr);

        await _graphClient.SignOutAsync(_authResponse.Session_id);
    }
}