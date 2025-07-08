namespace FastTechFoodsAuth.Security.Configuration;

/// <summary>
/// Configurações para autenticação JWT
/// </summary>
public class JwtConfiguration
{
    /// <summary>
    /// Seção de configuração no appsettings.json
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Chave secreta para assinatura do token (256 bits mínimo)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Emissor do token (quem emite)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audiência do token (para quem é destinado)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Tempo de expiração do token em horas (padrão: 24h)
    /// </summary>
    public int ExpirationHours { get; set; } = 24;

    /// <summary>
    /// Clock skew para tolerância de tempo (padrão: 0)
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Valida se a configuração está completa e válida
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Key))
            throw new InvalidOperationException("JWT Key is required");

        if (Key.Length < 32) // 256 bits mínimo
            throw new InvalidOperationException("JWT Key must be at least 32 characters (256 bits)");

        if (string.IsNullOrEmpty(Issuer))
            throw new InvalidOperationException("JWT Issuer is required");

        if (string.IsNullOrEmpty(Audience))
            throw new InvalidOperationException("JWT Audience is required");

        if (ExpirationHours <= 0)
            throw new InvalidOperationException("JWT ExpirationHours must be greater than 0");
    }
}
