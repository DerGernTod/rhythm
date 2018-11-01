﻿using Services;
using UnityEngine;

namespace Units {
	[RequireComponent(typeof(Animator))]
	public class Drummer : MonoBehaviour {
		private Animator _animator;

		private void Awake() {
			_animator = GetComponent<Animator>();
			ServiceLocator.Get<BeatInputService>().BeatHit += OnBeatHit;
		}

		private void OnBeatHit(BeatQuality arg1, float diff, int streak) {
			_animator.SetTrigger("LeftPerfect");
		}

		private void OnDestroy() {
			ServiceLocator.Get<BeatInputService>().BeatHit -= OnBeatHit;
		}
	}
}
