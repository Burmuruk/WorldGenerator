using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldG.Architecture
{
    public class BuildingWorker : IWorker
    {
        public event Action OnStop;

        public void Sleep()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            OnStop?.Invoke();
        }
    }

    public interface IWorker
    {
        event Action OnStop;

        void Sleep();

        void Stop();
    }
}
