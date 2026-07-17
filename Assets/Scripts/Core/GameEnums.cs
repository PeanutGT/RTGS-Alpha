namespace ProjectAlpha.Core
{
    public enum GameState
    {
        Drafting,   // 編制選擇階段
        Deployment, // 部署階段 (暫停)
        Playing     // 即時戰鬥階段
    }

    public enum TerrainType
    {
        Plain,      // 平原
        Hill,       // 丘陵
        Mountain,   // 山地
        Peak,       // 山峰 (不可通行)
        Swamp       // 沼地
    }

    public enum BorderType
    {
        Normal,     // 一般邊界
        River,      // 河流 (跨河攻擊懲罰)
        Ocean       // 海洋 (不可通行)
    }

    public enum UnitState
    {
        Idle,
        Moving,
        Engaging,
        Retreating,
        Restocking
    }
}