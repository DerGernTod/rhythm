using System;
using Rhythm.Commands;
using Rhythm.Services;
using Rhythm.Songs;
using Rhythm.Units;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Rhythm.Data {
    
    [Serializable] public class ExecutionFinishedEvent : UnityEvent<Unit> {}
    [Serializable] public class ExecutedEvent : UnityEvent<NoteQuality, int, Unit> {}
    [Serializable] public class UpdateEvent : UnityEvent<Unit> {}
    
    [CreateAssetMenu(fileName = "CommandData", menuName = "Rhythm/CommandData", order = 2)]
    public class CommandData: ScriptableObject {
        [FormerlySerializedAs("Song")] public SongData song;
        public CommandProvider commandProviderPrefab;
    }
}