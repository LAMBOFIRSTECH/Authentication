using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Authentifications.Interfaces;
using Authentifications.DataBaseContext;
using Microsoft.AspNetCore.Authorization;

namespace Authentifications.Controllers;
// [ApiController] il gère lui meme 
[Route("api/v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly IJwtToken jwtToken;
	private readonly ApiContext context ;
	
	public AccessTokenController(IJwtToken jwtToken,ApiContext context )
	{
		this.jwtToken = jwtToken; 
		this.context = context; 
		
	}
	/// <param name="email"></param>
	/// <param name="secretUser"></param> // je veux le mot de passe utilisateur ici
	/// <returns></returns>
	[HttpPost("token")]
	public async Task<ActionResult> Authentification(string email, [DataType(DataType.Password)] string secretUser)
	{
		if (!ModelState.IsValid)
		{
			// Ajouter les erreurs de validation dans HttpContext.Items
			var validationErrors = ModelState.Values
				.SelectMany(v => v.Errors)
				.Select(e => e.ErrorMessage)
				.ToList();
			HttpContext.Items["ModelValidationErrors"] = validationErrors;
			return StatusCode(422);
		}
		string regexMatch = "(?<alpha>\\w+)@(?<mailing>[aA-zZ]+)\\.(?<domaine>[aA-zZ]+$)";
		Match check = Regex.Match(email, regexMatch);

		if (!check.Success)
		{
			return BadRequest(new { Errors = "Cette adresse mail est invalide" });
		};

		if (jwtToken.CheckUserSecret(secretUser) == false)
		{
			return Unauthorized("Votre clé secrète incorrect");
		}
		var result = await jwtToken.GetToken(email);
		if (!result.Response)
		{
			return Unauthorized(new { result.Message });
		}
		return Ok(new { result.Token });
	}
	[Authorize]
	[HttpGet("users")]
	public async Task<ActionResult> Users()
	{
		await Task.Delay(10);
		return Ok(context.Utilisateurs.ToList());
	}
}