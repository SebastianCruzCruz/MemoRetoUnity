using System;
using System.Collections;
using UnityEngine;
using MemoRetos.API;
using MemoRetos.Data;

namespace MemoRetos.Game
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public string       CurrentSessionId { get; private set; }
        public MemoRetoData CurrentMemoReto  { get; private set; }
        public string       SessionStartISO  { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator StartSession(int memoreto_id,
                                         Action<StartSessionResponse> onSuccess,
                                         Action<string> onError)
        {
            yield return ApiManager.Instance.StartSession(memoreto_id,
                response =>
                {
                    CurrentSessionId = response.session_id;
                    CurrentMemoReto  = response.memoreto;
                    SessionStartISO  = DateTime.UtcNow.ToString("o");
                    Debug.Log($"[SessionManager] Sesión iniciada: {CurrentSessionId}");
                    onSuccess?.Invoke(response);
                },
                err =>
                {
                    Debug.LogWarning($"[SessionManager] Error: {err}");
                    onError?.Invoke(err);
                });
        }

        public IEnumerator EndSession(bool completed, Action onDone = null)
        {
            if (string.IsNullOrEmpty(CurrentSessionId)) yield break;

            yield return ApiManager.Instance.EndSession(CurrentSessionId, completed,
                _ =>
                {
                    Debug.Log($"[SessionManager] Sesión cerrada: {CurrentSessionId}");
                    CurrentSessionId = null;
                    CurrentMemoReto  = null;
                    onDone?.Invoke();
                },
                err => Debug.LogWarning($"[SessionManager] Error al cerrar: {err}"));
        }

        public IEnumerator SubmitAnswer(SubmitAnswerRequest req,
                                         Action<SubmitAnswerResponse> onSuccess,
                                         Action<string> onError)
        {
            yield return ApiManager.Instance.SubmitAnswer(req, onSuccess, onError);
        }

        public string NowISO() => DateTime.UtcNow.ToString("o");
    }
}
