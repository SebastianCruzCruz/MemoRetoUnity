using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using MemoRetos.Auth;
using MemoRetos.Game;
using MemoRetos.Data;

namespace MemoRetos.Game
{
    public class LobbyController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform  _listParent;
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private TMP_Text   _welcomeText;
        [SerializeField] private TMP_Text   _errorText;
        [SerializeField] private GameObject _loadingOverlay;
        [SerializeField] private Button     _logoutButton;

        [Header("Backend")]
        [SerializeField] private string _baseUrl = "http://localhost:5000";

        private List<MemoRetoData> _memoretos = new();

        private void Start()
        {
            _errorText.text = "";
            _logoutButton.onClick.AddListener(() => AuthManager.Instance.Logout());

            if (AuthManager.Instance.CurrentUser != null)
                _welcomeText.text = $"Hola, {AuthManager.Instance.CurrentUser.name}!";

            StartCoroutine(LoadMemoRetos());
        }

        private IEnumerator LoadMemoRetos()
        {
            _loadingOverlay.SetActive(true);

            string token = PlayerPrefs.GetString("jwt_token", "");
            using var req = UnityWebRequest.Get($"{_baseUrl}/memoretos/published");
            req.SetRequestHeader("Authorization", $"Bearer {token}");
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            _loadingOverlay.SetActive(false);

            if (req.result != UnityWebRequest.Result.Success)
            {
                _errorText.text = "Error al cargar memoretos.";
                yield break;
            }

            var wrapper = JsonUtility.FromJson<MemoRetoListWrapper>(req.downloadHandler.text);
            _memoretos  = wrapper.memoretos;

            foreach (var m in _memoretos)
                SpawnCard(m);
        }

        private void SpawnCard(MemoRetoData memoreto)
        {
            var card  = Instantiate(_cardPrefab, _listParent);
            var texts = card.GetComponentsInChildren<TMP_Text>();

            if (texts.Length >= 2)
            {
                texts[0].text = memoreto.title;
                texts[1].text = $"Nivel {memoreto.nivel} — {memoreto.dificultad}";
            }

            var btn = card.GetComponentInChildren<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnSelectMemoReto(memoreto.id));
        }

        private void OnSelectMemoReto(int memoreto_id)
        {
            StartCoroutine(SessionManager.Instance.StartSession(memoreto_id,
                _ => SceneManager.LoadScene("Game"),
                err =>
                {
                    _errorText.text = $"Error al iniciar: {err}";
                    Debug.LogWarning($"[LobbyController] {err}");
                }));
        }
    }

    [System.Serializable]
    public class MemoRetoListWrapper
    {
        public List<MemoRetoData> memoretos;
    }
}
