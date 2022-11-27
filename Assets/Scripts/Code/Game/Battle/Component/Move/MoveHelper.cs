using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public static class MoveHelper
    {
        // 可以多次调用，多次调用的话会取消上一次的协程
        public static async ETTask<int> MoveToAsync(this Unit unit, Vector3 targetPos, ETCancellationToken cancellationToken = null)
        {
            if (!unit.GetComponent<MoveComponent>().Enable)
            {
                Log.Error("暂时无法移动");
                return 1;
            }
            float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
            if (speed < 0.01)
            {
                return 2;
            }

            List<Vector3> path = new List<Vector3>() {unit.Position, targetPos};
            await unit.GetComponent<MoveComponent>().MoveToAsync(path,speed);
            return 0;
        }
        
        public static async ETTask<bool> MoveToAsync(this Unit unit, List<Vector3> path)
        {
            float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
            bool ret = await moveComponent.MoveToAsync(path, speed);
            return ret;
        }

        public static void Stop(this Unit unit, int error)
        {

        }
    }
}