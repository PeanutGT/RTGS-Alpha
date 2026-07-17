using UnityEngine;
using UnityEngine.InputSystem;
using ProjectAlpha.Map;
using ProjectAlpha.UI; // 引入 UI 命名空間

namespace ProjectAlpha.Core
{
    public class SelectionManager : MonoBehaviour
    {
        public FactionManager factionManager;
        public ContextPanelUI contextPanel; // UI 面板參照

        private void Update()
        {
            if (Mouse.current == null) return;

            bool isOverUI = UnityEngine.EventSystems.EventSystem.current != null && 
                            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            // 左鍵選取地塊
            if (Mouse.current.leftButton.wasPressedThisFrame && !isOverUI)
            {
                HandleClick(false);
            }
            // 右鍵部署 (僅限 Deployment 狀態)
            if (Mouse.current.rightButton.wasPressedThisFrame && !isOverUI)
            {
                if (GameManager.Instance.CurrentState == GameState.Deployment)
                {
                    HandleClick(true);
                }
            }
        }

        private void HandleClick(bool isRightClick)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                ProvinceComponent provinceComp = hit.collider.GetComponent<ProvinceComponent>();
                if (provinceComp != null)
                {
                    if (isRightClick)
                    {
                        // 執行部署邏輯
                        factionManager.TryDeployUnit(provinceComp.ProvinceData);
                        
                        // 部署完單位後，如果當前 UI 正在顯示這個地塊，就立即刷新它
                        if (contextPanel != null && contextPanel.panelRoot.activeSelf)
                        {
                            contextPanel.ShowProvinceInfo(provinceComp.ProvinceData);
                        }
                    }
                    else
                    {
                        // 左鍵點擊：呼叫 UI 面板顯示資訊
                        if (contextPanel != null)
                        {
                            contextPanel.ShowProvinceInfo(provinceComp.ProvinceData);
                        }
                    }
                }
            }
            else
            {
                // 如果點擊到沒有地塊的空白處，就隱藏面板
                if (contextPanel != null && !isRightClick)
                {
                    contextPanel.HidePanel();
                }
            }
        }
    }
}