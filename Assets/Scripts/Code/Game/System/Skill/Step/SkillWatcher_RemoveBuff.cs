namespace TaoTie
{
    /// <summary>
    /// 移除自己BUFF
    /// </summary>
    [SkillWatcher(SkillStepType.RemoveBuff)]
    public class SkillWatcher_RemoveBuff : ISkillWatcher
    {
        public void Run(SkillPara para)
        {

            var unit = para.From;
            var stepPara = para.GetCurSkillStepPara();
            Log.Info("SkillWatcher_RemoveBuff");
            if (stepPara.Paras.Length >= 1)
            {
                if (StepParaHelper.TryParseInt(ref stepPara.Paras[0], out var buffId))
                {
                    var bc = unit.GetComponent<BuffHolderComponent>();
                    if (bc != null)
                    {
                        bc.Remove(buffId, true);
                    }
                }
            }
        }
    }
}