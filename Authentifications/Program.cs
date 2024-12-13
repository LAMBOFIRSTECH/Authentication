using System.Reflection;
using Authentifications.DataBaseContext;
using Authentifications.Interfaces;
using Authentifications.Middlewares;
using Authentifications.Repositories;
using Authentifications.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Conteneur d'enregistrement de dépendances-------------------------------- 

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
	opt.SwaggerDoc("1", new OpenApiInfo
	{
		Title = "Authentification service | Api",
		Description = "An ASP.NET Core Web API for managing Users authentification",
		Version = "1",
		Contact = new OpenApiContact
		{
			Name = "Artur Lambo",
			Email = "lamboartur94@gmail.com"
		}
	});

	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

});
builder.Services.AddCors(options =>
{
	options.AddPolicy(name: MyAllowSpecificOrigins,
					  policy =>
					  {
						  policy.AllowAnyOrigin()
						   .AllowAnyMethod()
						   .AllowAnyHeader();
					  });
});

builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: false);

builder.Services.AddHttpClient();
builder.Services.AddScoped<ApiContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddHealthChecks();


/*
	+----------------------------------------------------------------------+
	|Enregistrement de services Injectées lorsqu'une interface est démandée|
	+----------------------------------------------------------------------+
*/
builder.Services.AddScoped<IJwtToken, JwtBearerAuthenticationService>();

/* 
	+----------------------------------------------------+
	| Enregistrement de repositories/Services Injectés directement|
	+----------------------------------------------------+
*/
builder.Services.AddScoped<JwtBearerAuthenticationRepository>();
builder.Services.AddScoped<JwtBearerAuthenticationService>();
builder.Services.AddScoped<RedisCacheService>();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddLogging();
builder.Services.AddAuthorization();

builder.Services.AddAuthentication("BasicAuthentication")
	.AddScheme<AuthenticationSchemeOptions, AuthentificationBasicService>("BasicAuthentication", options => { });
var redisConfig = builder.Configuration.GetSection("Redis");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig["ConnectionString"];
    options.InstanceName = redisConfig["InstanceName"];
});

	
// C'est dans TasksManagementAPI toute cette partie

// builder.Services.AddAuthentication("JwtAuthorization")
// 	.AddScheme<JwtBearerOptions, JwtBearerAuthorizationServer>("JwtAuthorization", options =>
// 	{
// 		var JwtSettings = builder.Configuration.GetSection("JwtSettings");
// 		var secretKeyLength = int.Parse(JwtSettings["JwtSecretKey"]);
// 		var randomSecretKey = new RandomUserSecret();
// 		var signingKey = randomSecretKey.GenerateRandomKey(secretKeyLength);

// 		options.SaveToken = true;
// 		options.RequireHttpsMetadata = false;
// 		options.TokenValidationParameters = new TokenValidationParameters
// 		{
// 			ValidateIssuer = true,             // Valider l'émetteur (issuer) du jeton
// 			ValidateAudience = true,           // Valider l'audience du jeton
// 			ValidateLifetime = true,           // Valider la durée de vie du jeton
// 			ValidateIssuerSigningKey = true,   // Valider la signature du jeton

// 			ValidIssuer = JwtSettings["Issuer"],       // Émetteur (issuer) valide
// 			ValidAudience = JwtSettings["Audience"],   // Audience valide
// 			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)) 
// 		};
// 	});

// builder.Services.AddAuthorization(options =>
//  {
// 	 // Politique d'autorisation pour les administrateurs
// 	 options.AddPolicy("AdminPolicy", policy =>
// 		 policy.RequireRole(nameof(UtilisateurDto.Privilege.Administrateur))
// 			   .RequireAuthenticatedUser()
// 			   .AddAuthenticationSchemes("JwtAuthorization"));
 
// 	 options.AddPolicy("UserPolicy", policy =>
// 			   policy.RequireAuthenticatedUser() 
// 			   .AddAuthenticationSchemes("BasicAuthentication"));

//  });
var app = builder.Build();

/* 
	+----------------------------------------------------+
	| Enregistrement de middlewares Injection directe	 |
	+----------------------------------------------------+
*/
//app.UseMiddleware<ContextPathMiddleware>("/lambo-authentification-manager"); // A terme on ne pourra plus accéder au swagger avec /index.html
app.UseMiddleware<ValidationHandlingMiddleware>(); // Le fait de le placer ici avant UseRouting garanti le fait que si la validation n'est pas correct la requete n'atteinge pas le controlleur
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(con =>
	 {
		 con.SwaggerEndpoint("/swagger/1/swagger.json", "Gestion des authentification");

		 con.RoutePrefix = string.Empty;

	 });
}
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
 {
	 endpoints.MapControllers();
	 endpoints.MapHealthChecks("/health");
	 endpoints.MapGet("/version", async context =>
		{
			await context.Response.WriteAsync("Version de l'API : 1");
		});
 });
app.Run();
