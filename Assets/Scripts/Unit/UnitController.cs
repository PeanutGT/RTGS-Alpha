using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空間
using ProjectAlpha.Map;
using ProjectAlpha.Core;

namespace ProjectAlpha.Unit
{
    public class UnitController : MonoBehaviour
    {
        public DivisionTemplate Template;
        public Province CurrentLocation;
        public Faction Owner;
        public float CurrentHP;

        [Header("Visual Components (兵牌 UI)")]
        public GameObject visualRoot;        // 控制整個兵牌顯示/隱藏的根節點
        public Image backgroundPanel;        // 兵牌底色 (依陣營區分)
        public Image natoIcon;               // 北約兵種符號
        public Image hpBarFill;              // 血量條
        public TextMeshProUGUI badgeText;    // 右下角的堆疊數字徽章

        public void Initialize(DivisionTemplate template, Province startProvince, Faction owner)
        {
            Template = template;
            CurrentLocation = startProvince;
            Owner = owner;
            CurrentHP = template.GetMaxHP();

            // 確保 Z 軸為 -1，讓兵牌顯示在地圖上方
            transform.position = new Vector3(startProvince.Center.x, startProvince.Center.y, -1f);
            // --- 新增這段：動態讀取圖片 ---
            if (Template.CounterSprite != null && natoIcon != null)
            {
                natoIcon.sprite = Template.CounterSprite; // 替換為你在資料庫設定的圖片
            }
            // 處理堆疊與視覺邏輯
            HandleStacking();
        }

        // 處理多單位堆疊時的顯示邏輯
        public void HandleStacking()
        {
            // 將自己加入該地塊的駐軍名單
            if (!CurrentLocation.StationedUnits.Contains(this))
            {
                CurrentLocation.StationedUnits.Add(this);
            }

            var stationed = CurrentLocation.StationedUnits;

            // 如果自己是該地塊的第一個單位 (代表單位)
            if (stationed[0] == this)
            {
                visualRoot.SetActive(true);
                // 只有當堆疊數量大於 1 時，才顯示數字徽章
                badgeText.text = stationed.Count > 1 ? stationed.Count.ToString() : "";
                
                // TODO: 根據 Template 設定 natoIcon 的圖片 (目前暫用預設)
                // hpBarFill.fillAmount = CurrentHP / Template.GetMaxHP();
            }
            else
            {
                // 如果自己不是第一個單位，隱藏自己的畫面實體，以節省效能並避免畫面混亂
                visualRoot.SetActive(false);
                
                // 更新「代表單位」的數字徽章
                stationed[0].badgeText.text = stationed.Count.ToString();
            }
        }

        // 當單位離開或被消滅時呼叫
        public void RemoveFromStack()
        {
            CurrentLocation.StationedUnits.Remove(this);
            if (CurrentLocation.StationedUnits.Count > 0)
            {
                // 重新推舉新的代表單位並更新徽章
                CurrentLocation.StationedUnits[0].HandleStacking();
            }
            Destroy(gameObject);
        }
    }
}