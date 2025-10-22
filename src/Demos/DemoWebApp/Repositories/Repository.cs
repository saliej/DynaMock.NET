using Microsoft.AspNetCore.DataProtection.Repositories;

namespace DemoWebApp.Repositories;

public interface IRepository
{
	List<string> GetFooItems();
	List<string> GetBarItems();
	List<string> GetBazItems();
}

public class DemoRepository : IRepository
{
	public List<string> GetFooItems() => ["Foo1", "Foo2"];
	public List<string> GetBarItems() => ["Bar1", "Bar2"];
	public List<string> GetBazItems() => ["Baz1", "Baz2"];
}
