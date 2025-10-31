using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModulPanel.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔹 1. MySQL connection string'i oku
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔹 2. DbContext'i servislere ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 🔹 3. JWT Authentication ayarlarını ekle
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// 🔹 4. Controller ve Swagger servisi (JWT destekli)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ModulPanel API", Version = "v1" });

    // 🔹 JWT için Security tanımı
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Lütfen 'Bearer <token>' formatında JWT token girin.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // 🔹 Tüm endpoint'lerde Authorize butonunu aktif et
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 🔹 5. Servisleri ekle
builder.Services.AddScoped<ModulPanel.Services.AuthService>();
builder.Services.AddScoped<ModulPanel.Services.UserService>();

var app = builder.Build();

// 🔹 6. Pipeline yapılandırması
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔹 7. JWT middleware'lerini aktif et
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 🔹 8. Database seed işlemi (ilk admin kullanıcısı)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // tablo yoksa oluştur
    DataSeeder.SeedAdmin(db);   // admin ekle
}

app.Run();
