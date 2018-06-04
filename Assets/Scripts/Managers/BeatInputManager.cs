using System;
using UnityEngine;

namespace Managers {
    public class BeatInputManager : MonoBehaviour {
        private BeatManager _beatManager;
        private double _startTime;
        private double _lastBeatTime;
        private float _givenBeat;
        private bool _hasBeat = true;
        private void Start() {
            _beatManager = GetComponent<BeatManager>();
            _startTime = AudioSettings.dspTime;
            _givenBeat = _beatManager.GetBeatsPerSecond();
        }

        private void Update() {
            double curTime = AudioSettings.dspTime - _startTime;
            if (Input.GetKeyDown(KeyCode.A)) {
                if (_hasBeat) {
                    HandleBeat(curTime);
                } else {
                    //HandleNewBeat(curTime);
                }
                _lastBeatTime = curTime;
            }
        }

        private void HandleBeat(double curTime) {
            // if the hit is off more than one full beat + half a second, lose it
            if (curTime > _lastBeatTime + _givenBeat + .5f) {
                //_hasBeat = false;
            } else {
                double dist = Math.Abs(curTime - _lastBeatTime - _givenBeat);
//                Debug.Log("beat dist " + dist);
                if (dist < .05) {
                    Debug.Log("perfect!");
                } else if (dist < .1) {
                    Debug.Log("good!");
                } else if (dist < .25) {
                    Debug.Log("bad!");
                } else {
                    Debug.Log("miss!");
                    //_hasBeat = false;
                }
            }
        }

        private void HandleNewBeat(double curTime) {
            if (Math.Abs(_lastBeatTime) > double.Epsilon) {
                float bps = (float) (curTime - _lastBeatTime);
                _givenBeat = bps;
                _beatManager.UpdateBeatsPerSecond(bps);
                _hasBeat = true;
            } 
        }
        
    }
}