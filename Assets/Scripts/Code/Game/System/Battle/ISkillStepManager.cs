using System.Collections.Generic;

namespace TaoTie
{
    public interface ISkillStepManager
    {
        public List<int> GetTimeLine(int skillId, string group);

        public List<int> GetStepType(int skillId, string group);

        public List<object[]> GetParas(int skillId, string group);
    }
}