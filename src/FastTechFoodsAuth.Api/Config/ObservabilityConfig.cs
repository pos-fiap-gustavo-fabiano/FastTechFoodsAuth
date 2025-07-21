using FastTechFoods.Observability;
using FastTechFoodsAuth.Infra.Context;

namespace FastTechFoodsAuth.Api.Config
{
    public static class ObservabilityConfig
    {
        public static void AddObservability(WebApplicationBuilder builder)
        {
            builder.Services.AddFastTechFoodsObservabilityWithSerilog(builder.Configuration);
            builder.Services.AddFastTechFoodsPrometheus(builder.Configuration);
            builder.Services.AddFastTechFoodsHealthChecks<ApplicationDbContext>(builder.Configuration);
        }

        public static void UseObservability(WebApplication app)
        {
            app.UseFastTechFoodsPrometheus();
        }
    }
}
