using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace Assets.Scripts {
    public class MapConstructorController : MonoBehaviour {
        public bool ConstructionMode;
        protected MapElement NewElement;
        protected TetrisElementModel NewTetisElement;
        void OnGUI() {
            if (ConstructionMode && GUI.Button(new Rect(Screen.width - 255, 5, 250, 60), "Construction\nMode")) {
                ConstructionMode = false;
            } else if (!ConstructionMode && GUI.Button(new Rect(Screen.width - 255, 5, 250, 60), "Play\nMode")) {
                ConstructionMode = true;
            }
        }
    }
}