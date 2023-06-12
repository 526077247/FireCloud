using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 固定位置hud
    /// </summary>
    public abstract class FixedHudInfo:HudInfo
    {
        private Vector3 _pos;

        protected void OnInit(int id,Vector3 pos)
        {
            this._pos = pos;
            base.OnInit(id).Coroutine();
        }

        public override Vector3 GetWorldPos()
        {
            return _pos;
        }

        public override void Dispose()
        {
            base.Dispose();
            _pos = default;
        }
    }
}