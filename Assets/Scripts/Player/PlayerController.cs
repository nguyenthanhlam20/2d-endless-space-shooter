﻿using CodeBase.Animation;
using CodeBase.Effects;
using CodeBase.Mobs;
using CodeBase.ObjectBased;
using CodeBase.UI;
using CodeBase.Utils;
using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;
using static CodeBase.Utils.Enums;

namespace CodeBase.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region Variables
        [Header("Storages")]
        [SerializeField] private PlayerStorage playerStorage;
        [SerializeField] private EnemyStorage enemyStorage;

        [Header("Shields")]
        [SerializeField] private Shield electroShield;
        [field: SerializeField] public float ElectroShieldActiveDuration { get; private set; }

        [Header("Components")]
        [SerializeField] private PlayerAnimationController playerAnimationController;
        [SerializeField] private WeaponController weaponController;

        [Space]
        [SerializeField] private PopUp popUp;
        [SerializeField] private GameObject body;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private ParticleType explosionEffect;
        [SerializeField] private float explosionAdditionalScale;
        [SerializeField] private Rigidbody2D playerBody;
        [SerializeField] private SpriteRenderer skinRenderer;
        [SerializeField] private float forceOnEnemyCollision;
        [SerializeField] private float minPercentOfHealthToBlink;
        [SerializeField] private Color playerHitColor = Color.red;

        private Color defaultColor;
        private Coroutine gameOverCoroutine;
        private Sequence playerCollisionBehaviour;
        private bool isChangedColor;
        private ParticlePool particlePool;
        private TouchController touchController;
        private UserInterface userInterface;
        #endregion

        [Inject]
        private void Construct(ParticlePool pool, TouchController touch, UserInterface ui)
        {
            particlePool = pool;
            touchController = touch;
            userInterface = ui;
        }

        private void OnEnable()
        {
            EventObserver.OnLevelLoaded += EnableTouchControls;
            EventObserver.OnGameRestarted += StartNewGame;
            EventObserver.OnPlayerCollision += ForceBackPlayer;
        }

        private void OnDisable()
        {
            EventObserver.OnLevelLoaded -= EnableTouchControls;
            EventObserver.OnGameRestarted -= StartNewGame;
            EventObserver.OnPlayerCollision -= ForceBackPlayer;
        }

        private void Start()
        {
            defaultColor = skinRenderer.color;
            transform.position = playerStorage.PlayerData.DefaultPlayerPosition;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag.Equals(Tags.EnemyProjectile) && !electroShield.IsActive)
            {
                var projectile = Dictionaries.EnemyProjectiles.FirstOrDefault(p => p.Key == collision.gameObject.transform);
                playerStorage.PlayerData.ModifyHealth(-projectile.Value.Damage);

                SpawnSpark(collision.gameObject.transform.position);
                CheckBehaviourDueToDamageTaken();
            }
        }

        private void EnableTouchControls() => touchController.enabled = true;

        private void CheckBehaviourDueToDamageTaken()
        {
            var minHealthEdge = (playerStorage.PlayerData.Health / 100f) * minPercentOfHealthToBlink;
            if (playerStorage.PlayerData.CurrentHealth <= minHealthEdge && playerStorage.PlayerData.CurrentHealth > 0f)
            {
                playerAnimationController.EnableCriticalDamageVisual(true);
                popUp.Spawn(transform, "danger", Color.red);
            }

            if (playerStorage.PlayerData.IsDead)
            {
                DestroyPlayer();
                EventObserver.OnPlayerDied?.Invoke();
                gameOverCoroutine = StartCoroutine(GameOver());
            }

            ChangeBodyColor();

            EventObserver.OnShakeCamera?.Invoke(0.2f, 0.25f);
            EventObserver.OnPlayerHit?.Invoke();
        }

        private void ChangeBodyColor()
        {
            if (!isChangedColor)
            {
                isChangedColor = true;

                playerCollisionBehaviour = DOTween.Sequence().SetAutoKill(true);
                playerCollisionBehaviour.Append(skinRenderer.DOColor(playerHitColor, 0.1f))
                                        .Append(skinRenderer.DOColor(defaultColor, 0.1f))
                                        .OnComplete(() => isChangedColor = false);
            }
        }

        private IEnumerator GameOver()
        {
            DestroyPlayer();
            yield return new WaitForSeconds(1.5f);
            EventObserver.OnGameOver?.Invoke();
            gameOverCoroutine = null;
        }

        private void SpawnSpark(Vector3 projectilePosition)
        {
            var newEffect = particlePool.GetFreeObject(ParticleType.SparkHit);
            newEffect.gameObject.SetActive(false);
            newEffect.transform.position = new Vector3(projectilePosition.x, projectilePosition.y - 1f, projectilePosition.z);
            newEffect.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            newEffect.SetBusyState(true);
        }

        private void ForceBackPlayer(Vector3 asteroidPosition)
        {
            var force = asteroidPosition - transform.position;
            playerBody.AddForce(-force.normalized * (forceOnEnemyCollision * 100f));
            playerStorage.PlayerData.ModifyHealth(-enemyStorage.DamageOnCollision);

            CheckBehaviourDueToDamageTaken();
        }

        private void DestroyPlayer()
        {
            touchController.enabled = false;
            weaponController.StartShooting(false);
            playerAnimationController.EnableCriticalDamageVisual(false);
            playerAnimationController.EnableStarterFlames(false);

            var newEffect = particlePool.GetFreeObject(explosionEffect);
            newEffect.gameObject.SetActive(false);
            newEffect.transform.position = transform.position;
            newEffect.transform.localScale = new Vector3(transform.localScale.x + explosionAdditionalScale, transform.localScale.y + explosionAdditionalScale, 1f);
            newEffect.SetBusyState(true);

            body.SetActive(false);
            playerCollider.enabled = false;
        }

        private void StartNewGame()
        {
            DestroyPlayer();
            transform.position = playerStorage.PlayerData.DefaultPlayerPosition;
            electroShield.gameObject.SetActive(true);
            touchController.enabled = true;
            body.SetActive(true);
            playerCollider.enabled = true;

            playerAnimationController.EnableStarterFlames(true);
            playerStorage.PlayerData.StartNewGame();
        }
    }
}
