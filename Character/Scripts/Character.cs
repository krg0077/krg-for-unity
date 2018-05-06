﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace KRG {

    [RequireComponent(typeof(GraphicsController))]
    public abstract class Character : MonoBehaviour, IStateOwner {

#region serialized fields

        /// <summary>
        /// A VisRect GameObject is an advanced form of the old "Center" GameObject.
        /// It serves the same purpose, but also has added functionality.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("m_visRect")]
        protected VisRect _visRect;

        /// <summary>
        /// DEPRECATED:
        /// A child "Center" GameObject that's positioned at the visual center of the character.
        /// By contrast, the current GameObject should be positioned under the character's feet.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("m_center")]
        protected GameObject _center;

#endregion

#region private & protected fields

#if DEBUG_VISIBILITY && !NS_TMPRO
        private static bool s_isCharacterDebugTextWarningLogged;
#endif

        GraphicsController _graphicsController;

        protected Transform _transform;

#endregion

#region properties

        public GraphicsController graphicsController { get { return _graphicsController; } }

        public abstract CharacterType type { get; }

        public VisRect visRect { get { return _visRect; } }

#endregion

#region MonoBehaviour methods

        protected virtual void Awake() {
            if (_visRect != null) {
                if (_center != null) {
                    G.U.Warning("When a VisRect is assigned to this Component, "
                    + "a manual \"Center\" GameObject assignment is unnecessary, and will be ignored.");
                }
                _center = _visRect.gameObject;
            } else {
                G.U.Require(_center, "VisRect (or \"Center\" GameObject)");
            }
            _graphicsController = G.U.Require<GraphicsController>(this);
            _transform = transform;

#if DEBUG_VISIBILITY
            KRGReferences refs = G.config.krgReferences;
#if NS_TMPRO
            Instantiate(refs.characterDebugTextPrefab, _visRect.transform).Init(this);
#else
            if (!s_isCharacterDebugTextWarningLogged) {
                G.U.Warning("CharacterDebugText requires the paid version of TextMesh Pro.");
                s_isCharacterDebugTextWarningLogged = true;
            }
#endif
            _graphicsController.rasterAnimationInfo = Instantiate(refs.rasterAnimationInfoPrefab, _visRect.transform);
#endif
        }

#endregion

    }
}
