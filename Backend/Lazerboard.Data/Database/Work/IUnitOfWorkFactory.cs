namespace Lazerboard.Data.Database.Work;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}