using DemoWebApp.Repositories;

namespace DemoWebApp.Services;

public interface IStorageService
{
	List<string> GetAllItems();
}

public class StorageService(IRepository repository) : IStorageService
{
	public List<string> GetAllItems() =>
		repository.GetFooItems()
			.Concat(repository.GetBarItems())
			.Concat(repository.GetBazItems())
			.ToList();
}
