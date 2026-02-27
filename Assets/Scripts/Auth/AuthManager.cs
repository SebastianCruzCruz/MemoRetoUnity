using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MemoRetos.API;
using MemoRetos.Data;

namespace MemoRetos.Auth
{
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        [SerializeField] private string _loginScene = "Login";
        [SerializeField] private string _lobbyScene = "Lobby";

        public UserData CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null && ApiManager.Instance.HasToken();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (ApiManager.Instance.HasToken())
                StartCoroutine(ValidateSession());
        }

        public IEnumerator Login(string username, string password,
                                  Action onSuccess,
                                  Action<string> onError)
        {
            var body = new LoginRequest { username = username, password = password };

            yield return ApiManager.Instance.Login(body,
                response =>
                {
                    ApiManager.Instance.SetToken(response.token);
                    CurrentUser = response.user;
                    Debug.Log($"[AuthManager] Login OK: {CurrentUser.username}");
                    onSuccess?.Invoke();
                },
                err =>
                {
                    Debug.LogWarning($"[AuthManager] Login error: {err}");
                    onError?.Invoke(err);
                });
        }

        public void Logout()
        {
            CurrentUser = null;
            ApiManager.Instance.ClearToken();
            SceneManager.LoadScene(_loginScene);
        }

        private IEnumerator ValidateSession()
        {
            yield return ApiManager.Instance.GetMe(
                user =>
                {
                    CurrentUser = user;
                    Debug.Log($"[AuthManager] Sesión restaurada: {CurrentUser.username}");
                    SceneManager.LoadScene(_lobbyScene);
                },
                _ =>
                {
                    ApiManager.Instance.ClearToken();
                    SceneManager.LoadScene(_loginScene);
                });
        }
    }
}
