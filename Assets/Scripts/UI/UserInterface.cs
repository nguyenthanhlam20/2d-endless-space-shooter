using CodeBase.Player;
using CodeBase.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class UserInterface : MonoBehaviour
    {
        #region Variables
        [Header("Storages")]
        [SerializeField] private PlayerStorage playerStorage;

        [Header("UI")]
        [SerializeField] private Image healthValue;
        [SerializeField] private TextMeshProUGUI scoreValue;
        [SerializeField] private Vector3 scoreValueScaleOnChange;

        [Header("Game Over Screen")]
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private TextMeshProUGUI finalScoreValue;
        [SerializeField] private Button replayBttn;
        [SerializeField] private Button exitGameBttn;

        private bool scoreIsScaling;
        #endregion

        private void OnEnable()
        {
            EventObserver.OnHealthChanged += RefreshHealthInfo;
            EventObserver.OnScoreChanged += RefreshScoreInfo;
            EventObserver.OnGameOver += ShowGameOverScreen;

            replayBttn.onClick.AddListener(ReplayButtonPressed);
        }

        private void OnDisable()
        {
            EventObserver.OnHealthChanged -= RefreshHealthInfo;
            EventObserver.OnScoreChanged -= RefreshScoreInfo;
            EventObserver.OnGameOver -= ShowGameOverScreen;

            replayBttn.onClick.RemoveListener(ReplayButtonPressed);
        }

        private void Start() => Loading();

        private void ReplayButtonPressed()
        {
            EventObserver.OnGameRestarted?.Invoke();

            gameOverScreen.SetActive(false);
            Time.timeScale = 1f;
            Loading();
        }


        private void Loading()
        {
            RefreshHealthInfo();
            RefreshScoreInfo();
            EventObserver.OnLevelLoaded?.Invoke();
        }

        private void ShowGameOverScreen()
        {
            gameOverScreen.SetActive(true);
            Time.timeScale = 0f;
            finalScoreValue.text = $"final score: {Mathf.Round(playerStorage.PlayerData.Score)}";
        }

        private void RefreshHealthInfo() 
            => healthValue.fillAmount = playerStorage.PlayerData.CurrentHealth / playerStorage.PlayerData.DefaultHealth;

        private void RefreshScoreInfo()
        {
            scoreValue.text = $"score {Mathf.Round(playerStorage.PlayerData.Score)}";

            if (!scoreIsScaling)
            {
                scoreIsScaling = true;
                scoreValue.transform.DOPunchScale(scoreValueScaleOnChange, 0.25f)
                                    .OnComplete(() => scoreIsScaling = false);
            }
        }
    }
}
