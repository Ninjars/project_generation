using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(GameNode))]
    public class CircleUI : NodeUI {

        public int circlePoints = 30;
        public float spacing = 0.5f;
        public float width = 0.1f;

        private List<LineRenderer> lines;

        protected override void init() {
            lines = createCircles(gameNode.maxValue, radius, spacing);
        }

        private List<LineRenderer> createCircles(int count, float radius, float spacing) {
            List<LineRenderer> renderers = new List<LineRenderer>();
            for (int i = 0; i < count; i++) {
                float lineRadius = radius + (spacing * i);
                renderers.Add(createCircle(circlePoints, lineRadius));
            }
            return renderers;
        }

        private LineRenderer createCircle(int pointCount, float radius) {
            GameObject lineContainer = new GameObject();
            lineContainer.transform.SetParent(uiRoot.transform);
            lineContainer.transform.localPosition = Vector3.zero;
            lineContainer.transform.rotation = uiRoot.transform.rotation;

            LineRenderer lineRenderer = lineContainer.AddComponent<LineRenderer>();
            lineRenderer.material = passiveValueMaterial;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.numPositions = pointCount+1;
            lineRenderer.useWorldSpace = false;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            float deltaTheta = (2.0f * Mathf.PI) / pointCount;
            float theta = 0;// 0.5f * Mathf.PI;
            List<Vector3> tempPositions = new List<Vector3>();
            for (int i = 0; i < pointCount+1; i++) {
                float x = radius * Mathf.Cos(theta);
                float z = radius * Mathf.Sin(theta);
                Vector3 pos = new Vector3(x, z, 0);
                tempPositions.Add(pos);
                theta += deltaTheta;
            }
            lineRenderer.SetPositions(tempPositions.ToArray());
            return lineRenderer;
        }

        public override void updateRenderer() {
            uiRoot.transform.LookAt(Camera.main.transform);
            if (!shouldUpdate) {
                return;
            }
            for (int i = 0; i < lines.Count; i++) {
                Material material;
                if (i < gameNode.currentValue) {
                    material = activeValueMaterial;
                } else {
                    material = passiveValueMaterial;
                }
                lines[i].material = material;
            }
        }
    }
}
