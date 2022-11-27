using System;
using UnityEngine;

namespace TaoTie
{
    public class DamageInfo :IDisposable
    {
        public float Value; //生命值变化量（生命值大于0的部分）
        public float RealValue; //全额伤害值
        public float NowHp; //现在生命
        public Vector3 HitPos;
        
        public static DamageInfo Create()
        {
            return ObjectPool.Instance.Fetch(typeof (DamageInfo)) as DamageInfo;
        }

        public void Dispose()
        {
            Value = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}