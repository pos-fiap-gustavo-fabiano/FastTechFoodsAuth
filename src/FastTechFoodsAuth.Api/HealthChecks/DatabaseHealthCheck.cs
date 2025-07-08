using Microsoft.Extensions.Diagnostics.HealthChecks;
using FastTechFoodsAuth.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace FastTechFoodsAuth.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se consegue conectar no banco
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                _logger.LogError("Não é possível conectar ao banco de dados");
                return HealthCheckResult.Unhealthy("Não é possível conectar ao banco de dados");
            }

            // Verificar se as tabelas principais existem
            var tables = new[] { "Users", "Roles", "UserRoles" };
            var existingTables = new List<string>();

            foreach (var table in tables)
            {
                try
                {
                    var query = $"SELECT 1 FROM \"{table}\" LIMIT 1";
                    await _context.Database.ExecuteSqlRawAsync(query, cancellationToken);
                    existingTables.Add(table);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Tabela {Table} não encontrada ou inacessível: {Error}", table, ex.Message);
                }
            }

            if (existingTables.Count != tables.Length)
            {
                var missingTables = tables.Except(existingTables);
                return HealthCheckResult.Degraded($"Algumas tabelas estão faltando: {string.Join(", ", missingTables)}");
            }

            // Verificar se há dados básicos (pelo menos uma role)
            var rolesCount = await _context.Roles.CountAsync(cancellationToken);
            
            var healthData = new Dictionary<string, object>
            {
                { "canConnect", canConnect },
                { "tablesFound", existingTables.Count },
                { "rolesCount", rolesCount },
                { "connectionString", _context.Database.GetConnectionString()?.Substring(0, Math.Min(50, _context.Database.GetConnectionString()?.Length ?? 0)) + "..." }
            };

            if (rolesCount == 0)
            {
                _logger.LogWarning("Nenhuma role encontrada no banco de dados");
                return HealthCheckResult.Degraded("Banco conectado mas sem dados básicos (roles)", null, healthData);
            }

            _logger.LogDebug("Database Health Check passou - {RolesCount} roles encontradas", rolesCount);
            
            return HealthCheckResult.Healthy("Banco de dados funcionando corretamente", healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Database Health Check");
            return HealthCheckResult.Unhealthy($"Erro na verificação do banco: {ex.Message}");
        }
    }
}
