using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MemoRetos.Rendering
{
    public class IntersectionPoint : MonoBehaviour
    {
        [SerializeField] private TextMeshPro    _label;
        [SerializeField] private Renderer       _renderer;
        [SerializeField] private GameObject     _inputPanel;   // Panel UI con InputField
        [SerializeField] private TMP_InputField _inputField;   // Campo de texto

        [SerializeField] private Color _emptyColor    = Color.white;
        [SerializeField] private Color _assignedColor = Color.cyan;
        [SerializeField] private Color _invalidColor  = Color.red;
        [SerializeField] private Color _hoverColor    = new Color(0.6f, 0.9f, 1f);

        public string NodeId       { get; private set; }
        public bool   HasValue     { get; private set; }
        public int    CurrentValue { get; private set; }

        private List<int> _numberSet;
        private Material  _mat;

        // ── Init ──────────────────────────────────────────────────────────────

        public void Initialize(string nodeId, List<int> numberSet)
        {
            NodeId     = nodeId;
            _numberSet = new List<int>(numberSet);
            _mat       = _renderer.material;

            _inputPanel.SetActive(false);
            _label.text = "?";
            Refresh();
        }

        // ── Click — abre el input ──────────────────────────────────────────────

        private void OnMouseDown()
        {
            _inputPanel.SetActive(true);
            _inputField.text = HasValue ? CurrentValue.ToString() : "";
            _inputField.ActivateInputField();
            _inputField.onEndEdit.RemoveAllListeners();
            _inputField.onEndEdit.AddListener(OnValueSubmitted);
        }

        // ── Validación al escribir ────────────────────────────────────────────

        private void OnValueSubmitted(string input)
        {
            _inputPanel.SetActive(false);

            if (!int.TryParse(input, out int val))
            {
                ShowInvalid("Ingresa un número.");
                return;
            }

            if (!_numberSet.Contains(val))
            {
                ShowInvalid($"{val} no está en el conjunto.");
                return;
            }

            CurrentValue = val;
            HasValue     = true;
            Refresh();
        }

        // ── Reset ──────────────────────────────────────────────────────────────

        public void ResetValue()
        {
            HasValue = false;
            _inputPanel.SetActive(false);
            Refresh();
        }

        // ── Hover ──────────────────────────────────────────────────────────────

        private void OnMouseEnter() => _mat.color = _hoverColor;
        private void OnMouseExit()  => Refresh();

        // ── Visual ─────────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (HasValue)
            {
                _label.text = CurrentValue.ToString();
                _mat.color  = _assignedColor;
            }
            else
            {
                _label.text = "?";
                _mat.color  = _emptyColor;
            }
        }

        private void ShowInvalid(string msg)
        {
            _label.text = "!";
            _mat.color  = _invalidColor;
            Debug.LogWarning($"[IntersectionPoint] {msg}");
        }

        private void OnDestroy()
        {
            if (_mat != null) Destroy(_mat);
        }
    }
}
