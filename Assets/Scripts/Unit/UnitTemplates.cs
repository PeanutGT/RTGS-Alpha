using System.Collections.Generic;
using UnityEngine;

namespace ProjectAlpha.Unit
{
    [CreateAssetMenu(fileName = "NewSubUnit", menuName = "ProjectAlpha/SubUnit Template")]
    public class SubUnitTemplate : ScriptableObject
    {
        public string SubUnitName;
        public float MaxHP = 10f;
        public float SoftAttack = 2f;
        public float Defense = 3f;
        public float Speed = 5f;
        public int Cost = 2; 
    }

    [System.Serializable]
    public class DivisionTemplate
    {
        public string DivisionName;
        public Sprite CounterSprite;
        public List<SubUnitTemplate> SubUnits = new List<SubUnitTemplate>();

        public float GetMaxHP() 
        { 
            float sum = 0; 
            foreach(var u in SubUnits) sum += u.MaxHP; 
            return sum; 
        }

        public int GetProductionCost()
        {
            int sum = 0;
            foreach(var u in SubUnits) sum += u.Cost;
            return sum;
        }
    }
}