using CodeBase.Utils;
using System;
using UnityEngine;

namespace CodeBase.Player
{
    [Serializable]
    public class Player
    {
        [field: Header("Player Settings")]
        [field: SerializeField] public bool IsDead { get; private set; } = false;
        [field: SerializeField] public float Health { get; private set; }
        [field: SerializeField] public float CurrentHealth { get; private set; }
        [field: SerializeField] public float Score { get; private set; } = 0f;
        [field: SerializeField] public float MovementSpeed { get; private set; }
        [field: SerializeField] public Vector3 DefaultPlayerPosition { get; private set; }

        [field: Header("Default Values")]
        [field: SerializeField] public float DefaultHealth { get; private set; }
        [field: SerializeField] public float DefaultMovementSpeed { get; private set; }

        private const float MAX_HEALTH = 10f;

        public void SetPlayerData(float health, float movementSpeed, Vector3 defaultPlayerPosition)
        {
            Health = health;
            MovementSpeed = movementSpeed;
            DefaultPlayerPosition = defaultPlayerPosition;
            CurrentHealth = Health;
            DefaultHealth = Health;
            DefaultMovementSpeed = MovementSpeed;
        }

        public void ModifyHealth(float health)
        {
            CurrentHealth += health;
            if (CurrentHealth <= 0f)
            {
                IsDead = true;
                CurrentHealth = 0f;
            }

            EventObserver.OnHealthChanged?.Invoke();
        }

        public void ModifyScore(float score)
        {
            Score += score;
            EventObserver.OnScoreChanged?.Invoke();
        }

        public void RevivePlayer()
        {
            ModifyHealth(Health);
            IsDead = false;
        }

        public void StartNewGame()
        {
            IsDead = false;
            ModifyHealth(DefaultHealth);
            ModifyScore(-Score);
            MovementSpeed = DefaultMovementSpeed;
        }
    }
}
