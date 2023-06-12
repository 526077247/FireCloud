using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 追随目标hud
    /// </summary>
    public abstract class FollowHudInfo:HudInfo
    {
        private Transform _followObj;

        protected void OnInit(int id,Transform followObj)
        {
            _followObj = followObj;
            base.OnInit(id).Coroutine();
        }

        public override Vector3 GetWorldPos()
        {
            return _followObj.position;
        }

        public override void Dispose()
        {
            base.Dispose();
            _followObj = null;
        }
    }
}