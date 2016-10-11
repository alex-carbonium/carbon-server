namespace Carbon.Framework.JobScheduling
{
    public interface IJob
    {
        void Execute(JobContext context);
        void Initialize(string parameters);
    }

    public abstract class Job : IJob
    {
        public abstract void Execute(JobContext context);
        public virtual void Initialize(string parameters)
        {            
        }
    }
}