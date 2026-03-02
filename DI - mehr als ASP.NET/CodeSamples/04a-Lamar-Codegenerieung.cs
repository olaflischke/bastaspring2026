// ===========================================
// Codegenerierung für Lamar - Beispiele
// ===========================================

// Beispielhafte Service-Implementierung
public interface IDataProvider { }
public class SqlDataProvider : IDataProvider { }

public interface IRepository
{
    IDataProvider Provider { get; }
}

public class Repository : IRepository
{
    public Repository(IDataProvider provider) => Provider = provider;
    public IDataProvider Provider { get; }
}

/*
// Manuelles Registrieren des RepositoryServices 
Func<IServiceProvider, IRepository> factory = provider =>
{
    var dataProvider = provider.GetRequiredService<IDataProvider>();
    return new Repository(dataProvider);
};
*/


// Lamar-Registry mit Factory
public class Lamar_Generated_Repository_Factory : Lamar.IoC.Resolvers.TransientResolver<IRepository>
{
    public override IRepository Build(Lamar.IoC.Scope scope)
    {
        var provider = new SqlDataProvider();
        return new Repository(provider);
    }
}

/*
// Übergabe an LamarCompiler/Roslyn
var generator = new AssemblyGenerator();
var assembly = generator.Generate(x =>
{
    x.Namespace("Generated");
    x.StartClass("Lamar_Generated_Repository_Factory",
                 typeof(Lamar.IoC.Resolvers.TransientResolver<IRepository>));

    using (x.Method("public override IRepository Build(Lamar.IoC.Scope scope)"))
    {
        x.Write("var provider = new SqlDataProvider();");
        x.Write("return new Repository(provider);");
    }

    x.FinishBlock(); // method
    x.FinishBlock(); // class
});
*/