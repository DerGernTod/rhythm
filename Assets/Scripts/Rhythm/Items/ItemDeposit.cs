using System.Collections.Generic;
using Rhythm.Data;
using Rhythm.Persistence;
using Rhythm.Services;
using Rhythm.Tools;
using Rhythm.Units;
using Rhythm.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Rhythm.Items {
    [RequireComponent(typeof(Unit))]
    [RequireComponent(typeof(NavMeshObstacle))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class ItemDeposit : MonoBehaviour {
        public event UnityAction<ItemData> ItemCollected;
        
#pragma warning disable 0649
        [SerializeField] private SpriteRenderer itemPrefab;
        [SerializeField] private AnimationCurve itemSpawnYCurve;
#pragma warning restore 0649
        
        public NavMeshObstacle Obstacle { get; private set; }

        private ItemData itemData;
        private Unit _unit;
        private SpriteRenderer _spriteRenderer;
        private float _maxContent;
        private float _curContent;

        private void Awake() {
            _unit = GetComponent<Unit>();
            Obstacle = GetComponent<NavMeshObstacle>();
        }

        private void Start() {
            _spriteRenderer = _unit.Representation.GetComponentInChildren<SpriteRenderer>();
            _spriteRenderer.sprite = itemData.depositSprite;
            BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
            boxCollider2D.size = _spriteRenderer.size;
            name = itemData.itemName + GetInstanceID();
        }

        public bool CanBeCollectedBy(ToolData toolData) {
            if (!itemData) {
                return false;
            }
            return itemData.requiredTier.quality == 0
                   || toolData.tier.quality >= itemData.requiredTier.quality
                   && itemData.requiredToolType == toolData.type;
        }

        public void Initialize(ItemData data, float content) {
            itemData = data;
            _curContent = content;
            _maxContent = content;
        }

        public void Collect(Unit collector, float damage) {
            int prevContent = Mathf.CeilToInt(_curContent);
            _curContent = Mathf.Clamp(_curContent - damage, 0, _maxContent);
            if (Mathf.CeilToInt(_curContent) < prevContent) {
                SpawnItem(collector);
            }
        }

        private void SpawnItem(Unit collector) {
            SpriteRenderer item = Instantiate(itemPrefab, transform);
            item.sprite = itemData.sprite;
            Color prevColor = item.color;
            Color transparent = new Color(prevColor.r, prevColor.g, prevColor.b, 0);
            item.color = transparent;
            ItemCollected?.Invoke(itemData);
            StartCoroutine(Coroutines.FadeColor(item.gameObject, prevColor, 1));
            StartCoroutine(Coroutines.MoveAlongCurve(item.transform, itemSpawnYCurve, Vector3.up, 1f, false,
                () => {
                    StartCoroutine(Coroutines.FadeColor(item.gameObject, transparent, 2, () => {
                        if (_curContent <= 0) {
                            // todo: animate destruction
                            Destroy(gameObject);
                        }
                        Destroy(item.gameObject);
                    }));
                }));
        }

    }
}