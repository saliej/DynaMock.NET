using DemoWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
	private readonly IDemoService _demoService;

	public DemoController(IDemoService demoService)
	{
		_demoService = demoService;
	}

	[HttpGet("ServiceName")]
	public string GetServiceName(CancellationToken cancellationToken)
	{
		return _demoService.GetName();
	}

	[HttpGet("CurrentDate")]
	public DateTime GetCurrentDate()
	{
		return _demoService.GetCurrentDate();
	}

	[HttpGet("RandomInt")]
	public Task<int> GetRandomInt()
	{
		return _demoService.GetRandomNumberAsync();
	}
}
