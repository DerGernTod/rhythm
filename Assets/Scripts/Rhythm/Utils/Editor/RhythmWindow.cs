using System;
using System.Collections.Generic;
using System.Linq;
using Rhythm.Persistence;
using Rhythm.Services;
using Rhythm.Songs;
using UnityEditor;
using UnityEngine;

namespace Rhythm.Utils.Editor {
    public class RhythmWindow: EditorWindow {
        private const int TAB_SIZE = 20;
        private bool _persistenceToolsEnabled = true;
        private bool _beatInputToolsEnabled = true;
        private bool _learnSongToolsEnabled = true;
        private PersistenceService _persistenceService;
        private BeatInputService _beatInputService;
        private SongService _songService;
        [MenuItem("Window/Rhythm")]
        static void Init() {
            RhythmWindow window = (RhythmWindow) GetWindow(typeof(RhythmWindow));
            window.Show();
            window.titleContent = EditorGUIUtility.TrTextContent("Rhythm");
        }

        private void Awake() {
            autoRepaintOnSceneChange = true;
        }

        private void OnGUI() {
            try {
                _persistenceService = ServiceLocator.Get<PersistenceService>();
                _songService = ServiceLocator.Get<SongService>();
                _beatInputService = ServiceLocator.Get<BeatInputService>();
            } catch (Exception e) {
                GUILayout.Label("Enter play mode to enable this window.");
                return;
            }

            
            _persistenceToolsEnabled = EditorGUILayout.Foldout(_persistenceToolsEnabled, "Persistence", true);
            if (_persistenceToolsEnabled) {
                DrawPersistence();
            }

            _beatInputToolsEnabled = EditorGUILayout.Foldout(_beatInputToolsEnabled, "Beat", true);
            if (_beatInputToolsEnabled) {
                DrawBeatInput();
            }
        }

        private void DrawPersistence() {
            PlayerStore currentPlayer = _persistenceService.CurrentPlayer;
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TAB_SIZE * EditorGUI.indentLevel);
            if (EditorGUILayout.DropdownButton(EditorGUIUtility.TrTextContent("Active player: " + currentPlayer.Name), FocusType.Keyboard)) {
                GenericMenu playerMenu = new GenericMenu();
                foreach (PlayerStore playerStore in _persistenceService.Players) {
                    playerMenu.AddItem(EditorGUIUtility.TrTextContent(playerStore.Name), playerStore.Name == currentPlayer.Name,
                        () => {
                            _persistenceService.ChangeActivePlayer(playerStore);
                        });
                }
                playerMenu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TAB_SIZE * EditorGUI.indentLevel);
            if (GUILayout.Button("Delete current player")) {
                _persistenceService.DeleteCurrentPlayer();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Known Songs", string.Join(", ", currentPlayer.KnownSongs));
            _learnSongToolsEnabled = EditorGUILayout.Foldout(_learnSongToolsEnabled, "Learn songs", true);
            if (_learnSongToolsEnabled) {
                EditorGUI.indentLevel++;
                foreach (string songName in _songService.GetAvailableSongNames()) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(TAB_SIZE * EditorGUI.indentLevel);
                    if (GUILayout.Button("Learn " + songName + " song")) {
                        currentPlayer.LearnSong(songName);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawBeatInput() {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Has beat", _beatInputService.HasBeat ? "yes" : "no");
            EditorGUILayout.Slider("Metronome Diff", -_beatInputService.MetronomeDiff / BeatInputService.HALF_NOTE_TIME, -1, 1);
            EditorGUILayout.LabelField("Quality", BeatInputService.CalcNoteQuality(_beatInputService.MetronomeDiff).ToString());
            EditorGUILayout.LabelField("Current Notes", string.Join(", ", _beatInputService.CurrentNotes));
            List<Song> matchingSongs = _songService.CheckSongs(_beatInputService.CurrentNotes.ToArray());
            EditorGUILayout.LabelField("Matching songs", string.Join(", ", matchingSongs.Select(song => song.Name)));
            EditorGUI.indentLevel--;
        }
    }
}