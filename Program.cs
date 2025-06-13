
using Microsoft.EntityFrameworkCore;
using PruebaFina.Data;



namespace PruebaFinarogram
{
    public class Program
    { 
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Obtiene la cadena de conexi�n desde appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Configura el contexto de base de datos
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

            // Registra HttpClient para consumo de APIs externas (como CriptoYa)
            builder.Services.AddHttpClient();

            // Agrega servicios de controladores y Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ... c�digo anterior ...

            // Configura CORS para permitir cualquier origen, m�todo y encabezado
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirTodo", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Habilita CORS con la pol�tica definida
            app.UseCors("PermitirTodo");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();



        }

    }

}


