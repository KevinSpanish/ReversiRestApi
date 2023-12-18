using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ReversiRestApi.DAL;

var builder = WebApplication.CreateBuilder(args);
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ReversiRestApi", Version = "v1" });
});

builder.Services.AddCors(
                options => {
                    options.AddPolicy(
                        name: MyAllowSpecificOrigins,
                        builder => {
                            builder.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        }
                    );
                }
            );

builder.Services.AddDbContext<SpelContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ReversiRestReparatieApiDb")));

//builder.Services.AddScoped<ISpelRepository, SpelRepository>(); //DI
builder.Services.AddScoped<ISpelRepository, SpelAccessLayer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReversiRestApi v1"));
}

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
