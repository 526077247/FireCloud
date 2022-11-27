using System;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public class TriggerComponent: MonoBehaviour
    {
        public EntityType CastEntityType;
        public UnityAction<long,TriggerType> OnTriggerEnterEvt;
        public UnityAction<long> OnTriggerStayEvt;
        public UnityAction<long,TriggerType> OnTriggerExitEvt;
        private void OnTriggerEnter(Collider other)
        {
            var entity = other.GetComponentInParent<EntityComponent>();
            if (entity.EntityType == CastEntityType||CastEntityType == EntityType.ALL)
            {
                OnTriggerEnterEvt?.Invoke(entity.Id,TriggerType.Enter);
            }
           
        }
        private void OnTriggerStay(Collider other)
        {
            var entity = other.GetComponentInParent<EntityComponent>();
            if (entity.EntityType == CastEntityType||CastEntityType == EntityType.ALL)
            {
                OnTriggerStayEvt?.Invoke(entity.Id);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            var entity = other.GetComponentInParent<EntityComponent>();
            if (entity.EntityType == CastEntityType||CastEntityType == EntityType.ALL)
            {
                OnTriggerExitEvt?.Invoke(entity.Id,TriggerType.Exit);
            }
        }
    }
}