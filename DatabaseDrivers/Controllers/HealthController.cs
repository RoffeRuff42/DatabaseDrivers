using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
[Tags("Health")]
public class HealthController : ControllerBase
{
	private readonly SecretClient? _secretClient;
	private readonly IConfiguration _configuration;
	private readonly IWebHostEnvironment _environment;

	public HealthController(IConfiguration configuration, IWebHostEnvironment environment, IServiceProvider serviceProvider)
	{
		_configuration = configuration;
		_environment = environment;
		_secretClient = serviceProvider.GetService<SecretClient>();
	}

	[HttpGet]
	public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
	{
		var secretName = _configuration["KeyVault:SecretName"];
		var keyVaultUrl = _configuration["KeyVault:Url"];

		var keyVaultStatus = "Not configured";
		var secretStatus = "Not checked";
		string? error = null;

		var canReadSecret = false;

		try
		{
			var response = await _secretClient.GetSecretAsync(
				secretName,
				cancellationToken: cancellationToken
			);

			var secret = response.Value;

			canReadSecret = !string.IsNullOrWhiteSpace(secret.Value);

			keyVaultStatus = "Reachable";
			secretStatus = canReadSecret ? "Secret found" : "Secret empty";
		}
		catch (RequestFailedException ex) when (ex.Status == 403)
		{
			keyVaultStatus = "Forbidden";
			secretStatus = "Cannot read secret";
			error = "The app does not have permission to read secrets from Key Vault.";
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			keyVaultStatus = "Reachable";
			secretStatus = "Secret not found";
			error = $"Secret was not found in Key Vault.";
		}
		catch (Exception ex)
		{
			keyVaultStatus = "Error";
			secretStatus = "Unknown";
			error = ex.GetType().Name;
		}
		

		var isHealthy = canReadSecret;

		var result = new
		{
			status = isHealthy ? "Healthy" : "Unhealthy",
			timestampUtc = DateTime.UtcNow,
			environment = _environment.EnvironmentName,

			keyVault = new
			{
				urlConfigured = !string.IsNullOrWhiteSpace(keyVaultUrl),
				status = keyVaultStatus,
				secretStatus
			},

			error
		};

		if (isHealthy)
			return Ok(result);

		return StatusCode(StatusCodes.Status503ServiceUnavailable, result);
	}
}