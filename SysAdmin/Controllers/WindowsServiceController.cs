using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.ServiceProcess;

namespace SysAdmin.Controllers
{
    [Route("api/service")]
    [ApiController]
    [Authorize]
    public class WindowsServiceController : ControllerBase
    {
        [HttpPost("start/{serviceName}")]
        public IActionResult StartService(string serviceName)
        {
            try
            {
                using var service = new ServiceController(serviceName);

                if (service.Status == ServiceControllerStatus.Running)
                {
                    Log.Information("Serviço {ServiceName} já está em execução.", serviceName);
                    return Ok($"O serviço {serviceName} já está rodando.");
                }

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
                Log.Information("Serviço {ServiceName} iniciado com sucesso.", serviceName);
                return Ok($"Serviço {serviceName} iniciado.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao iniciar o serviço {ServiceName}", serviceName);
                return Problem("Erro ao iniciar o serviço.");
            }
        }

        [HttpPost("stop/{serviceName}")]
        public IActionResult StopService(string serviceName)
        {
            try
            {
                using var service = new ServiceController(serviceName);

                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    Log.Information("Serviço {ServiceName} já está parado.", serviceName);
                    return Ok($"O serviço {serviceName} já está parado.");
                }

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
                Log.Information("Serviço {ServiceName} parado com sucesso.", serviceName);
                return Ok($"Serviço {serviceName} parado.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao parar o serviço {ServiceName}", serviceName);
                return Problem("Erro ao parar o serviço.");
            }
        }
    }
}
