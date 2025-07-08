using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;
using System.Diagnostics;

namespace FastTechFoodsAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class InstanceController : ControllerBase
{
    private readonly ILogger<InstanceController> _logger;

    public InstanceController(ILogger<InstanceController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retorna informações sobre a instância atual do pod/container
    /// </summary>
    /// <returns>Informações da instância</returns>
    [HttpGet("info")]
    public ActionResult<object> GetInstanceInfo()
    {
        try
        {
            var instanceInfo = new
            {
                // Informações do Pod/Container
                PodName = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName,
                PodNamespace = Environment.GetEnvironmentVariable("POD_NAMESPACE") ?? "default",
                PodIp = Environment.GetEnvironmentVariable("POD_IP") ?? GetLocalIPAddress(),
                NodeName = Environment.GetEnvironmentVariable("NODE_NAME") ?? "unknown",
                
                // Informações da Aplicação
                ApplicationName = "FastTechFoodsAuth.Api",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                
                // Informações do Sistema
                Platform = Environment.OSVersion.Platform.ToString(),
                Architecture = Environment.ProcessorCount,
                WorkingSet = GC.GetTotalMemory(false),
                StartTime = Process.GetCurrentProcess().StartTime,
                Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime,
                
                // Informações do Request
                RequestId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                
                // Informações de Kubernetes (se disponível)
                ServiceAccount = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_ACCOUNT") ?? "default",
                ClusterName = Environment.GetEnvironmentVariable("CLUSTER_NAME") ?? "unknown"
            };

            _logger.LogInformation("Informações da instância solicitadas: Pod {PodName}, Namespace {Namespace}", 
                instanceInfo.PodName, instanceInfo.PodNamespace);

            return Ok(instanceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações da instância");
            return StatusCode(500, new { Error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Retorna apenas o nome do pod atual
    /// </summary>
    /// <returns>Nome do pod</returns>
    [HttpGet("pod-name")]
    public ActionResult<string> GetPodName()
    {
        var podName = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName;
        return Ok(new { PodName = podName });
    }

    /// <summary>
    /// Endpoint para verificar a saúde da instância com informações básicas
    /// </summary>
    /// <returns>Status da instância</returns>
    [HttpGet("health")]
    public ActionResult<object> GetInstanceHealth()
    {
        var health = new
        {
            Status = "Healthy",
            PodName = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName,
            Timestamp = DateTime.UtcNow,
            Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime
        };

        return Ok(health);
    }

    private static string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
