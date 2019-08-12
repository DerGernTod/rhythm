using System.Collections.Generic;
using Rhythm.Services;
using Rhythm.Tools;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Rhythm.Items {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class ItemDeposit : MonoBehaviour {
        public event UnityAction DepositDepleted;
#pragma warning disable 0649
        [SerializeField] private SpriteRenderer itemPrefab;
        [SerializeField] private AnimationCurve itemSpawnYCurve;
#pragma warning restore 0649
        public float Health { get; private set; }
        
        private ItemData itemData;
        private SpriteRenderer _spriteRenderer;
        private UnitService _unitService;
        private bool _wasVisible;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _unitService = ServiceLocator.Get<UnitService>();
            _wasVisible = _spriteRenderer.isVisible;
        }

        public bool CanBeCollectedBy(ToolData toolData) {
            if (!itemData) {
                return false;
            }
            return itemData.requiredTier.quality == 0
                   || toolData.tier.quality >= itemData.requiredTier.quality
                   && itemData.requiredToolType == toolData.type;
        }

        public void Initialize(ItemData data) {
            _spriteRenderer.sprite = data.depositSprite;
            BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
            boxCollider2D.size = _spriteRenderer.size;
            name = data.itemName + GetInstanceID();
            itemData = data;
            Health = data.amount;
        }

        public void Collect(Unit collector, float damage) {
            int prevHealth = Mathf.CeilToInt(Health);
            Health = Mathf.Clamp(Health - damage, 0, itemData.amount);
            if (Mathf.CeilToInt(Health) < prevHealth) {
                SpawnItem(collector);
            }
            if (Health <= 0) {
                Debug.Log(Time.time + ": Deposit " + name + " depleted");
                DepositDepleted?.Invoke();
            }
        }

        private void SpawnItem(Unit collector) {
            SpriteRenderer item = Instantiate(itemPrefab, transform);
            item.sprite = itemData.sprite;
            Color prevColor = item.color;
            Color transparent = new Color(prevColor.r, prevColor.g, prevColor.b, 0);
            item.color = transparent;
            StartCoroutine(Coroutines.FadeColor(item.gameObject, prevColor, 1));
            StartCoroutine(Coroutines.MoveAlongCurve(item.transform, itemSpawnYCurve, Vector3.up, 1f, false,
                () => {
                    StartCoroutine(Coroutines.FadeColor(item.gameObject, transparent, 2, () => {
                        if (Health <= 0) {
                            // todo: animate destruction
                            Destroy(gameObject);
                        }
                        Destroy(item.gameObject);
                    }));
                }));
        }

        private void Update() {
            if (Health <= 0) {
                return;
            }
            if (!_wasVisible && _spriteRenderer.isVisible) {
                IEnumerable<Unit> units = _unitService.GetAllPlayerUnits();
                foreach (Unit unit in units) {
                    unit.AddVisibleDeposit(this);
                }
            }

            if (_wasVisible && !_spriteRenderer.isVisible) {
                IEnumerable<Unit> units = _unitService.GetAllPlayerUnits();
                foreach (Unit unit in units) {
                    unit.RemoveVisibleDeposit(this);
                }
            }

            _wasVisible = _spriteRenderer.isVisible;
        }
    }
}