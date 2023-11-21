using System.Collections;
using UnityEngine;
using System;

namespace MyDearAnima.Controll
{
    public class CoolDownAction
    {
        private float time;
        private float currentTime;
        private bool canUse;
        private bool inCoolDown;
        private Action<bool> OnFinished;
        private bool invertFunction;

        public bool CanUse
        {
            get => canUse;
            set
            {
                if (inCoolDown)
                    return;

                canUse = invertFunction? !value : value;

                if (OnFinished != null)
                    OnFinished(canUse);
            }
        }

        public CoolDownAction(float time)
        {
            this.time = time;
            currentTime = 0;
            canUse = true;
            inCoolDown = false;
            OnFinished = null;
        }

        public CoolDownAction (float time, bool invert) : this (time)
        {
            invertFunction = invert;
            canUse = false;
        }

        public CoolDownAction(float time, Action<bool> OnFinished)
        {
            this.time = time;
            currentTime = 0;
            canUse = true;
            inCoolDown = false;
            this.OnFinished = OnFinished;
            invertFunction = false;
        }

        public CoolDownAction(float time, Action<bool> OnFinished, bool invert)
        {
            this.time = time;
            currentTime = 0;
            canUse = false;
            inCoolDown = false;
            this.OnFinished = OnFinished;
            invertFunction = invert;
        }

        public void Restart()
        {
            currentTime = 0;
            CanUse = true;
            inCoolDown = false;
        }

        public IEnumerator CoolDown()
        {
            if (inCoolDown) yield break;

            CanUse = false;
            inCoolDown = true;
            currentTime = time - Time.deltaTime;

            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            inCoolDown = false;
            CanUse = true;
        }
    }
}