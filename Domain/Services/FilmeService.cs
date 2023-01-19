using System.Text.Json;
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

    public async Task CadastrarFilme(string id, string titulo, decimal popularidade)
    {
        var cmd = _cmdStr + $@"insert vertex filme
                                    (titulo, popularidade) 
                                values 
                                    ""{id}"":(""{titulo}"", ""{popularidade}"")";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var executionResponse = await _graphClient.ExecuteAsync(_authResponse.Session_id, cmd);

        await _graphClient.SignOutAsync(_authResponse.Session_id);
    }
}