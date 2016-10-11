namespace Carbon.Business.Services
{
    public interface IJavaScriptEngine
    {
        void AddHostObject(string item, object data);
        void Execute(string code);
    }
}