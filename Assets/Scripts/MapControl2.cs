using System;
using UnityEngine;
using UniLinq;
using System.Collections.Generic;

public class MapControl2 : MonoBehaviour {
    // Всякая азкрытая инициализация.
    public static MapBlockModel BRICKS = new MapBlockModel() { AliasInStorage = 'B', PrefabName = "BricksMapElement", Walkable = true };
    public static MapBlockModel BRIDGE = new MapBlockModel() { AliasInStorage = 'b', PrefabName = "BridgeMapElement", Walkable = true };
    public static MapBlockModel FIRE = new MapBlockModel() { AliasInStorage = 'F', PrefabName = "FireMapElement" };
    public static MapBlockModel GRASS1 = new MapBlockModel() { AliasInStorage = 'G', PrefabName = "Grass1MapElement", Walkable = true };
    public static MapBlockModel GRASS2 = new MapBlockModel() { AliasInStorage = 'g', PrefabName = "Grass2MapElement", Walkable = true };
    public static MapBlockModel ICE1 = new MapBlockModel() { AliasInStorage = 'I', PrefabName = "Ice1MapElement", Walkable = true };
    public static MapBlockModel ICE2 = new MapBlockModel() { AliasInStorage = 'i', PrefabName = "Ice2MapElement" };
    public static MapBlockModel MONSTER = new MapBlockModel() { AliasInStorage = 'M', PrefabName = "MonsterMapElement" };
    public static MapBlockModel ROCK = new MapBlockModel() { AliasInStorage = 'R', PrefabName = "RockMapElement", Walkable = true };
    public static MapBlockModel SAND = new MapBlockModel() { AliasInStorage = 'S', PrefabName = "SandMapElement" };
    public static MapBlockModel SNOW = new MapBlockModel() { AliasInStorage = 's', PrefabName = "SnowMapElement", Walkable = true };
    public static MapBlockModel TECH = new MapBlockModel() { AliasInStorage = 'T', PrefabName = "TechMapElement", Walkable = true };
    public static MapBlockModel WOOD = new MapBlockModel() { AliasInStorage = 'W', PrefabName = "WoodMapElement", Walkable = true };
    public static List<MapBlockModel> AvailableElements = new List<MapBlockModel>() { BRICKS, BRIDGE, FIRE, GRASS1, GRASS2, ICE1, ICE2, MONSTER, ROCK, SAND, SNOW, TECH, WOOD };
    // new MapBlockModel() { AliasInStorage = 'H', PrefabName = "Chest1MapElement" } //
    // new MapBlockModel() { AliasInStorage = 'h', PrefabName = "Chest2MapElement" } //
    // new MapBlockModel() { AliasInStorage = 'c', PrefabName = "CoinMapElement" } //
    public static List<TetrisElementModel> TetrisElements = new List<TetrisElementModel>() {
        new TetrisElementModel() {
            Blocks = new MapBlockModel[][] { // Прямая линия
                new MapBlockModel[] { BRICKS.Clone(), BRICKS.Clone(), BRICKS.Clone(), BRICKS.Clone() }
            }
        },
        new TetrisElementModel() {
            Blocks = new MapBlockModel[][] { // Буква Т
                new MapBlockModel[] { GRASS1.Clone(), GRASS1.Clone(), GRASS1.Clone() },
                new MapBlockModel[] { null,           GRASS1.Clone(), null }
            }
        },
        new TetrisElementModel() {
            Blocks = new MapBlockModel[][] { // Буква Z правая
                new MapBlockModel[] { null,           GRASS2.Clone(), GRASS2.Clone() },
                new MapBlockModel[] { GRASS2.Clone(), GRASS2.Clone(), null }
            }
        },
        new TetrisElementModel() {
            Blocks = new MapBlockModel[][] { // Кубик
                new MapBlockModel[] { ICE1.Clone(), ICE1.Clone() },
                new MapBlockModel[] { ICE1.Clone(), ICE1.Clone() }
            }
        },
        new TetrisElementModel() {
            Blocks = new MapBlockModel[][] { // Буква Г правая
                new MapBlockModel[] { TECH.Clone(), TECH.Clone(), TECH.Clone() },
                new MapBlockModel[] { null,         null,         TECH.Clone() }
            }
        },
        new TetrisElementModel() {
            Blocks = new MapBlockModel[][] { // Буква Г левая
                new MapBlockModel[] { WOOD.Clone(), WOOD.Clone(), WOOD.Clone() },
                new MapBlockModel[] { WOOD.Clone(), null        , null }
            }
        }
    };

    /// <summary> Превращающиеся клетки </summary>
    public Dictionary<char, char> Interactions = new Dictionary<char, char> {
        {'H', ' '},
        {'h', ' '},
        {'c', 'B'}
    };

    protected Dictionary<int, Dictionary<int, MapBlockModel>> MapStorage = new Dictionary<int, Dictionary<int, MapBlockModel>>();
    public MapBlockModel Map(int x, int y) {
        if (MapStorage.ContainsKey(x) && MapStorage[x].ContainsKey(y))
            return MapStorage[x][y];
        return NullBlockModel.Instance;
    }
    public void Map(int x, int y, MapBlockModel block) {
        if (block == null)
            block = NullBlockModel.Instance;
        if (!MapStorage.ContainsKey(x))
            MapStorage.Add(x, new Dictionary<int, MapBlockModel>());
        if (MapStorage[x].ContainsKey(y)) {
            MapStorage[x][y].DestroyPresentation();
            MapStorage[x][y] = block;
        } else {
            MapStorage[x].Add(y, block);
        }
    }
    private void LoadMapFromFile() { }
    private void SaveMapToFile() { }

    public Vector3 MapToWorld(Vector2 from) {
        return new Vector3(from.x - from.y, (from.x + from.y) / 2f, (from.x + from.y) / 100f) + MapShift;
    }
    public Vector2 WorldToMap(Vector3 from) {
        from = from - MapShift;
        return new Vector2(from.y + from.x / 2f, from.y - from.x / 2);
    }

    List<TetrisElementModel> _tetris = new List<TetrisElementModel>();
    public IEnumerable<TetrisElementModel> Tetris { get { return _tetris; } }
    public void AddTetris(TetrisElementModel tetrisElement) {
        _tetris.Add(tetrisElement);
        if (tetrisElement.GO == null) {
            tetrisElement.CreatePresentation();
        }
        tetrisElement.GO.transform.parent = transform;
        tetrisElement.GO.transform.localScale = Vector3.one;
        tetrisElement.GO.transform.localPosition = MapToWorld((Vector2)tetrisElement.Position);
        for (var x = 0; x < tetrisElement.Blocks.Length; x++)
            for (var y = 0; y < tetrisElement.Blocks[x].Length; y++) {
                var place = tetrisElement.Position + new Position(x, y);
                if (tetrisElement.Blocks[x][y] != null && !(tetrisElement.Blocks[x][y] is NullBlockModel)) {
                    if (Map(place.X, place.Y) is NullBlockModel) {
                        Map(place.X, place.Y, tetrisElement.Blocks[x][y]);
                    } else { // 
                        Debug.LogWarning("TetrisElement " + tetrisElement.Blocks[x][y].PrefabName + " идёт туда, где уже есть блок: " + tetrisElement.Position + " + (" + x + "," + y + ") => " + place);
                        throw new Exception("TetrisElement " + tetrisElement.Blocks[x][y].PrefabName + " идёт туда, где уже есть блок: " + tetrisElement.Position + " + (" + x + "," + y + ") => " + place);
                    }
                }
            }
    }
    public void RemoveTetris(TetrisElementModel element) {
        _tetris.Remove(element);
        for (var x = 0; x < element.Blocks.Length; x++)
            for (var y = 0; y < element.Blocks[x].Length; y++) {
                var place = element.Position + new Position(x, y);
                Map(x, y, null);
            }
    }
    public void MoveTetris(TetrisElementModel element, Position Move) {

    }
    public void TetrisFinishMoving(TetrisElementModel element) {

    }

    public Vector3 MapShift = new Vector3();
    public int WIDTH = 16;
    public int HEIGHT = 28*2;

	// Use this for initialization
	void Start () {
        Map(0, 0, BRICKS.Clone());
        Map(2, 3, BRIDGE.Clone());
        Map(-1, 2, FIRE.Clone());
        Map(-2, 1, MONSTER.Clone());
    }
	
	// Update is called once per frame
	void Update () {
        // Сначала посмотреть не нужно ли создать или потушить какую-нибудь реперзентацию.
        foreach (var line in MapStorage)
            foreach (var cell in line.Value)
                if (cell.Value.FieldElement && cell.Value.ParentTetrisElement == null && cell.Value.GO == null) {
                    cell.Value.CreatePresentation();
                    cell.Value.GO.transform.parent = transform;
                    cell.Value.GO.transform.localScale = Vector3.one;
                }
        // Расположить элементы по карте
        foreach (var line in MapStorage)
            foreach (var cell in line.Value)
                if (cell.Value.GO != null && cell.Value.ParentTetrisElement == null)
                    cell.Value.GO.transform.localPosition = MapToWorld(new Vector2(line.Key, cell.Key));
        // 


    }
    public Vector3? MousePosition() {
        RaycastHit hit;
        var MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        if (Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out hit) && hit.collider == GetComponent<Collider>())
            return transform.InverseTransformPoint(hit.point);
        return null;
    }
    /// <summary> Начинаем ДрагНдроп </summary>
    void OnMouseDown() {
        Debug.Log("OnMouseDown() // " + MousePosition() + " ");
    }
    /// <summary> Продолжаем ДрагНДроп </summary>
    void OnMouseDrag() {
        //Debug.Log("OnMouseDrag() // " + MousePosition() + " ");
    }
    /// <summary> Обрываем ДрагНДроп </summary>
    void OnMouseExit() {
        Debug.Log("OnMouseExit() // " + MousePosition() + " ");
    }
    /// <summary> Заканчиваем ДрагНДроп </summary>
    void OnMouseUpAsButton() {
        Debug.Log("OnMouseUpAsButton() // " + MousePosition() + " ");
    }
}
