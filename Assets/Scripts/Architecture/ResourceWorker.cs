using System;

namespace WorldG.Architecture
{
    public class ResourceWorker : IWorker
    {
        public event Action OnStop;

        public int Health { get; private set; } = 100;

        public int TakeResource(int amount)
        {
            if (amount - Health >= 0)
            {
                var last = Health;
                Health = 0;
                Stop();

                return amount - last;
            }
            
            Health -= amount;

            return amount;
        }

        public void Sleep()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            OnStop?.Invoke();
            OnStop = null;
        }
    }
}
