using System;
using UnityEngine;

namespace Game.World
{
    /// <summary>Подсвечивает стену, пока она раскрыта: подменяет материалы. Только отображение.</summary>
    public class SecretRevealVisual : MonoBehaviour
    {
        [SerializeField] private IllusoryWall wall;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material revealedMaterial;

        private Material[][] _originalMaterials;

        private void Awake()
        {
            _originalMaterials = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                _originalMaterials[i] = renderers[i].sharedMaterials;
            }
        }

        private void OnEnable()
        {
            wall.OnRevealed += Show;
            wall.OnHidden += Restore;
        }

        private void OnDisable()
        {
            wall.OnRevealed -= Show;
            wall.OnHidden -= Restore;
        }

        private void Show()
        {
            if (revealedMaterial == null) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                var materials = new Material[_originalMaterials[i].Length];
                Array.Fill(materials, revealedMaterial);
                renderers[i].sharedMaterials = materials;
            }
        }

        private void Restore()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterials = _originalMaterials[i];
            }
        }
    }
}