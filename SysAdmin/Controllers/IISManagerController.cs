using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Serilog;

namespace SysAdmin.Controllers
{
  [Route("api/iis")]
  [ApiController]
  [Authorize]
  public class IISManagerController : ControllerBase
  {
    private readonly IConfiguration _configuration;

    public IISManagerController(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    [HttpGet("pools")]
    public IActionResult GetPools()
    {
      try
      {
        var ignoredPools = _configuration.GetSection("IISSettings:IgnoredPools").Get<List<string>>();

        var userName = User.Identity?.Name ?? "Não identificado";
        Log.Information($"Usuário: {userName} está solicitando a lista de pools.");

        using var serverManager = new ServerManager();
        var pools = serverManager.ApplicationPools;
        var poolStatuses = new List<object>();

        foreach (var pool in pools)
        {
          if (ignoredPools is not null && ignoredPools.Contains(pool.Name, StringComparer.OrdinalIgnoreCase))
          {
            Log.Information($"Ignorando o pool: {pool.Name}");
            continue;
          }

          poolStatuses.Add(new
          {
            Name = pool.Name,
            Status = pool.State.ToString()
          });
        }

        if (poolStatuses.Count == 0)
        {
          Log.Warning("Nenhum pool encontrado no IIS.");
          return NotFound("Nenhum pool encontrado.");
        }

        Log.Information("Lista de pools retornada com sucesso.");
        return Ok(poolStatuses); // Retorna a lista com o status de cada pool
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Erro ao obter lista de pools do IIS.");
        return Problem("Erro ao obter lista de pools.");
      }
    }

    [HttpPost("start/{poolName}")]
    public IActionResult StartPool(string poolName)
    {
      try
      {
        var userName = User.Identity?.Name ?? "Não identificado";
        Log.Information($"Usuário: {userName} está iniciando o pool: {poolName}");

        using var serverManager = new ServerManager();
        var pool = serverManager.ApplicationPools[poolName];

        if (pool == null)
        {
          Log.Warning("Tentativa de iniciar um pool inexistente: {PoolName}", poolName);
          return BadRequest("Pool não encontrado.");
        }

        if (pool.State == ObjectState.Started)
        {
          Log.Information("Pool {PoolName} já está rodando.", poolName);
          return Ok($"O Pool {poolName} já está em execução.");
        }

        pool.Start();
        Log.Information("Pool {PoolName} iniciado com sucesso.", poolName);
        return Ok($"Pool {poolName} iniciado.");
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Erro ao iniciar o pool {PoolName}", poolName);
        return Problem("Erro ao iniciar o pool.");
      }
    }

    [HttpPost("stop/{poolName}")]
    public IActionResult StopPool(string poolName)
    {
      try
      {
        var userName = User.Identity?.Name ?? "Não identificado";
        Log.Information($"Usuário: {userName} está parando o pool: {poolName}");

        using var serverManager = new ServerManager();
        var pool = serverManager.ApplicationPools[poolName];

        if (pool == null)
        {
          Log.Warning("Tentativa de parar um pool inexistente: {PoolName}", poolName);
          return BadRequest("Pool não encontrado.");
        }

        if (pool.State == ObjectState.Stopped)
        {
          Log.Information("Pool {PoolName} já está parado.", poolName);
          return Ok($"O Pool {poolName} já está parado.");
        }

        pool.Stop();
        Log.Information("Pool {PoolName} parado com sucesso.", poolName);
        return Ok($"Pool {poolName} parado.");
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Erro ao parar o pool {PoolName}", poolName);
        return Problem("Erro ao parar o pool.");
      }
    }

   
    private string GetSysAdminPoolName(ServerManager serverManager)
    {
      // Iterar sobre todos os sites e suas aplicações para identificar o pool associado ao "SysAdmin"
      foreach (var site in serverManager.Sites)
      {
        foreach (var application in site.Applications)
        {
          // Verifique o caminho físico da aplicação para encontrar a aplicação SysAdmin
          if (application.Path.Contains("SysAdmin", StringComparison.OrdinalIgnoreCase))
          {
            // Retorna o nome do pool associado à aplicação "SysAdmin"
            return application.ApplicationPoolName;
          }
        }
      }

      // Caso o pool não seja encontrado
      Log.Warning("Pool para a aplicação SysAdmin não encontrado.");
      return string.Empty; // Ou algum nome padrão caso não encontre
    }
  }
}
