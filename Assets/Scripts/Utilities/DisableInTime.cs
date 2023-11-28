using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MyDearAnima.Controll
{
    public class DisableInTime<T>
    {
        public readonly float time;
        private T item;

        public T Item { get => item; private set => item = value; }
        public bool IsDisabled { get; private set; }
        public bool RestartRequested { get; private set; } = false;
        public bool IsRunning { get; private set; }

        public DisableInTime(float time, T item)
        {
            this.time = time;
            this.item = item;
            IsDisabled = true;
        }

        public IEnumerator EnableInTime(bool shouldDisable = true)
        {
            IsRunning = true;

            do
            {
                RestartRequested = false;
                Enable(shouldDisable);

                yield return new WaitForSeconds(time);
            }
            while (RestartRequested);
            
            Enable(!shouldDisable); 
            IsRunning = false;
        }

        public void Enable(bool shouldEnable)
        {
            switch (item)
            {
                case GameObject g:
                    g.SetActive(shouldEnable);
                    break;

                case Image i:
                    i.enabled = shouldEnable;
                    break;
            }

            IsDisabled = shouldEnable;
        }

        public void Restart()
        {
            RestartRequested = true;
        }

        public void Repleace_Item(T item) => Item = item;
    }
}
