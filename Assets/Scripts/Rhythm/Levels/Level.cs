using System.Collections;
using Rhythm.Items;
using Rhythm.Services;
using UnityEngine;

namespace Rhythm.Levels {
	public class Level : MonoBehaviour {
		private const float HORIZONTAL_DEPOSIT_SPREAD = 1.5f;
		private const float VERTICAL_DEPOSIT_SPREAD = 4f;
		
		public void Initialize(LevelData data) {
			int levelLength = data.length;
			ItemDeposit depositPrefab = Resources.Load<ItemDeposit>("prefabs/items/Item");
			GameObject finishLinePrefab = Resources.Load<GameObject>("prefabs/misc/FinishLine");

			// todo define start and end, deploy deposits at correct locations
			foreach (DepositProbability prob in data.depositProbability) {
				ItemData depositData = prob.deposit;
				int amount = (int)Random.Range(prob.min, prob.max);
				float verticalStep = levelLength / Mathf.Max(2f, amount + 1);
				for (int j = 0; j < amount; j++) {
					float randomHorizontal = Random.Range(-1f, 1f) * HORIZONTAL_DEPOSIT_SPREAD;
					// limit vertical spread to vertical step so nothing is generated after the finish line
					float randomVertical = Random.Range(-1f, 1f) * Mathf.Min(verticalStep, VERTICAL_DEPOSIT_SPREAD);
					ItemDeposit deposit = Instantiate(depositPrefab, new Vector3(randomHorizontal, verticalStep * j + randomVertical), Quaternion.identity, transform);
					deposit.Initialize(depositData);
				}
			}

			Instantiate(finishLinePrefab, Vector3.up * levelLength, Quaternion.identity, transform);
			Invoke(nameof(StartGame), 3);
		}

		private void StartGame() {
			ServiceLocator.Get<GameStateService>().TriggerGameStarted();
		}
		// Use this for initialization
		void Start () {
		
		}
	
		// Update is called once per frame
		void Update () {
		
		}
	}
}
