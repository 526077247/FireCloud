using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public static class BattleHelper
    {
        /// <summary>
        /// 结算伤害
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="broadcast"></param>
        public static void Damage(Unit from, Unit to, float value,Vector3 hitPos,bool broadcast = true)
        {
            // 由于AOI机制from可能为空，但不应该影响to的逻辑处理
            NumericComponent t = to.GetComponent<NumericComponent>();
            var buffF = from?.GetComponent<BuffHolderComponent>();
            var buffT = to.GetComponent<BuffHolderComponent>();
            DamageInfo info = DamageInfo.Create();
            info.Value = value;
            buffF?.BeforeDamage(from, to, info);
            buffT.BeforeDamage(from, to, info);
            int damageValue = (int)info.Value;
            info.Value = damageValue;
            if (damageValue != 0)
            {
                int realValue = damageValue;
                int now = t.GetAsInt(NumericType.Hp);
                int nowBaseValue = now - realValue;
                t.Set(NumericType.HpBase, nowBaseValue);
                info.NowHp = nowBaseValue;
                info.RealValue = realValue;
                info.HitPos = hitPos;
                if (broadcast)
                {
                    if(from!=null)
                        Messager.Instance.Broadcast(from.Id,MessageId.AfterCombatUnitGetDamage,from, to, info);
                    Messager.Instance.Broadcast(to.Id,MessageId.AfterCombatUnitGetDamage,from, to, info);
                    Messager.Instance.Broadcast(0,MessageId.AfterCombatUnitGetDamage,from, to, info);
                }
            }
            buffT.AfterDamage(from, to, info);
            buffF?.AfterDamage(from, to, info);
            info.Dispose();
        }
        
        /// <summary>
        /// 当触发
        /// </summary>
        /// <param name="type"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stepPara"></param>
        /// <param name="costId"></param>
        /// <param name="cost"></param>
        /// <param name="config"></param>
        /// <param name="skill"></param>
        public static void OnCollider(Vector3 hitPos,TriggerType type, Unit from, Unit to, SkillStepPara stepPara, List<int> costId,
            List<int> cost,SkillConfig config,Skill skill = null)
        {
            if (type == TriggerType.Enter)
            {
                OnColliderIn(hitPos,from, to, stepPara, costId, cost,config,skill);
            }
            else if (type == TriggerType.Exit)
            {
                OnColliderOut(hitPos,from, to, stepPara, costId, cost,config,skill);
            }
        }  
        /// <summary>
        /// 进入触发器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stepPara"></param>
        /// <param name="costId"></param>
        /// <param name="cost"></param>
        /// <param name="config"></param>
        /// <param name="skill">技能判断体</param>
        public static void OnColliderIn(Vector3 hitPos,Unit from, Unit to, SkillStepPara stepPara, List<int> costId,
            List<int> cost,SkillConfig config,Skill skill = null)
        {
            if(from==null||to==null) return;//伤害计算参与者无了
            var combatToU = to;
            var combatFromU = from;
            // Log.Info("触发"+type.ToString()+to.Id+"  "+from.Id);
            // Log.Info("触发"+type.ToString()+to.Position+" Dis: "+Vector3.Distance(to.Position,from.Position));
            int formulaId = 0;//公式
            if (stepPara.Paras.Length > 1)
            {
                StepParaHelper.TryParseInt(ref stepPara.Paras[1], out formulaId);
            }
            float percent = 1;//实际伤害百分比
            if (stepPara.Paras.Length > 2)
            {
                StepParaHelper.TryParseFloat(ref stepPara.Paras[2], out percent);
            }

            int maxNum = 0;
            if (stepPara.Paras.Length > 3)
            {
                StepParaHelper.TryParseInt(ref stepPara.Paras[3], out maxNum);
            }

            if (maxNum != 0 && stepPara.Count >= maxNum) return;//超上限
            stepPara.Count++;
            
            List<int[]> buffInfo = null;//添加的buff
            if (stepPara.Paras.Length > 4)
            {
                buffInfo = stepPara.Paras[4] as List<int[]>;
                if (buffInfo == null)
                {
                    string[] vs = stepPara.Paras[4].ToString().Split(';');
                    buffInfo = new List<int[]>();
                    for (int i = 0; i < vs.Length; i++)
                    {
                        var data = vs[i].Split(',');
                        int[] temp = new int[data.Length];
                        for (int j = 0; j < data.Length; j++)
                        {
                            temp[j] = int.Parse(data[j]);
                        }
                        buffInfo.Add(temp);
                    }
                    stepPara.Paras[4] = buffInfo;
                }
            }
            
            if(buffInfo!=null&&buffInfo.Count>0)
            {
                var buffC = combatToU.GetComponent<BuffHolderComponent>();
                
                for (int i = 0; i < buffInfo.Count; i++)
                {
                    
                    buffC.AddBuff(buffInfo[i][0],TimeHelper.ServerNow() + buffInfo[i][1],combatFromU.Id);
                }
            }

            FormulaConfig formula = FormulaConfigCategory.Instance.Get(formulaId);
            if (formula!=null)
            {
                FormulaStringFx fx = FormulaStringFx.Get(formula.Formula);
                NumericComponent f = combatFromU.GetComponent<NumericComponent>();
                NumericComponent t = combatToU.GetComponent<NumericComponent>();
                float value = fx.GetData(f, t);
                BattleHelper.Damage(combatFromU,combatToU,value,hitPos);
            }
        }
        /// <summary>
        /// 离开触发器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stepPara"></param>
        /// <param name="costId"></param>
        /// <param name="cost"></param>
        /// <param name="config"></param>
        /// <param name="skill">技能判断体</param>
        public static void OnColliderOut(Vector3 hitPos,Unit from, Unit to, SkillStepPara stepPara, List<int> costId,
            List<int> cost,SkillConfig config,Skill skill = null)
        {
            // Log.Info("触发"+type.ToString()+to.Id+"  "+from.Id);
            // Log.Info("触发"+type.ToString()+to.Position+" Dis: "+Vector3.Distance(to.Position,from.Position));
            if (stepPara.Paras.Length > 4)
            {
                List<int[]> buffInfo = stepPara.Paras[4] as List<int[]>;
                if (buffInfo != null&&buffInfo.Count>0)
                {
                    var buffC = to.GetComponent<BuffHolderComponent>();
                    for (int i = 0; i < buffInfo.Count; i++)
                    {
                        if (buffInfo[i][2] == 1)
                        {
                            buffC.RemoveByConfigId(buffInfo[i][0]);
                        }
                    }
                }
            }
        }
    }
}