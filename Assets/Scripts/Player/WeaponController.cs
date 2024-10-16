using CodeBase.Effects;
using CodeBase.ObjectBased;
using CodeBase.Utils;
using System;
using System.Collections;
using UnityEngine;
using Zenject;
using static CodeBase.Utils.Enums;

namespace CodeBase.Player
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private Projectile blaster;
        [SerializeField] private float blasterDamage;
        [SerializeField] private WeaponSettings[] _weaponSettings;
        [Inject] private DiContainer diContainer;
        public WeaponSettings[] WeaponSettings => _weaponSettings;

        //private PlayerController _playerInput;
        private Rigidbody2D _playerBody;
        private Transform _weaponShootingPoint;
        private bool _allWeaponMaxOut;
        private Coroutine shootingRoutine;
        private ParticlePool particlePool;

        [Inject]
        private void Construct(ParticlePool pool)
        {
            particlePool = pool;
        }

        public bool AllWeaponMaxOut => _allWeaponMaxOut;

        private void Awake()
        {
            foreach (var weapon in _weaponSettings)
            {
                weapon.DefaultAmmo = weapon.Ammo;
            }

            _weaponShootingPoint = _weaponSettings[0].WeaponShootingPoint;
        }

        private void OnEnable()
        {
            EventObserver.OnStartMoving += StartShooting;
        }

        private void OnDisable()
        {
            EventObserver.OnStartMoving -= StartShooting;
        }

        private void Start()
        {
            foreach (var weapon in _weaponSettings)
            {
                weapon.Ammo = weapon.DefaultAmmo;
            }

            _playerBody = GetComponent<Rigidbody2D>();
        }

        public void StartShooting(bool isMoving)
        {
            if (isMoving)
            {
                shootingRoutine = StartCoroutine(EndlessShooting());
            }
            else
            {
                if (shootingRoutine != null)
                    StopCoroutine(shootingRoutine);
            }
        }

        private IEnumerator EndlessShooting()
        {
            while (true)
            {
                Shoot();
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void Shoot()
        {
            var projectile = GetFreeProjectile();
            projectile.SetBusyState(true);
            projectile.transform.position = _weaponShootingPoint.position;
            projectile.transform.rotation = transform.rotation;
            projectile.Launch(_playerBody.velocity, transform.up);
        }

        public void KillAllEnemies()
        {
            var projectiles = FindObjectsOfType<Projectile>();
            foreach (var projectile in projectiles)
            {
                Destroy(projectile.gameObject);
            }
        }

        public Projectile GetFreeProjectile()
        {
            Projectile freeProjectile = particlePool.ProjectilesPool.Find(projectile => !projectile.IsBusy && projectile.WeaponType == blaster.WeaponType);
            if (freeProjectile == null)
                freeProjectile = CreateNewProjectile();

            Debug.Log(freeProjectile);
            return freeProjectile;
        }

        private Projectile CreateNewProjectile()
        {
            Projectile newProjectile = diContainer.InstantiatePrefabForComponent<Projectile>(blaster, particlePool.ProjectileContainer);
            particlePool.ProjectilesPool.Add(newProjectile);
            Dictionaries.Projectiles.Add(newProjectile.transform, newProjectile);
            newProjectile.SetDamage(blasterDamage);

            return newProjectile;
        }
    }

    [Serializable]
    public class WeaponSettings
    {
        [SerializeField] private string _name;
        [SerializeField] private Projectile _weapon;
        [SerializeField] private Transform _weaponShootingPoint;

        [Space]
        [SerializeField] private int _ammo;
        [SerializeField] private int _maxAmmo;
        [SerializeField] private int _ammoToReload;
        [SerializeField] private int _ammoPerShoot;

        [Space]
        [SerializeField] private float _shootingDelayMin;

        public string Name => _name;
        public Projectile Weapon => _weapon;
        public Transform WeaponShootingPoint => _weaponShootingPoint;

        public int Ammo
        {
            get => _ammo;
            set => _ammo = value;
        }

        public int MaxAmmo => _maxAmmo;
        public int DefaultAmmo { get; set; }
        public int AmmoToReload => _ammoToReload;
        public int AmmoPerShoot => _ammoPerShoot;
        public float ShootingDelayMin => _shootingDelayMin;
    }

    public class WeaponData
    {
        [field: SerializeField] public PlayerWeaponType Type { get; private set; }
        [field: SerializeField] public Projectile Weapon { get; private set; }
    }
}
