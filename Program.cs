
namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            if (builder.Environment.IsProduction())
            {
                var portVar = Environment.GetEnvironmentVariable("PORT");
                if (!string.IsNullOrEmpty(portVar) && int.TryParse(portVar, out var port))
                {
                    builder.WebHost.ConfigureKestrel(options =>
                    {
                        options.ListenAnyIP(port);
                    });
                }
            }

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddHttpClient();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });


            var app = builder.Build();
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }




            app.MapControllers();

            app.Run();
        }
    }
}
