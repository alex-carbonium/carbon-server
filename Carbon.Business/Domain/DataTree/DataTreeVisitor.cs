namespace Carbon.Business.Domain.DataTree
{
    public abstract class DataTreeVisitor
    {
        public abstract bool Visit(DataNode element, DataNodePath path);
    }
}