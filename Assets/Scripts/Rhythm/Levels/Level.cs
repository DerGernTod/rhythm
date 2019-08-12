using System.Collections.Generic;
using Rhythm.Data;
using Rhythm.Items;
using Rhythm.Persistence;
using Rhythm.Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rhythm.Levels {
	public class Level : MonoBehaviour {
		#if UNITY_EDITOR
		public static Level Singleton { get; private set; }
		#endif
		private const float HORIZONTAL_DEPOSIT_SPREAD = 1.5f;
		private const float VERTICAL_DEPOSIT_SPREAD = 4f;
		public List<ItemData> CollectedItems { get; private set; }
		
		public void Initialize(LevelData data) {
			int levelLength = data.length;
			ItemDeposit depositPrefab = Resources.Load<ItemDeposit>("prefabs/items/ItemDeposit");
			GameObject finishLinePrefab = Resources.Load<GameObject>("prefabs/misc/FinishLine");
			CollectedItems = new List<ItemData>();
			// todo define start and end, deploy deposits at correct locations
			foreach (DepositProbability prob in data.depositProbability) {
				ItemData depositData = prob.deposit;
				int amount = (int)Random.Range(prob.min, prob.max);
				float verticalStep = levelLength * .9f / Mathf.Max(2f, amount + 1);
				for (int j = 0; j < amount; j++) {
					float randomHorizontal = Random.Range(-1f, 1f) * HORIZONTAL_DEPOSIT_SPREAD;
					// limit vertical spread to vertical step so nothing is generated after the finish line
					float randomVertical = Random.Range(-1f, 1f) * Mathf.Min(verticalStep, VERTICAL_DEPOSIT_SPREAD);
					ItemDeposit deposit = Instantiate(depositPrefab, new Vector3(randomHorizontal, verticalStep * j + randomVertical), Quaternion.identity, transform);
					deposit.Initialize(depositData, Random.Range(prob.minHealth, prob.maxHealth));
					deposit.ItemCollected += item => CollectedItems.Add(item);
				}
			}

			Instantiate(finishLinePrefab, Vector3.up * levelLength, Quaternion.identity, transform);
			ServiceLocator.Get<GameStateService>().GameFinishing += OnGameFinishing;
		}

		private void OnGameFinishing() {
			PersistenceService persistenceService = ServiceLocator.Get<PersistenceService>();
			PlayerStore player = persistenceService.CurrentPlayer;
			foreach (ItemData collectedItem in CollectedItems) {
				player.AddItems(collectedItem, 1);
			}
			persistenceService.SaveCurrentPlayer();
		}

		private void OnDestroy() {
			ServiceLocator.Get<GameStateService>().GameFinishing -= OnGameFinishing;
		}

		// Use this for initialization
		void Start () {
#if UNITY_EDITOR
			Singleton = this;
			#endif
		}
	
		// Update is called once per frame
		void Update () {
		
		}
	}
}
