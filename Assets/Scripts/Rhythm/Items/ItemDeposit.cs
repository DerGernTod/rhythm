using System;
using System.Collections;
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

        private const float RAD_STEP = Mathf.PI * .1f;
        private const float MAX_CHECK_DIST = 5;

#pragma warning disable 0649
        [SerializeField] private CollectedItem itemPrefab;
        [SerializeField] private AnimationCurve itemSpawnYCurve;
#pragma warning restore 0649
        
        public NavMeshObstacle Obstacle { get; private set; }
        public Unit Unit => _unit;

        private ItemData itemData;
        private Unit _unit;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;
        private float _maxContent;
        private float _curContent;

        private void Awake() {
            _unit = GetComponent<Unit>();
            Obstacle = GetComponent<NavMeshObstacle>();
        }

        private void Start() {
            _spriteRenderer = _unit.Representation.GetComponentInChildren<SpriteRenderer>();
            _spriteRenderer.sprite = itemData.depositSprite;
            _collider = GetComponent<BoxCollider2D>();
            _collider.isTrigger = true;
            name = itemData.itemName + GetInstanceID();
            MoveToUnobstructedArea();
        }

        private void MoveToUnobstructedArea() {
            Vector3 initPos = transform.position;
            float curDetectionDistance = 0;
            float curDetectionRad = 0;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, Obstacle.radius);
            NavMeshHit hit;
            bool sampleWorked = NavMesh.SamplePosition(transform.position, out hit, Obstacle.radius, NavMesh.AllAreas);
            Vector3 targetPos = initPos;
            while (curDetectionDistance < MAX_CHECK_DIST && (!OnlyCollidesWithSelf(colliders) || !sampleWorked)) {
                targetPos = initPos + curDetectionDistance * new Vector3(Mathf.Cos(curDetectionRad), Mathf.Sin(curDetectionRad), 0);
                curDetectionDistance += .02f;
                curDetectionRad += RAD_STEP * Mathf.Lerp(1, 0.25f, curDetectionDistance / MAX_CHECK_DIST);
                sampleWorked = NavMesh.SamplePosition(targetPos, out hit, Obstacle.radius, NavMesh.AllAreas)
                    || NavMesh.FindClosestEdge(targetPos, out hit, NavMesh.AllAreas);
                if (sampleWorked) {
                    targetPos = hit.position;
                } 
                colliders = Physics2D.OverlapCircleAll(new Vector2(targetPos.x, targetPos.y), Obstacle.radius);
            }
            if (curDetectionDistance >= MAX_CHECK_DIST) {
                transform.position = initPos;
            } else {
                transform.position = targetPos;
            }
        }

        private bool OnlyCollidesWithSelf(Collider2D[] colliders) {
            return colliders.Length == 0 || (colliders.Length == 1 && colliders[0] == _collider);
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
            CollectedItem item = Instantiate(itemPrefab);
            item.transform.position = transform.position;
            item.Initialize(itemData.sprite, itemSpawnYCurve);
            ItemCollected?.Invoke(itemData);
            if (_curContent <= 0 && !_unit.IsDying) {
                _unit.Kill(collector);
                // todo: animate destruction
            }
        }

    }
}