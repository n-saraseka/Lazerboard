using Microsoft.Extensions.DependencyInjection;

namespace Lazerboard.Data.Database.Work;

public class UnitOfWorkFactory(IServiceScopeFactory scopeFactory) : IUnitOfWorkFactory
{
    public IUnitOfWork Create() => new UnitOfWork(scopeFactory.CreateScope());
}