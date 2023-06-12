namespace TaoTie
{
    /// <summary>
    /// 播动画(aoi创建Unit之前放的技能没播的就算了)
    /// </summary>
    [SkillWatcher(SkillStepType.Anim)]
    public class SkillWatcher_Anim : ISkillWatcher
    {
        public void Run(SkillPara para)
        {
            var unit = para.From;
        }
        
    }
}
