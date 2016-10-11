namespace Carbon.Framework.UnitOfWork
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork NewUnitOfWork();
    }
}