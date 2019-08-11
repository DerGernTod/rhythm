using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Services;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Items {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class ItemDeposit : MonoBehaviour {
        public event Action DepositDepleted;
        [SerializeField] private SpriteRenderer itemPrefab;
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
            int prevHealth = (int) Health;
            Health = Mathf.Clamp(Health - damage, 0, itemData.amount);
            if ((int) Health < prevHealth) {
                SpriteRenderer item = Instantiate(itemPrefab, transform);
                item.sprite = itemData.sprite;
                Debug.Log(collector.name + " collected " + name);
            }
            if (Health <= 0) {
                DepositDepleted?.Invoke();
            }
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