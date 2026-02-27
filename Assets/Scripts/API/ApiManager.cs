using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MemoRetos.Data;

namespace MemoRetos.API
{
    public class ApiManager : MonoBehaviour
    {
        public static ApiManager Instance { get; private set; }

        [SerializeField] private string _baseUrl = "http://localhost:5000/api/v1";

        private string _jwtToken;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _jwtToken = PlayerPrefs.GetString("jwt_token", "");
        }

        public void SetToken(string token)
        {
            _jwtToken = token;
            PlayerPrefs.SetString("jwt_token", token);
        }

        public void ClearToken()
        {
            _jwtToken = "";
            PlayerPrefs.DeleteKey("jwt_token");
        }

        public bool HasToken() => !string.IsNullOrEmpty(_jwtToken);

        // ── Auth ──────────────────────────────────────────────────────────────

        public IEnumerator Login(LoginRequest body,
                                 Action<LoginResponse> onSuccess,
                                 Action<string> onError)
        {
            yield return Post("/auth/login", body, false, onSuccess, onError);
        }

        public IEnumerator GetMe(Action<UserData> onSuccess,
                                 Action<string> onError)
        {
            yield return Get("/auth/me", true, onSuccess, onError);
        }

        // ── Game Session ──────────────────────────────────────────────────────

        public IEnumerator StartSession(int memoreto_id,
                                        Action<StartSessionResponse> onSuccess,
                                        Action<string> onError)
        {
            var body = new StartSessionRequest { memoreto_id = memoreto_id };
            yield return Post("/game/session/start", body, true, onSuccess, onError);
        }

        public IEnumerator EndSession(string sessionId, bool completed,
                                      Action<string> onSuccess,
                                      Action<string> onError)
        {
            var body = new EndSessionRequest { session_id = sessionId, completed = completed };
            yield return Post("/game/session/end", body, true, onSuccess, onError);
        }

        // ── Answers ───────────────────────────────────────────────────────────

        public IEnumerator SubmitAnswer(SubmitAnswerRequest body,
                                        Action<SubmitAnswerResponse> onSuccess,
                                        Action<string> onError)
        {
            yield return Post("/answers/answers", body, true, onSuccess, onError);
        }

        public IEnumerator GetRanking(string memoreto_id,
                                      Action<RankingResponse> onSuccess,
                                      Action<string> onError)
        {
            yield return Get($"/answers/ranking/{memoreto_id}", true, onSuccess, onError);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private IEnumerator Get<T>(string endpoint, bool auth,
                                   Action<T> onSuccess, Action<string> onError)
        {
            using var req = UnityWebRequest.Get(_baseUrl + endpoint);
            AddHeaders(req, auth);
            yield return req.SendWebRequest();
            HandleResponse(req, onSuccess, onError);
        }

        private IEnumerator Post<T>(string endpoint, object body, bool auth,
                                    Action<T> onSuccess, Action<string> onError)
        {
            byte[] raw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(body));
            using var req = new UnityWebRequest(_baseUrl + endpoint, "POST");
            req.uploadHandler   = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            AddHeaders(req, auth);
            yield return req.SendWebRequest();
            HandleResponse(req, onSuccess, onError);
        }

        private void AddHeaders(UnityWebRequest req, bool auth)
        {
            req.SetRequestHeader("Content-Type", "application/json");
            if (auth && HasToken())
                req.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
        }

        private void HandleResponse<T>(UnityWebRequest req,
                                       Action<T> onSuccess, Action<string> onError)
        {
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[ApiManager] {req.responseCode}: {req.error}");
                onError?.Invoke(req.downloadHandler.text);
                return;
            }
            try
            {
                onSuccess?.Invoke(JsonUtility.FromJson<T>(req.downloadHandler.text));
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiManager] Parse error: {e.Message}");
                onError?.Invoke("Error al parsear respuesta.");
            }
        }
    }
}
