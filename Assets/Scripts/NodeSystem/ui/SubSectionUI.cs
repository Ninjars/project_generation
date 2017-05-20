using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Node {
    [RequireComponent(typeof(GameNodeCompoundValue))]
    public class SubSectionUI : NodeUI {
        private static int[] triangles = new int[] {
            0, 1, 2,
            1, 3, 2
        };

        public float height = 1f;
        public float subSpacing = 0.025f;
        public float padding = 0.1f;
        public Material partiallyActiveMaterial;

        private GameNodeCompoundValue gameNodeSubValue;
        private List<Section> segmentRenderers;

        protected override void init() {
            gameNodeSubValue = (GameNodeCompoundValue) gameNode;
            segmentRenderers = createSections(gameNodeSubValue.maxValue, gameNodeSubValue.maxSubValue);
        }

        public override void updateRenderer() {
            uiRoot.transform.LookAt(Camera.main.transform);
            if (!shouldUpdate) {
                return;
            }
            for (int i = 0; i < segmentRenderers.Count; i++) {
                Section section = segmentRenderers[i];
                bool isComplete = i < gameNode.getOwnerValue();
                bool isActive = isComplete || (i == gameNode.getOwnerValue() && gameNodeSubValue.currentSubValue > 0);
                Material sectionMaterial;
                if (isComplete) {
                    sectionMaterial = activeValueMaterial;
                } else if (isActive) {
                    sectionMaterial = partiallyActiveMaterial;
                } else {
                    sectionMaterial = passiveValueMaterial;
                }
                section.section.material = sectionMaterial;

                List<MeshRenderer> subSections = section.subsections;
                for (int j = 0; j < subSections.Count; j++) {
                    isActive = isComplete || (isActive && j < gameNodeSubValue.currentSubValue);
                    Material subsectionMaterial;
                    if (isActive) {
                        subsectionMaterial = activeValueMaterial;
                    } else {
                        subsectionMaterial = passiveValueMaterial;
                    }
                    section.subsections[j].material = subsectionMaterial;
                }
            }
            shouldUpdate = false;
        }

        private class Section {
            public List<MeshRenderer> subsections = new List<MeshRenderer>();
            public MeshRenderer section;

            public Section(MeshRenderer renderer, List<MeshRenderer> subsections) {
                section = renderer;
                this.subsections = subsections;
            }
        }

        private List<Section> createSections(int sectionCount, int subsectionCount) {
            float sectionArc = 2f * Mathf.PI / (sectionCount * 1f);
            float subHeight = (height - (subsectionCount * subSpacing) - (2 * padding)) / subsectionCount;
            float paddedSubsection = subHeight + subSpacing;
            float sectionWidth = Mathf.Sin(sectionArc / 2f) * radius * 2f;
            List<Section> sections = new List<Section>();
            for (int i = 0; i < sectionCount; i++) {
                GameObject segmentObject = new GameObject();
                segmentObject.transform.SetParent(uiRoot.transform);
                float angle = sectionArc * i;
                segmentObject.transform.localPosition = calculateVector(angle, radius);
                segmentObject.transform.Rotate(Vector3.forward, -angle * Mathf.Rad2Deg);
                MeshFilter meshFilter = segmentObject.AddComponent<MeshFilter>();
                meshFilter.mesh = createPlane(sectionWidth, height);
                MeshRenderer meshRenderer = segmentObject.AddComponent<MeshRenderer>();

                List<MeshRenderer> subsections = new List<MeshRenderer>();
                for (int j = 0; j < subsectionCount; j++) {
                    GameObject subsectionObject = new GameObject();
                    subsectionObject.transform.SetParent(segmentObject.transform);
                    subsectionObject.transform.localPosition = new Vector3(0, paddedSubsection * j + padding, subSpacing);
                    subsectionObject.transform.Rotate(Vector3.forward, -angle * Mathf.Rad2Deg);
                    MeshFilter subMeshFilter = subsectionObject.AddComponent<MeshFilter>();
                    subMeshFilter.mesh = createPlane(sectionWidth - padding * 2, subHeight);
                    MeshRenderer subMeshRenderer = subsectionObject.AddComponent<MeshRenderer>();
                    subsections.Add(subMeshRenderer);
                }

                sections.Add(new Section(meshRenderer, subsections));
            }
            return sections;
        }

        private Mesh createPlane(float width, float height) {
            float halfWidth = width / 2f;
            Mesh mesh = new Mesh() {
                vertices = new Vector3[] {
                new Vector3(-halfWidth, 0, 0),
                new Vector3(-halfWidth, height, 0),
                new Vector3(halfWidth, 0, 0),
                new Vector3(halfWidth, height, 0)
            },
                triangles = triangles
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        private Vector3 calculateVector(float angle, float radius) {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);
            float x = sin * (radius);
            float y = cos * (radius);
            return new Vector3(x, y, 0);
        }
    }
}
