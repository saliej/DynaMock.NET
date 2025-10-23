namespace DemoWebApp.Services;

public abstract class BaseNameGenerator
{
	public virtual string GetOverridableName() => "BaseName";
	public abstract string GetImplementedName();
}

public class ConcreteNameGenerator : BaseNameGenerator
{
	public override string GetOverridableName() => "OverridedName";
	public override string GetImplementedName() => "ImplementedName";
}

public interface INameGenerationService
{
	string GenerateBaseName();
	string GenerateImplementedName();
}

public class NameGenerationService(BaseNameGenerator nameGenerator) : INameGenerationService
{
	public string GenerateBaseName() => nameGenerator.GetOverridableName();
	public string GenerateImplementedName() => nameGenerator.GetImplementedName();
}
