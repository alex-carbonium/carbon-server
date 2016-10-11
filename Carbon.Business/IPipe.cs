using System.Collections.Generic;

namespace Carbon.Business
{
    public interface IPipe<T>
    {        
        IEnumerable<T> Write();     
        void Read(IEnumerable<T> buffers);

        void GetStatistics(out int count, out int max, out int total);        
    }
}
