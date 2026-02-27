using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MemoRetos.Data;
using MemoRetos.Rendering;
using MemoRetos.UI;

namespace MemoRetos.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Referencias")]
        [SerializeField] private MemoRetoVisualizer _visualizer;
        [SerializeField] private GameUI             _gameUI;

        public int    AttemptNumber   { get; private set; } = 1;
        public string AttemptStartISO { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (SessionManager.Instance?.CurrentMemoReto == null)
            {
                Debug.LogError("[GameManager] No hay memoreto cargado.");
                SceneManager.LoadScene("Lobby");
                return;
            }

            AttemptStartISO = SessionManager.Instance.SessionStartISO;

            _visualizer.BuildScene(SessionManager.Instance.CurrentMemoReto);

            _gameUI.Setup(
                SessionManager.Instance.CurrentMemoReto,
                AttemptNumber,
                onSubmit:  OnSubmitClicked,
                onGiveUp:  OnGiveUpClicked
            );
        }

        private void OnSubmitClicked()
        {
            var nodes = _visualizer.GetNodes();

            foreach (var node in nodes)
            {
                if (!node.HasValue)
                {
                    _gameUI.ShowError("Completa todos los nodos antes de enviar.");
                    return;
                }
            }

            var answers = new List<AnswerItem>();
            foreach (var node in nodes)
                answers.Add(new AnswerItem
                {
                    intersection_id = node.NodeId,
                    value           = node.CurrentValue
                });

            _gameUI.SetLoading(true);

            SubmitAnswers(answers);
        }

        private void SubmitAnswers(List<AnswerItem> answers)
        {
            var req = new SubmitAnswerRequest
            {
                id_memoreto    = SessionManager.Instance.CurrentMemoReto.id,
                attempt_number = AttemptNumber,
                start_time     = SessionManager.Instance.SessionStartISO,
                end_time       = SessionManager.Instance.NowISO(),
                answers        = answers
            };

            StartCoroutine(SessionManager.Instance.SubmitAnswer(req,
                response =>
                {
                    _gameUI.SetLoading(false);
                    var result = response.data_memoreto;

                    if (result.is_correct)
                    {
                        StartCoroutine(SessionManager.Instance.EndSession(true,
                            () => _gameUI.ShowResult(result)));
                    }
                    else
                    {
                        AttemptNumber++;
                        AttemptStartISO = SessionManager.Instance.NowISO();
                        _visualizer.ResetAllValues();
                        _gameUI.ShowWrongAnswer(AttemptNumber, result.next_attempt_max_score);
                    }
                },
                err =>
                {
                    _gameUI.SetLoading(false);
                    _gameUI.ShowError($"Error: {err}");
                }));
        }

        private void OnGiveUpClicked()
        {
            StartCoroutine(SessionManager.Instance.EndSession(false,
                () => SceneManager.LoadScene("Lobby")));
        }
    }
}
