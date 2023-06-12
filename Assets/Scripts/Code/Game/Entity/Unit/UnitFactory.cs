using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public static class UnitFactory
    {
        public static Skill CreateSkillCollider(EntityManager currentScene, int configId, Vector3 pos,Quaternion rota,SkillPara para)
        {
            Skill unit = currentScene.CreateEntity<Skill,int>(configId);
        
            unit.Position = pos;
            unit.Rotation = rota;
            var collider = SkillJudgeConfigCategory.Instance.Get(configId);
            if (collider.ColliderType == ColliderType.Target)//朝指定位置方向飞行碰撞体
            {
                var numc = unit.AddComponent<NumericComponent>();
                numc.Set(NumericType.SpeedBase, collider.Speed);
                var moveComp = unit.AddComponent<MoveComponent>();
                List<Vector3> target = new List<Vector3>();
                target.Add(pos);
                target.Add(pos + (para.Position - pos).normalized * collider.Speed * collider.Time / 1000f);
                moveComp.MoveToAsync(target, collider.Speed).Coroutine();
                unit.AddComponent<SkillColliderComponent,SkillPara,Vector3>(para,para.Position);
            }
            else if (collider.ColliderType == ColliderType.Aim) //锁定目标飞行
            {
                var numc = unit.AddComponent<NumericComponent>();
                numc.Set(NumericType.SpeedBase,collider.Speed);
                unit.AddComponent<MoveComponent>();
                unit.AddComponent<RunAfterAimComponent, Unit, Action>(para.To, () =>
                {
                    unit.Dispose();
                });
                unit.AddComponent<SkillColliderComponent,SkillPara,long>(para,para.To.Id);
            }
            else
            {
                unit.AddComponent<SkillColliderComponent, SkillPara>(para);
            }
            return unit;
        }
    }
}