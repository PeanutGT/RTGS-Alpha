using System.Collections.Generic;
using UnityEngine;
using ProjectAlpha.Unit;
using ProjectAlpha.Map;

namespace ProjectAlpha.Core
{
    public class FactionManager : MonoBehaviour
    {
        public Faction PlayerFaction;
        public GameObject UnitPrefab; 

        [Header("設計局資料庫")]
        public List<SubUnitTemplate> AvailableSubUnits = new List<SubUnitTemplate>();
        public List<DivisionTemplate> UnlockedDivisions = new List<DivisionTemplate>();
        
        public int SelectedDivisionIndex = 0;

        private void Awake()
        {
            PlayerFaction = new Faction { FactionID = 1, FactionName = "Player", DeploymentBudget = 100 };
        }

        private void Start()
        {
            CreateDefaultDivision();
        }

        private void CreateDefaultDivision()
        {
            DivisionTemplate defaultDiv = new DivisionTemplate();
            defaultDiv.DivisionName = "預設混合師";
            
            if (AvailableSubUnits.Count > 0)
            {
                defaultDiv.SubUnits.Add(AvailableSubUnits[0]);
                defaultDiv.SubUnits.Add(AvailableSubUnits[0]);
            }
            
            UnlockedDivisions.Add(defaultDiv);
        }

        public bool TryDeployUnit(Province targetProvince)
        {
            if (UnlockedDivisions.Count == 0) return false;

            DivisionTemplate templateToDeploy = UnlockedDivisions[SelectedDivisionIndex];
            int cost = templateToDeploy.GetProductionCost();

            if (PlayerFaction.DeploymentBudget >= cost)
            {
                PlayerFaction.DeploymentBudget -= cost;
                
                GameObject unitGo = Instantiate(UnitPrefab);
                UnitController unit = unitGo.GetComponent<UnitController>();
                unit.Initialize(templateToDeploy, targetProvince, PlayerFaction);
                
                Debug.Log($"[FactionManager] 部署成功！花費 {cost} 點，剩餘點數: {PlayerFaction.DeploymentBudget}");
                return true;
            }
            
            Debug.LogWarning("[FactionManager] 部署點數不足！");
            return false;
        }
    }
}