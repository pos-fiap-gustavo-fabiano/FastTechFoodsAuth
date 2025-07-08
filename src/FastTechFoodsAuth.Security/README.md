# FastTechFoodsAuth.Security

Biblioteca compartilhada para autenticação JWT padronizada nos projetos FastTechFoods.

## ✨ Funcionalidades

- ✅ Configuração automática de autenticação JWT
- ✅ Validação robusta de configurações
- ✅ Swagger integrado com JWT Bearer
- ✅ Middleware de auditoria de segurança
- ✅ Logging detalhado de eventos de autenticação
- ✅ Mensagens de erro amigáveis
- ✅ Suporte a variáveis de ambiente para secrets

## 🚀 Uso Rápido

### 1. Instalar o pacote
```bash
dotnet add package FastTechFoodsAuth.Security
```

### 2. Configurar no `appsettings.json`
```json
{
  "Jwt": {
    "Key": "sua-chave-super-secreta-de-pelo-menos-32-caracteres",
    "Issuer": "FastTechFoods.Auth",
    "Audience": "FastTechFoods.Apps",
    "ExpirationHours": 24
  }
}
```

### 3. Configurar no `Program.cs`
```csharp
using FastTechFoodsAuth.Security.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ✨ Uma linha para configurar toda a autenticação JWT!
builder.Services.AddFastTechFoodsJwtAuthentication(builder.Configuration);

// ✨ Uma linha para configurar Swagger com JWT!
builder.Services.AddFastTechFoodsSwaggerWithJwt("Minha API");

var app = builder.Build();

// ✨ Middleware de auditoria de segurança (opcional)
app.UseFastTechFoodsSecurityAudit();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

## 🔐 Segurança

### Variáveis de Ambiente
Para máxima segurança, defina a chave JWT via variável de ambiente:
```bash
export JWT_SECRET="sua-chave-super-secreta-de-pelo-menos-32-caracteres"
```

### Validações Automáticas
- ✅ Chave mínima de 256 bits (32 caracteres)
- ✅ Issuer e Audience obrigatórios
- ✅ Validação de expiração
- ✅ Assinatura obrigatória
- ✅ Clock skew configurável

## 📊 Logging

A biblioteca fornece logging detalhado de todos os eventos de segurança:

```
[13:45:20 WRN] Falha na autenticação JWT: The token is expired | Path: /api/protected | IP: 192.168.1.100
[13:45:25 WRN] Tentativa de acesso não autorizado: GET /api/admin | IP: 192.168.1.100 | HasAuthHeader: False
```

## ⚙️ Configuração Avançada

```csharp
builder.Services.AddFastTechFoodsJwtAuthentication(
    builder.Configuration,
    options =>
    {
        options.ExpirationHours = 8; // Override para 8 horas
        options.ClockSkew = TimeSpan.FromMinutes(5); // Tolerância de 5 minutos
    });
```

## 🔄 Migração de Projeto Existente

**Antes (código repetitivo):**
```csharp
// 50+ linhas de configuração JWT repetidas em cada projeto
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? ...
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(options => { ... })
.AddJwtBearer(options => { ... });
// ... mais configurações
```

**Depois (simples e padronizado):**
```csharp
// 1 linha que faz tudo!
builder.Services.AddFastTechFoodsJwtAuthentication(builder.Configuration);
```

## 📦 Reutilização

Esta biblioteca pode ser usada em:
- ✅ APIs FastTechFoods
- ✅ Microserviços
- ✅ Gateways
- ✅ Qualquer aplicação ASP.NET Core

## 🛠️ Desenvolvimento

Para contribuir ou modificar a biblioteca:

1. Clone o repositório
2. Faça suas alterações
3. Aumente a versão no `.csproj`
4. Execute `dotnet pack` para gerar o NuGet
5. Publique o pacote

## 📝 Exemplos de Uso

Veja os projetos de exemplo na pasta `/examples` para casos de uso específicos.
