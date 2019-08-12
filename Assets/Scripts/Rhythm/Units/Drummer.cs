using Rhythm.Services;
using UnityEngine;

namespace Rhythm.Units {
	[RequireComponent(typeof(Animator))]
	public class Drummer : MonoBehaviour {
		private Animator _animator;
		private static readonly int LeftPerfect = Animator.StringToHash("LeftPerfect");
		private static readonly int RightPerfect = Animator.StringToHash("RightPerfect");
		private int nextAnim = LeftPerfect;

		private void Awake() {
			_animator = GetComponent<Animator>();
			ServiceLocator.Get<BeatInputService>().NoteHit += NoteHit;
		}

		private void NoteHit(NoteQuality arg1, float diff) {
			_animator.SetTrigger(nextAnim);
			nextAnim = nextAnim == LeftPerfect ? RightPerfect : LeftPerfect;
		}

		private void OnDestroy() {
			ServiceLocator.Get<BeatInputService>().NoteHit -= NoteHit;
		}
	}
}
