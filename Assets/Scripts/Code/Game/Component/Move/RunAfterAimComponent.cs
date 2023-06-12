using System;
using UnityEngine;

namespace TaoTie
{
    public class RunAfterAimComponent:Component,IComponent<Unit,Action>
    {
        [Timer(TimerType.RunAfterTimer)]
        public class RunAfterTimer: ATimer<RunAfterAimComponent>
        {
            public override void Run(RunAfterAimComponent self)
            {
                try
                {
                    self.Check();
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        private Unit Aim;
        private Action OnArrived;
        private long TimerId;

        public void Init(Unit a,Action b)
        {
            Aim = a;
            OnArrived = b;
            TimerId = GameTimerManager.Instance.NewRepeatedTimer(200, TimerType.RunAfterTimer, this);
        }

        public void Destroy()
        {
            Aim = null;
            OnArrived = null;
            GameTimerManager.Instance.Remove(ref TimerId);
        }
        
        public void Arrived()
        {
            Aim = null;
            OnArrived?.Invoke();
            OnArrived = null;
        }

        public void Check()
        {
            var myUnit = GetParent<Unit>();
            Vector3 nextTarget = Aim.Position;
            myUnit.MoveToAsync(nextTarget).Coroutine(); 
            
            
            if(Vector3.Distance(nextTarget,myUnit.Position)<0.1f) 
                Arrived();
        }
    }
}