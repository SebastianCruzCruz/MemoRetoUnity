using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MemoRetos.Rendering
{
    public class IntersectionPoint : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _label;
        [SerializeField] private Renderer    _renderer;

        [SerializeField] private Color _emptyColor    = Color.white;
        [SerializeField] private Color _assignedColor = Color.cyan;
        [SerializeField] private Color _hoverColor    = new Color(0.6f, 0.9f, 1f);

        public string NodeId       { get; private set; }
        public bool   HasValue     { get; private set; }
        public int    CurrentValue { get; private set; }

        private List<int> _numberSet;
        private int       _index = -1;
        private Material  _mat;

        public void Initialize(string nodeId, List<int> numberSet)
        {
            NodeId     = nodeId;
            _numberSet = new List<int>(numberSet);
            _mat       = _renderer.material;
            Refresh();
        }

        private void OnMouseDown()
        {
            _index = (_index + 1) % (_numberSet.Count + 1);
            HasValue     = _index < _numberSet.Count;
            CurrentValue = HasValue ? _numberSet[_index] : 0;
            Refresh();
        }

        private void OnMouseEnter() => _mat.color = _hoverColor;
        private void OnMouseExit()  => Refresh();

        public void ResetValue()
        {
            _index   = -1;
            HasValue = false;
            Refresh();
        }

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

        private void OnDestroy()
        {
            if (_mat != null) Destroy(_mat);
        }
    }
}