using UnityEngine;

namespace CodeBase.Player
{
    [CreateAssetMenu(fileName = "PlayerStorage", menuName = "ScriptableObjects/PlayerStorage")]
    public class PlayerStorage : ScriptableObject
    {
        [SerializeField] private string playerPrefsSaveString = "_playerSave";
        [SerializeField] private PlayerDataStorage playerDataStorage;

        [Header("ConcretePlayer")]
        [SerializeField] private Player playerData = new ();

        public Player PlayerData => playerData;

        public void SavePlayer()
        {
            Debug.Log("SAVED");
            PlayerPrefs.SetString(playerPrefsSaveString, JsonUtility.ToJson(playerData));
        }

        public void LoadPlayer()
        {
            var playerString = PlayerPrefs.GetString(playerPrefsSaveString, "");
            if (playerString != "")
            {
                Debug.Log("Load Player Data");
                playerData = JsonUtility.FromJson<Player>(playerString);
            }
            else
            {
                Debug.Log("Start Game");
                playerData = new Player();
                InitPlayer();
            }
        }

        private void InitPlayer()
        {
            playerData.SetPlayerData(playerDataStorage.DefaultHealth,
                                     playerDataStorage.DefaultMovementSpeed,
                                     playerDataStorage.DefaultPlayerPosition);
        }
    }
}
