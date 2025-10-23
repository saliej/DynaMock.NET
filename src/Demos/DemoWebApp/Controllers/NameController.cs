using DemoWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class NameController : ControllerBase
{
	private readonly INameGenerationService _nameService;

	public NameController(INameGenerationService nameService)
	{
		_nameService = nameService;
	}

	[HttpGet("Base")]
	public string GetGeneratedeName(CancellationToken cancellationToken)
	{
		return _nameService.GenerateBaseName();
	}

	[HttpGet("Implemented")]
	public string GetCurrentDate()
	{
		return _nameService.GenerateImplementedName();
	}
}