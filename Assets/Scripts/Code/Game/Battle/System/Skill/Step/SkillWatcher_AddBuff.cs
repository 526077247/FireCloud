namespace TaoTie
{
    /// <summary>
    /// 给自己加BUFF
    /// </summary>
    [SkillWatcher(SkillStepType.AddBuff)]
    public class SkillWatcher_AddBuff : ISkillWatcher
    {
        public void Run(SkillPara para)
        {
            var unit = para.From;
            var stepPara = para.GetCurSkillStepPara();
            Log.Info("SkillWatcher_AddBuff");
            if (stepPara.Paras.Length >= 2)
            {
                if (StepParaHelper.TryParseInt(ref stepPara.Paras[0], out var buffId))
                {
                    StepParaHelper.TryParseInt(ref stepPara.Paras[1], out var time);
                    var bc = unit.GetComponent<BuffHolderComponent>();
                    if (bc != null)
                    {
                        bc.AddBuff(buffId, TimeHelper.ServerNow() + time, unit.Id);
                    }
                }
            }

        }
        
    }
}