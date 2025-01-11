namespace Authentifications.Middlewares
{
	public class ContextPathMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly PathString _contextPath;
		public ContextPathMiddleware(RequestDelegate next, string contextPath)
		{
			_next = next;
			_contextPath = new PathString(contextPath);
		}
		public async Task InvokeAsync(HttpContext context)
		{
			if (context.Request.Path.StartsWithSegments(_contextPath, out var remainingPath))
			{
				context.Request.Path = remainingPath;
				await _next(context);
			}
			else
			{
				context.Response.StatusCode = StatusCodes.Status404NotFound;
			}
		}
	}
}
// namespace Authentifications.Middlewares;
// public class ContextPathMiddleware
// {
// 	private readonly RequestDelegate _next;
// 	private readonly PathString _contextPath;

// 	public ContextPathMiddleware(RequestDelegate next, string contextPath)
// 	{
// 		_next = next;
// 		_contextPath = new PathString(contextPath);
// 	}

// 	public async Task InvokeAsync(HttpContext context)
// 	{
// 		// Liste des chemins à ignorer ou à autoriser directement
// 		var allowedPaths = new[]
// 		{
// 				"/health",
// 				"/swagger",
// 				"/swagger/index.html",
// 				"/swagger/1/swagger.json",
// 				"/api/auth/refreshToken",
// 				"/api/auth/login",
// 				"/version"
// 			};

// 		// Si la requête correspond à un chemin autorisé, ne pas appliquer la logique du middleware
// 		if (allowedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
// 		{
// 			await _next(context);
// 			return;
// 		}

// 		// Vérifie si le chemin commence par le contexte configuré
// 		if (context.Request.Path.StartsWithSegments(_contextPath, out var remainingPath))
// 		{
// 			context.Request.Path = remainingPath; // Supprime le préfixe pour le traitement interne
// 			await _next(context); // Continue avec le prochain middleware
// 		}
// 		else
// 		{
// 			context.Response.StatusCode = StatusCodes.Status404NotFound; // Requête invalide
// 		}
// 	}
// }
