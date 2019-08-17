using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Data;
using Rhythm.Commands;
using Rhythm.Items;
using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.Tools;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

namespace Rhythm.Units {
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(NavMeshAgent))]
	public class Unit : MonoBehaviour {
		private static int ids;
#pragma warning disable 0649
        [SerializeField] private OwnerType owner = OwnerType.NONE;
        [SerializeField] private UnitData startupUnitData;
#pragma warning restore 0649
        public OwnerType Owner => owner;
		public float MovementSpeed { get; private set;}
        public float Health { get; private set; }
        public float Range => _range;
        public bool IsDying { get; private set; }
        public bool IsVisible => _renderers.Any(renderer => renderer.isVisible);
        public NavMeshAgent Agent => _navMeshAgent;

        private int _unitId;
		private string _name;
        private int _xp;
        private int _xpReward;
        private float _range;
        private float _damage;
        private bool _wasVisible;

        private UnityAction _updateFunc;
        private SpriteRenderer[] _renderers;
        private NavMeshAgent _navMeshAgent;
        private List<ItemDeposit> _depositsInSight;
        private List<Unit> _enemiesInSight;
        private CommandData[] _commandData;
		private ToolData _toolData;
        private WeaponData _weaponData;
        private BeatInputService _beatInputService;
		private GameStateService _gameStateService;
        private UnitService _unitService;

		private void Awake() {
			_updateFunc = Constants.Noop;
			_depositsInSight = new List<ItemDeposit>();
            _enemiesInSight = new List<Unit>();
            _unitService = ServiceLocator.Get<UnitService>();
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start() {
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            _navMeshAgent.updateUpAxis = false;
            _navMeshAgent.updateRotation = false;
            if (startupUnitData) {
                Initialize(startupUnitData);
                _unitService.AddUnit(this);
            }
        }

        public void Initialize(UnitData unitData) {

            _name = unitData.name;
            Health = unitData.health;
            MovementSpeed = unitData.movementSpeed;
            if (owner == OwnerType.NONE) {
                owner = OwnerType.PLAYER;
            }
            _unitId = ids++;
            _toolData = unitData.toolData;
            _weaponData = unitData.weaponData;
            InitializeWeapon(unitData.weaponData);
            InitializeCommands(unitData);

            _unitService.UnitAppeared += AddVisibleEnemy;
            _unitService.UnitDisappeared += RemoveVisibleEnemy;
            _unitService.UnitDying += RemoveVisibleEnemy;
            _beatInputService.BeatLost += BeatLost;
            _beatInputService.ExecutionFinishing += ExecutionFinishing;
            _gameStateService.GameFinishing += OnGameFinishing;
        }

        private void InitializeWeapon(WeaponData weaponData) {
            _damage = weaponData.damage;
            _range = weaponData.attackType == Weapons.AttackType.Melee ? .5f : 5f;
        }

        private void InitializeCommands(UnitData unitData) {
            _commandData = unitData.commandData;
            if (Owner != OwnerType.PLAYER ) {
                return;
            }
            // TODO: handle unitData.WeaponData
            for (int i = 0; i < _commandData.Length; i++) {
                CommandData commandData = _commandData[i];
                CommandProvider commandProvider = Instantiate(commandData.commandProviderPrefab, transform);
                commandProvider.RegisterUnit(this);
                string songName = commandData.song.name;
                Song song = ServiceLocator.Get<SongService>().Get(songName);
                commandProvider.RegisterSong(song);
            }
        }

        public void AddVisibleEnemy(Unit enemy) {
            if (enemy.Owner == Owner || enemy.IsDying) {
                return;
            }
            _enemiesInSight.Add(enemy);
        }

        public void RemoveVisibleEnemy(Unit enemy) {
            _enemiesInSight.Remove(enemy);
        }

        public Unit GetClosestEnemy() {
            if (_enemiesInSight.Count == 0) {
                return null;
            }
            Vector3 transformPosition = transform.position;
            _enemiesInSight.Sort((a, b) => (int)(Vector3.SqrMagnitude(a.transform.position - transformPosition)
                                                  - Vector3.SqrMagnitude(b.transform.position - transformPosition)));
            return _enemiesInSight[0];
        }

        public bool IsInRange(Transform target) {
            return Vector3.SqrMagnitude(target.position - transform.position) <= _range;
        }

        public ItemDeposit GetClosestDeposit() {
			if (_depositsInSight.Count == 0) {
				return null;
			}
			Vector3 transformPosition = transform.position;
			_depositsInSight.Sort((a, b) => (int)(Vector3.SqrMagnitude(a.transform.position - transformPosition)
			                                      - Vector3.SqrMagnitude(b.transform.position - transformPosition)));
			return _depositsInSight[0];
		}

        public void TakeDamage(Unit damageDealer, float streakMultiplier) {
            float prevHealth = Health;
            float damage = (damageDealer._damage + damageDealer._damage * streakMultiplier) * Time.deltaTime;
            Debug.Log(name + " took " + damage + " damage from " + damageDealer.name);
            Health -= damage;
            if (Health <= 0 && prevHealth > 0) {
                IsDying = true;
                _unitService.TriggerUnitDying(this);
                StartCoroutine(PlayDeathAnimation());
            }
        }

        private IEnumerator PlayDeathAnimation() {
            // TODO: play dying animation, sounds, etc
            yield return null;
            _unitService.TriggerUnitDied(this);
            Destroy(gameObject);
        }

        public void AddVisibleDeposit(ItemDeposit deposit) {
			if (!deposit.CanBeCollectedBy(_toolData)) {
				return;
			}
			_depositsInSight.Add(deposit);
		}

		public void RemoveVisibleDeposit(ItemDeposit deposit) {
			if (!deposit.CanBeCollectedBy(_toolData)) {
				return;
			}
			_depositsInSight.Remove(deposit);
		}

		private void OnGameFinishing() {
			StartCoroutine(WalkThroughFinishLine());
		}

		private IEnumerator WalkThroughFinishLine() {
			while (IsVisible) {
				transform.Translate(MovementSpeed * Time.deltaTime * Vector2.up);
				yield return null;
			}
		}

		private void Update() {
			_updateFunc();
        }

		private void OnDestroy() {
			_beatInputService.BeatLost -= BeatLost;
			_beatInputService.ExecutionFinishing -= ExecutionFinishing;
			_gameStateService.GameFinishing -= OnGameFinishing;
            _unitService.UnitAppeared -= AddVisibleEnemy;
            _unitService.UnitDisappeared -= RemoveVisibleEnemy;
            _unitService.UnitDying -= RemoveVisibleEnemy;
            _unitService.RemoveUnit(this);
            StopAllCoroutines();
		}

		private void BeatLost() {
			_updateFunc = DropUpdate;
		}

		private void ExecutionFinishing(Song song, int streakPower) {
			_updateFunc = Constants.Noop;
		}
		
		private void DropUpdate() {
			// do drop anims and sounds
			_updateFunc = Constants.Noop;
		}
	}
}
