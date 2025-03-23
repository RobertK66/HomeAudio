
using Scalar.AspNetCore;
using DbWebApi.Controllers;
using WebRadioImpl;

namespace DbWebApi {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();      

                        builder.Services.AddEndpointsApiExplorer();

                        builder.Services.AddSwaggerGen();
            var connectionString = "Server=THINKP-15\\DEVSERVER;Database=HomeAudio;Trusted_Connection=True;TrustServerCertificate=true;";
            builder.Services.AddSqlServer<MyDataContext>(connectionString);


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
                // 2 choices of UI -> see at /swagger, /scalar/v1
                app.UseSwaggerUI(options => {
                    options.SwaggerEndpoint("/openapi/v1.json", "My Demo");
                });
                app.MapScalarApiReference();
            }

                        if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

                        app.MapWebRadioEndpoints();

            app.Run();
        }
    }
}
