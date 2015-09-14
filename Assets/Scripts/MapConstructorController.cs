using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

public class MapConstructorController : MonoBehaviour {
    public bool ConstructionMode { get; protected set; }
    public GameObject ElementBackground;
    public GameObject SwitchElementTypeBtn;
    public GameObject RotateElementBtn;

    public enum ConstructionElementsType { Obstacle, Tetris, WalkableElement };
    public ConstructionElementsType SelectedType;

    protected MapBlockModel NewElement;
    protected MapBlockModel NewObstacle;
    protected TetrisElementModel NewTetisElement;

    protected int SelectedElementIndex = 0;
    protected int SelectedObstacleIndex = 0;
    protected int SelectedTetrisIndex = 0;

    void Start() {
        ElementBackground.SetActive(false);
        SwitchElementTypeBtn.SetActive(false);
        RotateElementBtn.SetActive(false);
        ElementBackground.GetComponent<ButtonHandler>().Click += NextElement;
        SwitchElementTypeBtn.GetComponent<ButtonHandler>().Click += SwitchElementsType;
        RotateElementBtn.GetComponent<ButtonHandler>().Click += RotateTetrisElement;
    }
    void OnGUI() {
        if (ConstructionMode && GUI.Button(new Rect(Screen.width - 95, 5, 90, 45), "Construction\nMode")) {
            ConstructionMode = false;
            ElementBackground.SetActive(false);
            SwitchElementTypeBtn.SetActive(false);
            RotateElementBtn.SetActive(false);
            DestroySelectedType();
        } else if (!ConstructionMode && GUI.Button(new Rect(Screen.width - 95, 5, 90, 45), "Play\nMode")) {
            ConstructionMode = true;
            CreateSelectedType();
            ElementBackground.SetActive(true);
            SwitchElementTypeBtn.SetActive(true);
            RotateElementBtn.SetActive(SelectedType == ConstructionElementsType.Tetris);
        }
    }
    private void DestroySelectedType() {
        if (SelectedType == ConstructionElementsType.Obstacle) {
            DestroyNewObstacle();
        } else if (SelectedType == ConstructionElementsType.WalkableElement) {
            DestroyNewElement();
        } else if (SelectedType == ConstructionElementsType.Tetris) {
            DestroyNewTetrisElement();
        }
    }
    private void DestroyNewObstacle() {
        NewObstacle.DestroyPresentation();
        NewObstacle = null;
    }
    private void DestroyNewTetrisElement() {
        NewTetisElement.DestroyPresentation();
        NewTetisElement = null;
    }
    private void DestroyNewElement() {
        NewElement.DestroyPresentation();
        NewElement = null;
    }
    private void CreateSelectedType() {
        if (SelectedType == ConstructionElementsType.Obstacle) {
            CreateNewObstacle();
        } else if (SelectedType == ConstructionElementsType.WalkableElement) {
            CreateNewElement();
        } else if (SelectedType == ConstructionElementsType.Tetris) {
            CreateNewTetrisElement();
        }
    }
    private void CreateNewObstacle() {
        NewObstacle = MapControl2.AvailableElements.Where(e => !e.Walkable).Skip(SelectedObstacleIndex).First().Clone();
        NewObstacle.CreatePresentation();
        NewObstacle.GO.transform.parent = ElementBackground.transform.parent;
        NewObstacle.GO.transform.localPosition = ElementBackground.transform.localPosition + Vector3.up / 8;
        NewObstacle.GO.transform.localScale = Vector3.one / 4;
    }
    private void CreateNewElement() {
        NewElement = MapControl2.AvailableElements.Where(e => e.Walkable).Skip(SelectedElementIndex).First().Clone();
        NewElement.CreatePresentation();
        NewElement.GO.transform.parent = ElementBackground.transform.parent;
        NewElement.GO.transform.localPosition = ElementBackground.transform.localPosition + Vector3.up/8;
        NewElement.GO.transform.localScale = Vector3.one / 4;
    }
    private void CreateNewTetrisElement() {
        NewTetisElement = MapControl2.TetrisElements[SelectedTetrisIndex].Clone();
        NewTetisElement.CreatePresentation();
        NewTetisElement.GO.transform.parent = ElementBackground.transform.parent;
        NewTetisElement.GO.transform.localScale = Vector3.one / 8;
        CenterTetriseElement();
    }
    private void CenterTetriseElement() {
        NewTetisElement.GO.transform.localPosition = ElementBackground.transform.localPosition - (Vector3)NewTetisElement.CenterVector2() / 8 + Vector3.up / 16;
    }
    private void SwitchElementsType() {
        DestroySelectedType();
        SelectedType = SelectedType == ConstructionElementsType.Obstacle ? ConstructionElementsType.WalkableElement : SelectedType == ConstructionElementsType.WalkableElement ? ConstructionElementsType.Tetris : ConstructionElementsType.Obstacle;
        CreateSelectedType();
        RotateElementBtn.SetActive(SelectedType == ConstructionElementsType.Tetris);
    }
    private void NextElement() {
        DestroySelectedType();
        if (SelectedType == ConstructionElementsType.Obstacle) {
            if (++SelectedObstacleIndex >= MapControl2.AvailableElements.Where(e => !e.Walkable).Count())
                SelectedObstacleIndex = 0;
        } else if (SelectedType == ConstructionElementsType.WalkableElement) {
            if (++SelectedElementIndex >= MapControl2.AvailableElements.Where(e => e.Walkable).Count())
                SelectedElementIndex = 0;
        } else if (SelectedType == ConstructionElementsType.Tetris) {
            if (++SelectedTetrisIndex >= MapControl2.TetrisElements.Count)
                SelectedTetrisIndex = 0;
        }
        CreateSelectedType();
    }
    private void RotateTetrisElement() {
        NewTetisElement.RotateRight();
        NewTetisElement.MoveElements();
        CenterTetriseElement();
    }
    void Update() {
    }
    private void CreateTetrisElements() {

    }
}