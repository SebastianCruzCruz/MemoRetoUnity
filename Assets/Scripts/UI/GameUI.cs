using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using MemoRetos.Data;

namespace MemoRetos.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _attemptText;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _maxScoreText;

        [Header("Botones")]
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _giveUpButton;

        [Header("Feedback")]
        [SerializeField] private TMP_Text   _feedbackText;
        [SerializeField] private GameObject _loadingOverlay;

        [Header("Panel Resultado")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TMP_Text   _resultMessage;
        [SerializeField] private TMP_Text   _resultScore;
        [SerializeField] private TMP_Text   _resultAttempts;
        [SerializeField] private TMP_Text   _resultRanking;
        [SerializeField] private Button     _backButton;

        private static readonly int[] _maxScores = { 5000,4000,3000,2000,1000,800,600,400,200,100 };

        private float _elapsed;
        private bool  _ticking;

        // ── Init ──────────────────────────────────────────────────────────────

        public void Setup(MemoRetoData memoreto, int attempt,
                          Action onSubmit, Action onGiveUp)
        {
            _titleText.text  = memoreto.title;
            _feedbackText.text = "";
            _resultPanel.SetActive(false);
            _loadingOverlay.SetActive(false);
            _elapsed = 0f;
            _ticking = true;

            _submitButton.onClick.RemoveAllListeners();
            _submitButton.onClick.AddListener(() => onSubmit?.Invoke());

            _giveUpButton.onClick.RemoveAllListeners();
            _giveUpButton.onClick.AddListener(() => onGiveUp?.Invoke());

            RefreshAttempt(attempt);
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (!_ticking) return;
            _elapsed += Time.deltaTime;
            int s = Mathf.FloorToInt(_elapsed);
            _timerText.text = $"{s / 60:00}:{s % 60:00}";
        }

        // ── Feedback ──────────────────────────────────────────────────────────

        public void ShowError(string msg)
        {
            _feedbackText.text = msg;
        }

        public void ShowWrongAnswer(int nextAttempt, int nextMaxScore)
        {
            _feedbackText.text = $"Incorrecto — Intento {nextAttempt}, máx {nextMaxScore} pts";
            RefreshAttempt(nextAttempt);
            _elapsed = 0f;
        }

        public void ShowResult(AnswerResultData result)
        {
            _ticking = false;
            _resultPanel.SetActive(true);

            _resultMessage.text  = result.is_correct ? "Correcto!" : "Sigue intentando";
            _resultScore.text    = $"Puntaje: {result.score}";
            _resultAttempts.text = $"Intentos: {result.attempt_number}";
            _resultRanking.text  = $"Ranking: #{result.ranking_position}";

            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(() => SceneManager.LoadScene("Lobby"));
        }

        public void SetLoading(bool state)
        {
            _loadingOverlay.SetActive(state);
            _submitButton.interactable = !state;
            _giveUpButton.interactable = !state;
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void RefreshAttempt(int attempt)
        {
            _attemptText.text  = $"Intento {attempt}";
            int max = attempt <= _maxScores.Length ? _maxScores[attempt - 1] : 50;
            _maxScoreText.text = $"Máx: {max} pts";
        }
    }
}
