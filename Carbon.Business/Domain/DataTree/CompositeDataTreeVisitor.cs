namespace Carbon.Business.Domain.DataTree
{
    public class CompositeDataTreeVisitor : DataTreeVisitor  
    {
        private readonly DataTreeVisitor[] _visitors;

        public CompositeDataTreeVisitor(params DataTreeVisitor[] visitors)
        {
            _visitors = visitors;
        }

        public override bool Visit(DataNode element, DataNodePath path)
        {            
            foreach (var visitor in _visitors)
            {
                if (!visitor.Visit(element, path))
                {
                    return false;
                }
            }
            return true;
        }
    }
}