namespace Authentifications.Middlewares;
public class ValidationHandlingMiddleware
{
	private readonly RequestDelegate _next;

	public ValidationHandlingMiddleware(RequestDelegate next)
	{
		_next = next;
	}
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context); 
		}
		catch (Exception ex)
		{
			// Gestion des exceptions non gérées (par exemple : erreurs 500)
			await HandleExceptionAsync(context, ex);
		}
		// Vérifier si le modèle est invalide et gérer manuellement l'erreur 400
		if (context.Items.ContainsKey("ModelValidationErrors") && !context.Response.HasStarted)
		{
			var validationErrors = (List<string>)context.Items["ModelValidationErrors"]!;
			context.Response.StatusCode = 400;
			context.Response.ContentType = "application/json";
			var response = new
			{
				Type = "DataModelValidationError",
				Title = "Champs potentiellement vide.",
				Status = 400,
				Errors = validationErrors
			};

			await context.Response.WriteAsJsonAsync(response);
		}
	}
	private Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = 500;

		var response = new
		{
			Type = "ServerError",
			Title = "An unexpected error occurred.",
			Status = 500,
			TraceId = context.TraceIdentifier,
			Message = exception.Message
		};

		return context.Response.WriteAsJsonAsync(response);
	}
}
