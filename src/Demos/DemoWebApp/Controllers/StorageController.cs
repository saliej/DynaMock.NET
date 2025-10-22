using DemoWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class StorageController : ControllerBase
{
	private readonly IStorageService _storageService;

	public StorageController(IStorageService storageService)
	{
		_storageService = storageService;
	}

	[HttpGet("AllItems")]
	public List<string> GetAllItems(CancellationToken cancellationToken)
	{
		return _storageService.GetAllItems();
	}
}
