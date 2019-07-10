using System;
using Rhythm.Services;
using Rhythm.Songs;
using Units;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Rhythm.Commands {
    
    [Serializable] public class ExecutionFinishedEvent : UnityEvent<Unit> {}
    [Serializable] public class ExecutedEvent : UnityEvent<BeatQuality, int, Unit> {}
    [Serializable] public class UpdateEvent : UnityEvent<Unit> {}
    
    [CreateAssetMenu(fileName = "CommandData", menuName = "Rhythm/CommandData", order = 2)]
    public class CommandData: ScriptableObject {
        [FormerlySerializedAs("Song")] public SongData song;

        [FormerlySerializedAs("ExecutionFinished")] public ExecutionFinishedEvent executionFinished;
        [FormerlySerializedAs("Executed")] public ExecutedEvent executed;
        [FormerlySerializedAs("Update")] public UpdateEvent update;
    }
}