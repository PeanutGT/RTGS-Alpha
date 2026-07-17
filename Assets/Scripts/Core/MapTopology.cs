using System.Collections.Generic;
using UnityEngine;

namespace ProjectAlpha.Core
{
    // 預留陣營類別，後續再實作細節
    public class Faction
    {
        public int FactionID;
        public string FactionName;
        public int DeploymentBudget;
    }

    public class Border
    {
        public Province NodeA;
        public Province NodeB;
        public BorderType Type;
        public bool HasRailroad;

        public Border(Province a, Province b)
        {
            NodeA = a;
            NodeB = b;
            Type = BorderType.Normal;
            HasRailroad = false;
        }

        // 取得對向節點的輔助方法
        public Province GetOppositeNode(Province current)
        {
            if (current == NodeA) return NodeB;
            if (current == NodeB) return NodeA;
            return null;
        }
    }

    public class Province
    {
        public int ID;
        public List<Vector2> Vertices;
        public Vector2 Center;
        
        // 靜態屬性
        public TerrainType Terrain;
        public int BaseIC;
        public int InfrastructureLevel;
        
        // 動態狀態
        public Faction Owner;
        public float CurrentSupplyLevel;
        // --- 新增這行：記錄駐紮在此的軍隊 ---
        public List<ProjectAlpha.Unit.UnitController> StationedUnits = new List<ProjectAlpha.Unit.UnitController>();
        // 圖論關聯：相鄰的邊界
        public List<Border> Borders;

        public Province(int id)
        {
            ID = id;
            Vertices = new List<Vector2>();
            Borders = new List<Border>();
            BaseIC = 1;
            InfrastructureLevel = 0;
            CurrentSupplyLevel = 0f;
        }
    }
}