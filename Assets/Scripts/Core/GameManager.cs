using UnityEngine;

namespace ProjectAlpha.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 遊戲啟動預設進入編制選擇階段
            ChangeState(GameState.Deployment);
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            Debug.Log($"[GameManager] 切換狀態至: {CurrentState}");

            switch (CurrentState)
            {
                case GameState.Drafting:
                    HandleDraftingState();
                    break;
                case GameState.Deployment:
                    HandleDeploymentState();
                    break;
                case GameState.Playing:
                    HandlePlayingState();
                    break;
            }
        }

        private void HandleDraftingState()
        {
            // TODO: 載入 UI 讓玩家選擇 5 個 DivisionTemplate
        }

        private void HandleDeploymentState()
        {
            var mapGen = Object.FindFirstObjectByType<ProjectAlpha.Map.MapGenerator>();
            if (mapGen != null)
            {
                mapGen.GenerateMap();
            }
            else
            {
                Debug.LogError("[GameManager] 在場景中找不到 MapGenerator！請確保有物件掛載了該腳本。");
            }
            Time.timeScale = 0f; 
        }
        private void HandlePlayingState()
        {
            Time.timeScale = 1f;
            // TODO: 啟動 EconomyManager 等即時系統
        }
    }
}