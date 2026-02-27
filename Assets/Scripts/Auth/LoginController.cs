using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using MemoRetos.Auth;

namespace MemoRetos.Auth
{
    public class LoginController : MonoBehaviour
    {
        [Header("Campos")]
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private TMP_InputField _passwordInput;

        [Header("Botones")]
        [SerializeField] private Button _loginButton;

        [Header("Feedback")]
        [SerializeField] private TMP_Text   _errorText;
        [SerializeField] private GameObject _loadingOverlay;

        [Header("Siguiente escena")]
        [SerializeField] private string _lobbyScene = "Lobby";

        private void Start()
        {
            _errorText.text = "";
            _loadingOverlay.SetActive(false);
            _loginButton.onClick.AddListener(OnLoginClicked);
            _passwordInput.onSubmit.AddListener(_ => OnLoginClicked());
        }

        private void OnLoginClicked()
        {
            string user = _usernameInput.text.Trim();
            string pass = _passwordInput.text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                _errorText.text = "Ingresa usuario y contraseña.";
                return;
            }

            StartCoroutine(DoLogin(user, pass));
        }

        private IEnumerator DoLogin(string user, string pass)
        {
            SetLoading(true);

            yield return AuthManager.Instance.Login(user, pass,
                onSuccess: () =>
                {
                    SetLoading(false);
                    SceneManager.LoadScene(_lobbyScene);
                },
                onError: err =>
                {
                    SetLoading(false);
                    _errorText.text = "Usuario o contraseña incorrectos.";
                    Debug.LogWarning($"[LoginController] {err}");
                });
        }

        private void SetLoading(bool state)
        {
            _loadingOverlay.SetActive(state);
            _loginButton.interactable = !state;
            _errorText.text = "";
        }
    }
}
