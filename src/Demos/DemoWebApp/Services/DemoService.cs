namespace DemoWebApp.Services;


public interface IDemoService
{
	string GetName();
	DateTime GetCurrentDate();
	Task<int> GetRandomNumberAsync()
		=> Task.FromResult(Random.Shared.Next(1, 101));
}

public class DemoService : IDemoService
{
	public string GetName() => "DemoService";

	public DateTime GetCurrentDate() => DateTime.Now;
}
