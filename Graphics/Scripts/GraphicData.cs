using System.Collections.Generic;
using UnityEngine;

namespace KRG
{
    [System.Serializable]
    public struct GraphicData
    {
        public bool ExcludeFromGallery;

        public Material BaseSharedMaterial;

        public Texture2D EditorSprite;

        public string IdleAnimationName;

        public List<StateAnimation> StateAnimations;
    }
}