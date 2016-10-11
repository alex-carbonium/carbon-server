namespace Carbon.Framework.Dependencies
{
    //reference anything in Ninject.Extensions.ContextPreservation to make it deployed to other projects
    public class ContextPerservationDeployer
    {
        public static void ReferenceMethod()
        {
            new Ninject.Extensions.ContextPreservation.ContextPreservingResolutionRootActivationStrategy();
            new Ninject.Extensions.Factory.FuncModule();
            new Ninject.Extensions.NamedScope.NamedScopeModule();
        }
    }
}
