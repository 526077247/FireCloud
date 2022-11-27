namespace TaoTie
{
    /// <summary>
    /// 判断切换Group
    /// </summary>
    [SkillWatcher(SkillStepType.ChangeGroup)]
    public class SkillWatcher_ChangeGroup : ISkillWatcher
    {
        public void Run(SkillPara para)
        {

            if (para.GetCurSkillStepPara().Paras.Length == 0)
            {
                Log.Error(para.Ability.ConfigId+"判断切换Group参数数量不对"+para.GetCurSkillStepPara().Paras.Length);
                return;
            }
            
            var stepPara = para.GetCurSkillStepPara();
            var unit = para.From;
            var spell = unit.GetComponent<SpellComponent>();
            if (stepPara.Paras.Length == 1)
            {
                if(StepParaHelper.TryParseString(ref stepPara.Paras[0], out var group))
                {
                    spell.ChangeGroup(group);
                }
            }
            else if (stepPara.Paras.Length >= 2)
            {
                if(StepParaHelper.TryParseString(ref stepPara.Paras[0], out var condition))
                {
                    var res = SkillSystem.Instance.CheckCondition(condition,para);
                    StepParaHelper.TryParseString(ref stepPara.Paras[1], out var suc);
                    if (res)
                    {
                        spell.ChangeGroup(suc);
                    }
                    else if(stepPara.Paras.Length >= 3)
                    {
                        StepParaHelper.TryParseString(ref stepPara.Paras[2], out var fail);
                        spell.ChangeGroup(fail);
                    }
                }

                // spell.WaitStep(SkillStepType.ChangeGroup); //如果需要等服务端消息走这里

            }

        }
    }
}