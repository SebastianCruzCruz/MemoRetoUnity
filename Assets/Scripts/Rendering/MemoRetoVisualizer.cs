using System.Collections.Generic;
using UnityEngine;
using MemoRetos.Data;

namespace MemoRetos.Rendering
{
    public class MemoRetoVisualizer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _nodePrefab;

        [Header("Materiales (URP/Lit)")]
        [SerializeField] private Material _circleMat;
        [SerializeField] private Material _triangleMat;
        [SerializeField] private Material _squareMat;
        [SerializeField] private Material _defaultMat;

        [Header("Layout")]
        [SerializeField] private float _figureRadius  = 2.5f;
        [SerializeField] private float _layoutSpacing = 5f;
        [SerializeField] private float _nodeHeight    = 0.5f;

        private List<IntersectionPoint> _nodes      = new();
        private List<GameObject>        _figureObjs = new();
        private Dictionary<string, Vector3> _nodePos = new();

        // ── Public ────────────────────────────────────────────────────────────

        public void BuildScene(MemoRetoData data)
        {
            Clear();

            int total = data.figuras.Count;
            for (int i = 0; i < total; i++)
            {
                float   angle  = (360f / total) * i;
                Vector3 center = Quaternion.Euler(0, angle, 0) * Vector3.forward * _layoutSpacing;
                SpawnFigure(data.figuras[i], center, i);
            }

            foreach (var inter in data.intersecciones)
            {
                Vector3 pos = _nodePos.ContainsKey(inter.nodo)
                    ? _nodePos[inter.nodo]
                    : Vector3.up * _nodeHeight;
                SpawnNode(inter.nodo, pos, data.number_set);
            }
        }

        public List<IntersectionPoint> GetNodes() => _nodes;

        public void ResetAllValues()
        {
            foreach (var n in _nodes) n.ResetValue();
        }

        // ── Private builders ──────────────────────────────────────────────────

        private void SpawnFigure(FiguraData fig, Vector3 center, int index)
        {
            GameObject go = fig.type.ToLower() switch
            {
                "circle"             => CreateDisc(center, _figureRadius,
                                            _circleMat   ?? _defaultMat),
                "triangle"           => CreatePolygon(center, _figureRadius, 3,
                                            _triangleMat ?? _defaultMat),
                "square" or "rectangle" => CreatePolygon(center, _figureRadius, 4,
                                            _squareMat   ?? _defaultMat),
                _                    => CreateDisc(center, _figureRadius, _defaultMat)
            };

            go.name = $"Figura_{fig.type}_{index}";
            go.transform.SetParent(transform);
            _figureObjs.Add(go);

            // Registrar posición aproximada de cada nodo de esta figura
            int n = fig.nodos.Count;
            for (int i = 0; i < n; i++)
            {
                float   a   = (2 * Mathf.PI / n) * i;
                Vector3 pos = center + new Vector3(
                    Mathf.Cos(a) * _figureRadius,
                    _nodeHeight,
                    Mathf.Sin(a) * _figureRadius);

                if (_nodePos.ContainsKey(fig.nodos[i]))
                    _nodePos[fig.nodos[i]] = (_nodePos[fig.nodos[i]] + pos) * 0.5f;
                else
                    _nodePos[fig.nodos[i]] = pos;
            }
        }

        private void SpawnNode(string nodeId, Vector3 pos, List<int> numberSet)
        {
            if (_nodePrefab == null)
            {
                Debug.LogError("[MemoRetoVisualizer] Asigna _nodePrefab en el Inspector.");
                return;
            }
            var go   = Instantiate(_nodePrefab, pos, Quaternion.identity, transform);
            go.name  = $"Node_{nodeId}";
            var node = go.GetComponent<IntersectionPoint>();
            node.Initialize(nodeId, numberSet);
            _nodes.Add(node);
        }

        // ── Geometry ──────────────────────────────────────────────────────────

        private GameObject CreateDisc(Vector3 center, float radius, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.transform.position   = center;
            go.transform.localScale = new Vector3(radius * 2, 0.05f, radius * 2);
            go.GetComponent<Renderer>().material = mat;
            Destroy(go.GetComponent<Collider>());
            return go;
        }

        private GameObject CreatePolygon(Vector3 center, float radius, int sides, Material mat)
        {
            var go            = new GameObject($"Polygon_{sides}");
            go.transform.position = center;
            var lr            = go.AddComponent<LineRenderer>();
            lr.useWorldSpace  = false;
            lr.loop           = true;
            lr.positionCount  = sides;
            lr.startWidth     = 0.12f;
            lr.endWidth       = 0.12f;
            lr.material       = mat;
            for (int i = 0; i < sides; i++)
            {
                float a = (2 * Mathf.PI / sides) * i - Mathf.PI / 2f;
                lr.SetPosition(i, new Vector3(
                    Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
            }
            return go;
        }

        // ── Cleanup ───────────────────────────────────────────────────────────

        private void Clear()
        {
            foreach (var go in _figureObjs) if (go) Destroy(go);
            foreach (var n  in _nodes)      if (n)  Destroy(n.gameObject);
            _figureObjs.Clear();
            _nodes.Clear();
            _nodePos.Clear();
        }
    }
}
