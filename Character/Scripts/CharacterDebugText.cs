﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if NS_TMPRO
using TMPro;
#endif

namespace KRG {

    public class CharacterDebugText : MonoBehaviour {
        
        ICharacterDebugText _characterInterface;

#if NS_TMPRO
        TextMeshPro _text;
#else
        TextMesh _text;
#endif

        void Awake() {
#if NS_TMPRO
            _text = G.U.Require<TextMeshPro>(this);
#else
            _text = G.U.Require<TextMesh>(this);
#endif
        }

        public void Init(Character character) {
            _characterInterface = character as ICharacterDebugText;
            if (_characterInterface == null) {
                G.U.Warning("This character must implement the ICharacterDebugText interface to show debug info.",
                    this, character);
                G.End(gameObject);
            }
        }

        void LateUpdate() {
            _text.text = _characterInterface.lateUpdateText;
        }
    }
}
