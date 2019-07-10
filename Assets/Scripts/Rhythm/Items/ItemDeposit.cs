using System.Collections.Generic;
using Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhythm.Items {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class ItemDeposit : MonoBehaviour {
        private float health;
        private ItemData itemData;
        [FormerlySerializedAs("enteringUnits")] public List<Unit> enclosedUnits = new List<Unit>();
        public void Initialize(ItemData data) {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = data.depositSprite;
            BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
            boxCollider2D.size = spriteRenderer.size;
            name = data.itemName + GetInstanceID();
            itemData = data;
            health = data.amount;
        }

        private void Update() {
            foreach (Unit unit in enclosedUnits) {
                // TODO: check for equipment, reduce health, drop items depending on dmg, remove if health <= 0v
            }
        }
    }
}