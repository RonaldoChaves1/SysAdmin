using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.AccountManagement;

namespace SysAdmin.Controllers
{
  [Route("api/user")]
  [ApiController]
  [Authorize]  // Garante que o endpoint só pode ser acessado por usuários autenticados
  public class UserController : ControllerBase
  {
    [HttpGet("name")]
    public IActionResult GetUserName()
    {
      // Obtém o nome de usuário do contexto de autenticação
      var userName = User.Identity?.Name; // Exemplo: "intranet\\username"

      if (string.IsNullOrEmpty(userName))
      {
        return Unauthorized("Usuário não autenticado.");
      }

      try
      {
        // Remove o domínio, se necessário, e obtenha o nome do usuário
        var cleanUserName = userName.Split('\\')[1];

        // Usando o AccountManagement para acessar o AD e obter o nome completo
        using (var context = new PrincipalContext(ContextType.Domain))
        {
          var userPrincipal = UserPrincipal.FindByIdentity(context, cleanUserName);

          if (userPrincipal != null)
          {
            var fullName = string.Format("{0} ({1})", userPrincipal.DisplayName, userPrincipal.EmailAddress);  // Nome completo do usuário
            return Ok(new { UserName = fullName });
          }
          else
          {
            return NotFound("Usuário não encontrado no Active Directory.");
          }
        }
      }
      catch (Exception ex)
      {
        return BadRequest($"Erro ao recuperar nome completo: {ex.Message}");
      }
    }
  }
}
