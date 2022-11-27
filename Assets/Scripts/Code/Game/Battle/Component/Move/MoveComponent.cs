using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class MoveComponent:Component,IComponent
    {
        [Timer(TimerType.MoveTimer)]
        public class AMoveTimer: ATimer<MoveComponent>
        {
            public override void Run(MoveComponent self)
            {
                try
                {
                    if (self.IsDisposed) return;
                    self.MoveForward(false);
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }

        public Vector3 PreTarget
        {
            get
            {
                return this.Targets[this.N - 1];
            }
        }

        public Vector3 NextTarget
        {
            get
            {
                return this.Targets[this.N];
            }
        }

        // 开启移动协程的时间
        public long BeginTime;

        // 每个点的开始时间
        public long StartTime { get; set; }
        // 上次Update的开始时间
        public long UpdateTime { get; set; }
        // 开启移动协程的Unit的位置
        public Vector3 StartPos;

        public Vector3 RealPos
        {
            get
            {
                return this.Targets[0];
            }
        }

        private long needTime;

        public long NeedTime
        {
            get
            {
                return this.needTime;
            }
            set
            {
                this.needTime = value;
            }
        }

        public long MoveTimer;

        public float Speed; // m/s

        public Action<bool> Callback;

        public List<Vector3> Targets = new List<Vector3>();

        public Vector3 FinalTarget
        {
            get
            {
                return this.Targets[this.Targets.Count - 1];
            }
        }

        public int N;

        public int TurnTime;

        public bool IsTurnHorizontal;

        public Quaternion From;

        public Quaternion To;
        
        public bool Enable { get; set; }//是否允许移动

        private bool IsDisposed;
        #region override

        public void Init()
        {
            this.StartTime = 0;
            this.StartPos = Vector3.zero;
            this.NeedTime = 0;
            this.MoveTimer = 0;
            this.Callback = null;
            this.Targets.Clear();
            this.Speed = 0;
            this.Enable = true;
            this.N = 0;
            this.TurnTime = 0;
            IsDisposed = false;
            Messager.Instance.AddListener<int,bool>(Id,MessageId.ActionControlActiveChange,ActionControlActiveChange);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<int,bool>(Id,MessageId.ActionControlActiveChange,ActionControlActiveChange);
            IsDisposed = true;
            this.Stop();
        }

        private void ActionControlActiveChange(int type, bool value)
        {
            if (type == ActionControlType.BanMove)
            {
                this.Enable = value;
            }
        }
        #endregion

        public bool IsArrived()
        {
            return this.Targets.Count == 0;
        }

        public bool ChangeSpeed(float speed)
        {
            if (this.IsArrived())
            {
                return false;
            }

            if (speed < 0.0001)
            {
                return false;
            }
            
            Unit unit = this.GetParent<Unit>();

            using (ListComponent<Vector3> path = ListComponent<Vector3>.Create())
            {
                this.MoveForward(true);
                
                path.Add(unit.Position); // 第一个是Unit的pos
                for (int i = this.N; i < this.Targets.Count; ++i)
                {
                    path.Add(this.Targets[i]);
                }
                this.MoveToAsync(path, speed).Coroutine();
            }
            return true;
        }

        public async ETTask<bool> MoveToAsync(List<Vector3> target, float speed, int turnTime = 100, ETCancellationToken cancellationToken = null)
        {
            this.Stop();

            foreach (Vector3 v in target)
            {
                this.Targets.Add(v);
            }

            this.IsTurnHorizontal = true;
            this.TurnTime = turnTime;
            this.Speed = speed;
            ETTask<bool> tcs = ETTask<bool>.Create(true);
            this.Callback = (ret) => { tcs.SetResult(ret); };

            Messager.Instance.Broadcast(Id,MessageId.MoveStart,this.GetParent<Unit>());

            this.StartMove();
            
            void CancelAction()
            {
                this.Stop();
            }
            
            bool moveRet;
            try
            {
                cancellationToken?.Add(CancelAction);
                moveRet = await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }

            if (moveRet)
            {
                Messager.Instance.Broadcast(Id,MessageId.MoveStop,this.GetParent<Unit>());
            }
            return moveRet;
        }

        public void MoveForward(bool needCancel)
        {
            long lastUpdateTime = this.UpdateTime;
            this.UpdateTime = TimeHelper.ClientNow();
            if (!this.Enable)
            {
                this.StartTime += this.UpdateTime - lastUpdateTime;
                
                return;
            }
            Unit unit = this.GetParent<Unit>();
            
            
            long moveTime = this.UpdateTime - this.StartTime;

            while (true)
            {
                if (moveTime <= 0)
                {
                    return;
                }
                
                // 计算位置插值
                if (moveTime >= this.NeedTime)
                {
                    unit.Position = this.NextTarget;
                    if (this.TurnTime > 0)
                    {
                        unit.Rotation = this.To;
                    }
                }
                else
                {
                    // 计算位置插值
                    float amount = moveTime * 1f / this.NeedTime;
                    if (amount > 0)
                    {
                        Vector3 newPos = Vector3.Lerp(this.StartPos, this.NextTarget, amount);
                        unit.Position = newPos;
                    }
                    
                    // 计算方向插值
                    if (this.TurnTime > 0)
                    {
                        amount = moveTime * 1f / this.TurnTime;
                        Quaternion q = Quaternion.Slerp(this.From, this.To, amount);
                        unit.Rotation = q;
                    }
                }

                moveTime -= this.NeedTime;

                // 表示这个点还没走完，等下一帧再来
                if (moveTime < 0)
                {
                    return;
                }
                
                // 到这里说明这个点已经走完
                
                // 如果是最后一个点
                if (this.N >= this.Targets.Count - 1)
                {
                    if(this.Targets.Count>0)
                        unit.Position = this.NextTarget;
                    unit.Rotation = this.To;
                    Action<bool> callback = this.Callback;
                    this.Callback = null;

                    this.Clear();
                    callback?.Invoke(!needCancel);
                    return;
                }

                this.SetNextTarget();
            }
        }

        private void StartMove()
        {
            Unit unit = this.GetParent<Unit>();
            
            this.BeginTime = TimeHelper.ClientNow();
            this.StartTime = this.BeginTime;
            this.SetNextTarget();

            this.MoveTimer = GameTimerManager.Instance.NewFrameTimer(TimerType.MoveTimer, this);
        }

        private void SetNextTarget()
        {

            Unit unit = this.GetParent<Unit>();

            ++this.N;

            // 时间计算用服务端的位置, 但是移动要用客户端的位置来插值
            Vector3 v = this.GetFaceV();
            float distance = v.magnitude;
            
            // 插值的起始点要以unit的真实位置来算
            this.StartPos = unit.Position;

            this.StartTime += this.NeedTime;
            
            this.NeedTime = (long) (distance / this.Speed * 1000);

            
            if (this.TurnTime > 0)
            {
                // 要用unit的位置
                Vector3 faceV = this.GetFaceV();
                if (faceV.sqrMagnitude < 0.0001f)
                {
                    return;
                }
                this.From = unit.Rotation;
                
                if (this.IsTurnHorizontal)
                {
                    faceV.y = 0;
                }

                if (Math.Abs(faceV.x) > 0.01 || Math.Abs(faceV.z) > 0.01)
                {
                    this.To = Quaternion.LookRotation(faceV, Vector3.up);
                }

                return;
            }
            
            if (this.TurnTime == 0) // turn time == 0 立即转向
            {
                Vector3 faceV = this.GetFaceV();
                if (this.IsTurnHorizontal)
                {
                    faceV.y = 0;
                }

                if (Math.Abs(faceV.x) > 0.01 || Math.Abs(faceV.z) > 0.01)
                {
                    this.To = Quaternion.LookRotation(faceV, Vector3.up);
                    unit.Rotation = this.To;
                }
            }
        }

        private Vector3 GetFaceV()
        {
            return this.NextTarget - this.PreTarget;
        }

        public bool FlashTo(Vector3 target)
        {
            Unit unit = this.GetParent<Unit>();
            unit.Position = target;
            return true;
        }
        
        public void Stop()
        {
            if (this.Targets.Count > 0)
            {
                this.MoveForward(true);
            }

            this.Clear();
        }

        public void Clear()
        {
            this.StartTime = 0;
            this.StartPos = Vector3.zero;
            this.BeginTime = 0;
            this.NeedTime = 0;
            GameTimerManager.Instance?.Remove(ref this.MoveTimer);
            this.Targets.Clear();
            this.Speed = 0;
            this.N = 0;
            this.TurnTime = 0;
            this.IsTurnHorizontal = false;

            if (this.Callback != null)
            {
                Action<bool> callback = this.Callback;
                this.Callback = null;
                callback.Invoke(false);
            }
        }
    }
}