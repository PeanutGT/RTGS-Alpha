using UnityEngine;
using TMPro; // 使用 TextMeshPro
using ProjectAlpha.Core;
using System.Text;

namespace ProjectAlpha.UI
{
    public class ContextPanelUI : MonoBehaviour
    {
        [Header("UI 參照")]
        public GameObject panelRoot;          // 面板的根節點 (用來控制顯示/隱藏)
        public TextMeshProUGUI provinceInfoText; // 顯示地塊 ID、地形與產值
        public TextMeshProUGUI unitListText;     // 顯示駐紮軍隊清單

        private void Start()
        {
            // 遊戲開始時預設隱藏面板
            HidePanel();
        }

        public void ShowProvinceInfo(Province province)
        {
            panelRoot.SetActive(true);

            // 1. 更新地塊基本資訊
            provinceInfoText.text = $"地塊 ID: {province.ID}\n" +
                                    $"地形: {province.Terrain}\n" +
                                    $"基礎產值 (IC): {province.BaseIC}";

            // 2. 利用 StringBuilder 組合駐紮部隊清單
            StringBuilder sb = new StringBuilder();
            if (province.StationedUnits.Count > 0)
            {
                sb.AppendLine("<color=#FFD700>駐紮部隊：</color>"); // 黃色標題
                for (int i = 0; i < province.StationedUnits.Count; i++)
                {
                    var unit = province.StationedUnits[i];
                    // 顯示部隊名稱與當前血量
                    sb.AppendLine($"- {unit.Template.DivisionName} (HP: {unit.CurrentHP}/{unit.Template.GetMaxHP()})");
                }
            }
            else
            {
                sb.AppendLine("<color=#AAAAAA>無駐紮部隊</color>"); // 灰色提示
            }
            
            unitListText.text = sb.ToString();
        }

        public void HidePanel()
        {
            panelRoot.SetActive(false);
        }
    }
}