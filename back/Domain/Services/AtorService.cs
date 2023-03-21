using MovieFinder.Domain.Entities;
using Nebula.Graph;
using NebulaNet;

namespace MovieFinder.Domain.Services;

/// <summary>
/// Classe que fornece serviços relacionados ao usuário.
/// </summary>
public class AtorService
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

    public AtorService(IConfiguration config)
    {
        _connUser = config.GetValue<string>("BusCon:Usuario");
        _connPwd = config.GetValue<string>("BusCon:Senha");
        _iP = config.GetValue<string>("BusCon:IP");
        _port = config.GetValue<int>("BusCon:Porta");
    }

    public async Task<IEnumerable<Ator>?> ListarAsync()
    {
        string cmdStr = _cmdStr +
                        $@"match (v:ator) 
                            return 
                                id(v) as id, 
                                v.ator.nome as nome
                            limit 100;";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var atores = await _graphClient
            .ExecuteAsync(_authResponse.Session_id, cmdStr)
            .ToListAsync<Ator>();

        await _graphClient.SignOutAsync(_authResponse.Session_id);
        return atores;
    }

    public async Task<IEnumerable<Ator>?> ListarAtoresCoatuacaoAsync(string idAtor)
    {
        string cmdStr = _cmdStr +
                        $@"MATCH (v1:ator)-->(v)<--(v2)
                            WHERE id(v2) == ""{idAtor}""
                            RETURN 
                                id(v1) as id, 
                                v1.ator.nome as nome;";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var atores = await _graphClient
            .ExecuteAsync(_authResponse.Session_id, cmdStr)
            .ToListAsync<Ator>();

        await _graphClient.SignOutAsync(_authResponse.Session_id);
        return atores;
    }

    public async Task<IEnumerable<Ator>?> RecuperarDistancia(string idAtorOrigem, string idAtorDestino)
    {
        string cmdStr = _cmdStr +
                            $@"find shortest path with prop 
                                from ""{idAtorOrigem}"" to ""{idAtorDestino}"" 
                                over segue 
                                yield path as p |
                                yield nodes($-.p) as n |
                                unwind $-.n as l |
                                YIELD 
                                    id($-.l) as id,
                                    $-.l.nome as nome;";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var atores = await _graphClient
            .ExecuteAsync(_authResponse.Session_id, cmdStr)
            .ToListAsync<Ator>();

        await _graphClient.SignOutAsync(_authResponse.Session_id);
        return atores;
    }

    public async Task<IEnumerable<Ator>?> ListarSeguidores(string idAtor, int passos = 1)
    {
        string cmdStr = _cmdStr +
                            $@"GO {passos} STEPS 
                                FROM ""{idAtor}"" 
                                OVER segue 
                                YIELD 
                                    id($$) as id,
                                    properties($$).nome as nome;";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var atores = await _graphClient
            .ExecuteAsync(_authResponse.Session_id, cmdStr)
            .ToListAsync<Ator>();

        await _graphClient.SignOutAsync(_authResponse.Session_id);
        return atores;
    }

    public async Task SalvarAsync(IEnumerable<Ator> atores)
    {
        string cmdStr = _cmdStr;
        for (var x = 0; x < atores.Count(); x++)
            cmdStr += $@"insert vertex if not exists ator
                            (nome) 
                        values 
                            ""{atores.ElementAt(x).Id}"":(""{atores.ElementAt(x).Nome}"");";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var executionResponse = await _graphClient.ExecuteAsync(_authResponse.Session_id, cmdStr);

        await _graphClient.SignOutAsync(_authResponse.Session_id);
    }

    public async Task SalvarAtuacoesFilmeAsync(string idFilme, IEnumerable<string> idsAtores)
    {
        string cmdStr = _cmdStr;
        var data = DateTime.Now.ToString("s");
        for (var x = 0; x < idsAtores.Count(); x++)
            cmdStr += $@"insert edge atua
                            (data_atuacao) 
                        values
                            ""{idsAtores.ElementAt(x)}"" -> ""{idFilme}"":(datetime(""{data}""));";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var executionResponse = await _graphClient.ExecuteAsync(_authResponse.Session_id, cmdStr);

        await _graphClient.SignOutAsync(_authResponse.Session_id);
    }

    public async Task LimparAsync()
    {
        string cmdStr = _cmdStr +
                            $@"LOOKUP ON ator 
                                YIELD id(vertex) as id |
                                delete vertex $-.id with edge;";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var executionResponse = await _graphClient.ExecuteAsync(_authResponse.Session_id, cmdStr);

        await _graphClient.SignOutAsync(_authResponse.Session_id);
    }

    public async Task SeguirAsync(string idAtor, IEnumerable<string> idsAtores, DateTime dataFilme)
    {
        string cmdStr = _cmdStr;
        var data = dataFilme.ToString("s");
        for (var x = 0; x < idsAtores.Count(); x++)
            cmdStr += $@"insert edge segue
                            (data_filme) 
                        values
                            ""{idAtor}"" -> ""{idsAtores.ElementAt(x)}"":(datetime(""{data}""));";

        await _graphClient.OpenAsync(_iP, _port);
        _authResponse = await _graphClient.AuthenticateAsync(_connUser, _connPwd);
        var executionResponse = await _graphClient.ExecuteAsync(_authResponse.Session_id, cmdStr);

        await _graphClient.SignOutAsync(_authResponse.Session_id);
    }
}