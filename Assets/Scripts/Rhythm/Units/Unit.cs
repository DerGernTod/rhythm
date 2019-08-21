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
	public class Unit : MonoBehaviour {
		private static int ids;
#pragma warning disable 0649
        [SerializeField] private OwnerType owner = OwnerType.NONE;
        [SerializeField] private UnitData startupUnitData;
#pragma warning restore 0649
        public OwnerType Owner => owner;
        public UnitType UnitType { get; private set; }
        public GameObject Representation => _representation;
		public float MovementSpeed { get; private set;}
        public float Health { get; private set; }
        public float Damage { get; private set; }
        public float Range => _range;
        public bool IsDying { get; private set; }
        public bool IsVisible => _renderers != null && _renderers.Any(renderer => renderer.isVisible);
        public NavMeshAgent Agent => _navMeshAgent;

        private int _unitId;
		private string _name;
        private int _xp;
        private int _xpReward;
        private float _range;
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
        private GameObject _representation;

        private void Awake() {
			_updateFunc = Constants.Noop;
			_depositsInSight = new List<ItemDeposit>();
            _enemiesInSight = new List<Unit>();
            _unitService = ServiceLocator.Get<UnitService>();
            _beatInputService = ServiceLocator.Get<BeatInputService>();
            _gameStateService = ServiceLocator.Get<GameStateService>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            if (_navMeshAgent) {
                _navMeshAgent.updateUpAxis = false;
                _navMeshAgent.updateRotation = false;
            }
            _representation = transform.Find("Representation").gameObject;
        }

        private void Start() {
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            if (startupUnitData) {
                Initialize(startupUnitData, owner);
                _unitService.AddUnit(this);
            }
        }

        public void Initialize(UnitData unitData, OwnerType assignedOwner) {

            _name = unitData.name;
            Health = unitData.health;
            MovementSpeed = unitData.movementSpeed;
            UnitType = unitData.type;
            if (assignedOwner == OwnerType.NONE) {
                assignedOwner = OwnerType.PLAYER;
            }
            string targetLayerName = "Default";
            switch (assignedOwner) {
                case OwnerType.NONE:
                    break;
                case OwnerType.NEUTRAL:
                    targetLayerName = "NeutralUnits";
                    break;
                case OwnerType.PLAYER:
                    targetLayerName = "PlayerUnits";
                    break;
                case OwnerType.AI:
                    targetLayerName = "AIUnits";
                    break;
            }
            gameObject.layer = LayerMask.NameToLayer(targetLayerName);
            owner = assignedOwner;
            _unitId = ids++;
            _toolData = unitData.toolData;
            _weaponData = unitData.weaponData;
            InitializeWeapon(unitData.weaponData);
            InitializeCommands(unitData);
            if (UnitType == UnitType.CHARACTER) {
                _unitService.UnitAppeared += AddVisibleUnit;
                _unitService.UnitDisappeared += RemoveVisibleUnit;
                _unitService.UnitDying += RemoveVisibleUnit;
            }
            _beatInputService.BeatLost += BeatLost;
            _beatInputService.ExecutionFinishing += ExecutionFinishing;
            _gameStateService.GameFinishing += OnGameFinishing;
        }

        private void InitializeWeapon(WeaponData weaponData) {
            if (!weaponData) {
                return;
            }
            Damage = weaponData.damage;
            _range = weaponData.attackType == Weapons.AttackType.Melee ? .5f : 5f;
        }

        private void InitializeCommands(UnitData unitData) {
            _commandData = unitData.commandData;
            if (Owner != OwnerType.PLAYER || _commandData == null) {
                return;
            }
            for (int i = 0; i < _commandData.Length; i++) {
                CommandData commandData = _commandData[i];
                CommandProvider commandProvider = Instantiate(commandData.commandProviderPrefab, transform);
                commandProvider.RegisterUnit(this);
                string songName = commandData.song.name;
                Song song = ServiceLocator.Get<SongService>().Get(songName);
                commandProvider.RegisterSong(song);
            }
        }

        public void AddVisibleUnit(Unit unit) {
            if (unit.IsDying) {
                return;
            }
            if (unit.Owner == OwnerType.AI && unit.UnitType == UnitType.CHARACTER) {
                _enemiesInSight.Add(unit);
            } else if (unit.UnitType == UnitType.DEPOSIT) {
                AddVisibleDeposit(unit.GetComponent<ItemDeposit>());
            }
            
        }

        public void RemoveVisibleUnit(Unit unit) {
            if (unit.UnitType == UnitType.CHARACTER) {
                _enemiesInSight.Remove(unit);
            } else if (unit.UnitType == UnitType.DEPOSIT) {
                RemoveVisibleDeposit(unit.GetComponent<ItemDeposit>());
            }
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
            return Vector3.Magnitude(target.position - transform.position) <= _range;
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

        public void TakeDamage(Unit damageDealer, float damage) {
            int prevHealth = Mathf.CeilToInt(Health);
            Health -= damage;

            if (Mathf.CeilToInt(Health) < prevHealth) {
                iTween.PunchScale(_representation, Vector3.one * 1.01f, .125f);
            }
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

        private void AddVisibleDeposit(ItemDeposit deposit) {
			if (!deposit.CanBeCollectedBy(_toolData)) {
				return;
			}
			_depositsInSight.Add(deposit);
		}

		private void RemoveVisibleDeposit(ItemDeposit deposit) {
			if (!deposit.CanBeCollectedBy(_toolData)) {
				return;
			}
			_depositsInSight.Remove(deposit);
		}

		private void OnGameFinishing() {
            if (Owner == OwnerType.PLAYER) {
                Debug.Log("Start walking through finish line, cur velocity: " + _navMeshAgent.velocity);
                StartCoroutine(WalkThroughFinishLine());
            }
		}

        private IEnumerator WalkThroughFinishLine() {
            // must have at least SOME value for x axis, no idea why
            Vector3 targetVelocity = new Vector3(0.0001f, 1, 0);
            while(IsVisible) {
                yield return null;
                _navMeshAgent.velocity = targetVelocity;
            }
        }
        
		private void Update() {
			_updateFunc();
            if (_navMeshAgent) {
                Debug.DrawLine(transform.position, _navMeshAgent.nextPosition, Color.blue);
                Debug.DrawLine(transform.position, _navMeshAgent.destination);
            }
        }

		private void OnDestroy() {
			_beatInputService.BeatLost -= BeatLost;
			_beatInputService.ExecutionFinishing -= ExecutionFinishing;
			_gameStateService.GameFinishing -= OnGameFinishing;
            if (UnitType == UnitType.CHARACTER) {
                _unitService.UnitAppeared -= AddVisibleUnit;
                _unitService.UnitDisappeared -= RemoveVisibleUnit;
                _unitService.UnitDying -= RemoveVisibleUnit;
            }
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

        public void Kill(Unit killer) {
            TakeDamage(killer, Health);
        }
    }
}
