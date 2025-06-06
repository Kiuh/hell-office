using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace TileUnion.Tile
{
    [AddComponentMenu("Scripts/TileUnion/Tile/TileUnion.Tile.View")]
    public class View : MonoBehaviour
    {
        [SerializeField]
        private GameObject foundation;

        [SerializeField]
        private Transform tileBase;

        [SerializeField]
        private Material transparentMaterial;

        [SerializeField]
        private Material errorMaterial;

        [SerializeField]
        private Material defaultMaterial;

        [ReadOnly]
        [SerializeField]
        private List<Renderer> renderers = new();

        [ReadOnly]
        [SerializeField]
        private Dictionary<State, Material> materialsByState;

        private readonly float selectLiftingHeight = 3;
        private float unselectedYPosition;
        private float selectedYPosition;

        private void Awake()
        {
            SetActiveChilds(transform);
            renderers = GetComponentsInChildren<Renderer>(true).ToList();
            if (foundation != null)
            {
                foreach (Renderer toRemove in foundation.GetComponentsInChildren<Renderer>(true))
                {
                    _ = renderers.Remove(toRemove);
                }
            }

            unselectedYPosition = tileBase.position.y;
            selectedYPosition = unselectedYPosition + selectLiftingHeight;

            foreach (Renderer renderer in renderers)
            {
                if (!renderer.TryGetComponent(out TextMeshPro _) && !renderer.gameObject.name.EndsWith("_um"))
                {
                    renderer.SetMaterials(new List<Material>());
                }
            }
            materialsByState = new()
            {
                { State.Normal, defaultMaterial },
                { State.Selected, transparentMaterial },
                { State.Errored, errorMaterial },
                { State.SelectedAndErrored, errorMaterial },
            };
            ApplyTileState(State.Normal);
        }

        private void SetActiveChilds(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.gameObject.SetActive(true);
                SetActiveChilds(child);
            }
        }

        // Must be called by TileImpl event
        public void ApplyTileState(State state)
        {
            if (foundation != null)
            {
                bool active = state switch
                {
                    State.Normal => true,
                    State.Selected => false,
                    State.Errored => true,
                    State.SelectedAndErrored => true,
                    _ => throw new System.ArgumentException()
                };
                foundation.SetActive(active);
            }

            float newY = state switch
            {
                State.Normal => unselectedYPosition,
                State.Selected => selectedYPosition,
                State.Errored => unselectedYPosition,
                State.SelectedAndErrored => selectedYPosition,
                _ => throw new InvalidOperationException()
            };
            tileBase.SetYPosition(newY);

            foreach (Renderer renderer in renderers)
            {
                if (!renderer.TryGetComponent(out TextMeshPro _) && !renderer.gameObject.name.EndsWith("_um"))
                {
                    renderer.sharedMaterial = materialsByState[state];
                }
            }
        }
    }
}
