﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KRG {

    public static class KRGMenuCreateConfig {

        [MenuItem("KRG/Create KRGConfig", false, 1)]
        public static void CreateConfig() {
            KRGConfig config;
            //see if the config already exists
            config = Resources.Load<KRGConfig>(KRGConfig.resourcePath);
            if (config != null) {
                G.Err("A KRGConfig asset already exists in a Resources folder.", config);
                Resources.UnloadAsset(config);
                return;
            }
            //create a new config asset (in the resources folder)
            config = ScriptableObject.CreateInstance<KRGConfig>();
            AssetDatabase.CreateAsset(config, KRGConfig.assetPath);
            AssetDatabase.SaveAssets();
			//ping the asset in the Project window
			EditorGUIUtility.PingObject(config);
			//finish
			G.Log("A KRGConfig asset was created at {0}.", KRGConfig.assetPath);
        }
    }
}
