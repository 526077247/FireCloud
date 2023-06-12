using System;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// hud
    /// </summary>
    public abstract class HudInfo: IDisposable
    {
        private Transform _parent;
        public RectTransform hud { get; private set; }
        public Vector2 offset;
        public long attachTime;
        public long disposeTime;

        protected bool _isDisposable = true;
        public int configId { get; private set; }
        public HudDataConfig config => HudDataConfigCategory.Instance.Get(configId);
        /// <summary>
        /// 回收
        /// </summary>
        public virtual void Dispose()
        {
            if(_isDisposable) return;
            _isDisposable = true;
            if (hud != null)
            {
                GameObjectPoolManager.Instance?.RecycleGameObject(hud.gameObject);
                hud = null;
            }
            attachTime = default;
            offset = default;
            disposeTime = default;
            _parent = null;
            ObjectPool.Instance.Recycle(this);
        }
        /// <summary>
        /// 调用base时注意先设置参数，再调用base
        /// </summary>
        /// <param name="id"></param>
        protected async ETTask OnInit(int id)
        {
            _isDisposable = false;
            configId = id;

            var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(config.ResName);
            if (_isDisposable)//等加载回来可能已经销毁了
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }
            hud = obj.GetComponent<RectTransform>();
            OnGameObjectLoad();
        }

        /// <summary>
        /// 当预制体从池中加载出来
        /// </summary>
        protected virtual void OnGameObjectLoad()
        {
            if (_parent != null && hud != null)
            {
                hud.SetParent(_parent);
                hud.localPosition = Vector3.zero;
                hud.localScale = Vector3.one;
            }
        }
        
        /// <summary>
        /// 当设置Unity侧父物体时
        /// </summary>
        /// <param name="transform"></param>
        public virtual void OnAttach(Transform transform)
        {
            if (_parent != transform)
            {
                _parent = transform;
                if (hud != null)
                {
                    hud.SetParent(_parent);
                    hud.localPosition = Vector3.zero;
                    hud.localScale = Vector3.one;
                }
            }
            this.attachTime = GameTimerManager.Instance.GetTimeNow();
            if (this.config.LifeTime > 0)
            {
                this.disposeTime = this.attachTime + this.config.LifeTime;
            }
        }
                
        /// <summary>
        /// 获取世界坐标
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 GetWorldPos();
    }
}