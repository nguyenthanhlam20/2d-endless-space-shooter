using CodeBase.Player;
using UnityEngine;

namespace CodeBase.Service
{
    public class GameSession : MonoBehaviour
    {
        [Header("Storages")]
        [SerializeField] private PlayerStorage playerStorage;

        [SerializeField] private int _tries;

        [Space]
        [SerializeField] private float _minEnemySpawnCooldown;
        [SerializeField] private EnemySpawner[] _enemySpawners;

        [Space]
        [SerializeField] private int _everyLevelSpawn;

        private int _score;
        private int _xp;
        private int _playerLvl = 1;
        private int _nextLvl = 500;
        private PlayerController _player;
        private WeaponController _weaponController;
        //private HealthComponent _playerHealth;

        public int Tries => _tries;
        //public HealthComponent PlayerHealth => _playerHealth;
        public int Score => _score;
        public int XP => _xp;
        public int PlayerLVL => _playerLvl;
        public int NextLvl => _nextLvl;

        private void Awake()
        {
            _player = FindObjectOfType<PlayerController>();
            _weaponController = FindObjectOfType<WeaponController>();
            //_playerHealth = _player.GetComponent<HealthComponent>();
        }

        public void ModifyXp(int xp)
        {
            _score += xp;
            _xp += xp;
        }

        public void ModifyTries(int tries)
        {
            _tries += tries;
            if (_tries < 0)
                _tries = 0;
        }

        private void Update()
        {
            if (playerStorage.PlayerData.IsDead) return;

            if (_xp == _nextLvl)
            {
                LevelUp();
            }
            else if (_xp > _nextLvl)
            {
                LevelUp(_xp - _nextLvl);
            }
        }

        private void LevelUp(int currentXp = 0)
        {
            if (!_weaponController)
                _weaponController = FindObjectOfType<WeaponController>();

            if (!_player)
                _player = FindObjectOfType<PlayerController>();

            _playerLvl++;
            _xp = currentXp;

            if (_playerLvl % _everyLevelSpawn == 0)
            {
                _weaponController.KillAllEnemies();
         
            }

            _nextLvl = (((_nextLvl / 100) * 20) + _nextLvl);
        }
    }
}
