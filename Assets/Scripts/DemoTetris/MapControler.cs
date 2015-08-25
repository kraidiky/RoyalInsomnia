using UnityEngine;
using UniLinq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MapControler : MonoBehaviour {

    private List<MapElement> Elements { get; set; }
    public int MAP_WIDTH { get; set; } // По координате X
    public int MAP_LENGTH { get; set; } // По кооринате Y
    public int MAP_HEIGHT { get { return 2; } } // По координате Z
    private Vector3 _elementSize = new Vector3(1, 0.53f, 0.001f);
    public Vector3 ElementSize { get { return _elementSize; } }
    /// <summary> Индекс это координата Y </summary>
    public Dictionary<int, MapLine> Map = new Dictionary<int, MapLine>();
    /// <summary> Первый индек Y, Второй индекс Z, Внутри строки порядок символов X. </summary>
    public List<List<char[]>> StoredMap = new List<List<char[]>>();
    public float BasicSheepSpeed { get; protected set; }
    public float CurrentMapShift { get; protected set; }
    private float _mapSpeed = .5f;
    public float MapSpeed { get { return _mapSpeed; } }

    public GameObject TetrisBlocks;
    public GameObject MapElements;
    public GameObject Prediction;

    // Всякая азкрытая инициализация.
    /// <summary> Символьные алиасы элементов карты. </summary>
    public Dictionary<char, string> Presets = new Dictionary<char, string>() {
        { 'B', "BricksMapElement" },
        { 'b', "BridgeMapElement" },
        { 'H', "Chest1MapElement" },
        { 'h', "Chest2MapElement" },
        { 'c', "CoinMapElement" },
        { 'F', "FireMapElement" },
        { 'G', "Grass1MapElement" },
        { 'g', "Grass2MapElement" },
        { 'I', "Ice1MapElement" },
        { 'i', "Ice2MapElement" },
        { 'M', "MonsterMapElement" },
        { 'R', "RockMapElement" },
        { 'S', "SnadMapElement" },
        { 's', "SnowMapElement" },
        { 'T', "TechMapElement" },
        { 'W', "WoodMapElement" }
    };
    /// <summary> Проходимые клетки </summary>
    public char[] Possible = new[] { 'B', 'b', 'c', 'G', 'g', 'I', 'R', 's', 'T', 'W' };
    public Dictionary<char, char> Interactions = new Dictionary<char, char> {
        {'H', ' '},
        {'h', ' '},
        {'c', 'B'}
    };

    /// <summary> Элементы которые можно кидать на карту. Первая координата - номер элемента, вторая и третья координаты X и Y соответственно. </summary>
    public List<List<List<char>>> Tetrised = new List<List<List<char>>>() {
        new List<List<char>> { // Прямая линия
            new List<char> { 'G','G','G','G' }
        },
        new List<List<char>> { // Буква Т
            new List<char>() { 'g','g','g' },
            new List<char>() {     'g' }
        },
        new List<List<char>> { // Буква Z правая
            new List<char>() { ' ',' ','B','B', },
            new List<char>() {     'B','B' }
        },
        new List<List<char>> { // Буква Z левая
            new List<char>() { 'T','T' },
            new List<char>() {     'T','T' }
        },
        new List<List<char>> { // Кубик
            new List<char>() { ' ','I','I' },
            new List<char>() {     'I','I' }
        },
        new List<List<char>> { // Буква Г правая
            new List<char>() { ' ','R','R','R' },
            new List<char>() {     'R' }
        },
        new List<List<char>> { // Буква Г левая
            new List<char>() { 'S','S','S'},
            new List<char>() {     ' ','S' }
        }
    };

    // Use this for initialization
    void Start() {
        Elements = new List<MapElement>();
        Screen.orientation = ScreenOrientation.Portrait;
        var smth = Resources.Load("pregeneratedMap") as TextAsset;
        var firstString = new Regex("(\\d*)\r?\n");
        var matches = firstString.Match(smth.text);
        if (!matches.Success)
            Debug.LogError("Не прочиталась первая строка с шириной карты");
        MAP_WIDTH = int.Parse(matches.Groups[1].Value);
        var index = matches.Index + matches.Groups[0].Length;
        var eachString = new Regex("([^\r\n]{0," + MAP_WIDTH + "})(\t([^\r\n]{0," + MAP_WIDTH + "}))?(\r\n|\n|$)");
        Func<string, int, char[]> fill = (str, length) => Enumerable.Range(0, length).Select(i => i < str.Length ? str[i] : ' ').ToArray();
        do {
            matches = eachString.Match(smth.text, index);
            if (!matches.Success) break;
            index += matches.Groups[0].Length;
            StoredMap.Add(new List<char[]> { fill(matches.Groups[1].Value, MAP_WIDTH) , fill(matches.Groups[3].Value, MAP_WIDTH) });
        } while (index < smth.text.Length);
        MAP_LENGTH = StoredMap.Count;
        //Debug.Log(StoredMap.ToJoinedString((str, x) => "|" + str.ToJoinedString((level, y) => level, "|") + "|", "\n"));
        foreach (var preset in Presets)
            PresetObjects[preset.Key] = Resources.Load(preset.Value);
        // Установка элементов тетриса
        createTetrisBlock();
        createTetrisBlock();
        // Установка исходной овцы
        Sheeps.Add(PlaceNextEmptyPosition(-(MAP_WIDTH - 1) / 2f));
        Test();
    }
    private void Test(bool dara = true) {
        Debug.Log("dara = " + dara);
    }
    public enum SpeedMultiplayer { x16, x8, x5, x3, x2, x1, x0 };
    public SpeedMultiplayer CurrentSpeedMultiplayer = SpeedMultiplayer.x2;
    public int OvecCount = 1;
    private void OnGUI() {
        Func<float, float, float, float, Rect> ButtomRight = (right, bottom, width, height) => new Rect(Screen.width - right - width, Screen.height - bottom - height, width, height);
        //Func<float, float, float, float, Rect> TopRight = (right, top, width, height) => new Rect(Screen.width - right - width, top, width, height);
        //Func<float, float, float, float, Rect> ButtomLeft = (left, bottom, width, height) => new Rect(left, Screen.height - bottom - height, width, height);
        if (GUI.Button(ButtomRight(5, 5, 100, 100), "Rotate"))
            CurrentTetrisBlock.RotateElements();
        if (GUI.Button(new Rect(5, 5, 60, 35), "Restart"))
            Application.LoadLevel(0);
        if (UseDestructionMode) {
            if (GUI.Button(new Rect(70, 5, 75, 35), "Destruct"))
                UseDestructionMode = false;
        } else {
            if (GUI.Button(new Rect(70, 5, 75, 35), "Replace"))
                UseDestructionMode = true;
        }
        if (UseSheepWayDetection) {
            if (GUI.Button(new Rect(150, 5, 105, 35), "Way Detection"))
                UseSheepWayDetection = false;
        } else {
            if (GUI.Button(new Rect(150, 5, 105, 35), "Manual Way"))
                UseSheepWayDetection = true;
        }
        Action<float, float, float, float, string, SpeedMultiplayer, float> GUISpeed = (float left, float top, float width, float height, string text, SpeedMultiplayer multiplier, float speed) => {
            if (CurrentSpeedMultiplayer == multiplier) {
                GUI.Box(new Rect(left, top, width, height), text);
            } else if (GUI.Button(new Rect(left, top, width, height), text)) {
                CurrentSpeedMultiplayer = multiplier;
                _mapSpeed = speed;
            }
        };
        GUISpeed(5, 50, 40, 45, "x16",  SpeedMultiplayer.x16,   2.4f);
        GUISpeed(5, 100, 30, 45, "x8",  SpeedMultiplayer.x8,    1.2f);
        GUISpeed(5, 150, 30, 45, "x5",  SpeedMultiplayer.x5,    .75f);
        GUISpeed(5, 200, 30, 45, "x3",  SpeedMultiplayer.x3,    .45f);
        GUISpeed(5, 250, 30, 45, "x2",  SpeedMultiplayer.x2,    .3f);
        GUISpeed(5, 300, 30, 45, "x1",  SpeedMultiplayer.x1,    .15f);
        GUISpeed(5, 350, 30, 45, "x0",  SpeedMultiplayer.x0,    0);
        Action<float, float, float, float, int> GUIOvec = (float left, float top, float width, float height, int count) => {
            if (OvecCount == count) {
                GUI.Box(new Rect(left, top, width, height), count + "овц");
            } else if (GUI.Button(new Rect(left, top, width, height), count + "овц")) {
                OvecCount = count;
                if (Sheeps.Count > OvecCount) { // Если овец больше чем нужно.
                    Sheeps.OfType<WaitingSheepStrategy>().Take(Sheeps.Count - OvecCount).Each(sheep => sheep.DestroyPrefab()).Each(sheep => Sheeps.Remove(sheep)); // Сначала удаляем тех, кто в режиме ожидания
                    if (Sheeps.Count > OvecCount)
                        Sheeps.Take(Sheeps.Count - OvecCount).Each(sheep => sheep.DestroyPrefab()).Each(sheep => Sheeps.Remove(sheep));// Потом обычных если ещё нужно.
                } else if (Sheeps.Count < OvecCount) { // Если овец меньше, чем нужно.
                    while (Sheeps.Count < OvecCount)
                        OrientedSheepStrategy.CreateNew(this);
                }
            }
        };
        GUIOvec(5, 400, 40, 45, 1);
        GUIOvec(5, 450, 40, 45, 3);
        GUIOvec(5, 500, 40, 45, 5);
        GUIOvec(5, 550, 40, 45, 8);
        


    }
    private void DestroyElementOnMap(MapElement element) {
        UnregisterMapElement(element);
        Destroy(element.gameObject);
    }

    // Update is called once per frame
    public int allowedLoops = 1;
    public bool playUpdate = true;
    void Update() {
        if (!playUpdate) return;
        if (--allowedLoops < 0) return;
        if (Input.GetMouseButtonDown(1)) // Разворот по правому клику
            CurrentTetrisBlock.RotateElements();
        CurrentMapShift += Time.deltaTime * _mapSpeed;
        foreach (var sheep in Sheeps)
            sheep.Update(this);
        DrawTetris();
        // Границы зримого
        var firstY = CurrentMapShift - MAP_LENGTH / 2f;
        var lastY = CurrentMapShift + MAP_LENGTH / 2f;
        // Поудалять всех до первого и после последнего.
        for (var index = Elements.Count-1; index>=0; index--) {
            var positionY = CalculateYPosition(Elements[index].Position);
            if (firstY > positionY || positionY > lastY)
                DestroyElementOnMap(Elements[index]);
        }
        // Подобавлять всех, кого только нужно из сохранённого уровня.
        var minY = Mathf.Max(Mathf.CeilToInt(firstY - (MAP_WIDTH - 1) / 2f), 0);
        var maxY = Mathf.Min(Mathf.FloorToInt(lastY + (MAP_WIDTH - 1) / 2f), StoredMap.Count - 1);
        for (var y = minY; y <= maxY; y++)
            for (var x = 0; x < MAP_WIDTH; x++)
                for (var z = 0; z < MAP_HEIGHT; z++) {
                    var positionY = CalculateYPosition(x, y);
                    if (firstY <= positionY && positionY <= lastY) { // Если это в пределах области
                        if (Presets.ContainsKey(StoredMap[y][z][x])) { // Если в застореной карте на этом месте что-то есть
                            if ((!Map.ContainsKey(y)) || Map[y].XCells[x].ZDepths[z] == null) { // Если мапа не содержит такой строки, или мозиция на карте пустая.
                                GameObject instance = GameObject.Instantiate(PresetObjects[StoredMap[y][z][x]]) as GameObject;
                                var element = instance.GetComponent<MapElement>();
                                element.SourceLetter = StoredMap[y][z][x];
                                element.Position = new Position(x, y, z);
                                RegisterMapElement(element);
                            }
                        }
                    }
                }
        // И все элементы передвинуть по координатам
        foreach (var element in Elements)
            element.transform.localPosition = MapToWorld(element.Position);
        // Вставка тетриса
        CorrectTetrisPosition();
        //Debug.Log(PointerOnScreenPosition.ToString("F6"));
        PointerOnScreenPosition = Position.Between(PointerOnScreenPosition - Time.deltaTime * _mapSpeed * Vector2.up * ElementSize.y * transform.localScale.y, -AvailableSize, AvailableSize);
        var onMap = WorldToMap(PointerOnScreenPosition + CurrentMapShift * Vector2.up * ElementSize.y);
        PreviewPosition = Position.Round(onMap);
        PreviewPosition.Z = 0;
    }
    public MapElement ElementsFactory(char letter, int x, int y, int z = 0) {
        if (Presets.ContainsKey(letter)) {
            GameObject instance = GameObject.Instantiate(PresetObjects[letter]) as GameObject;
            var element = instance.GetComponent<MapElement>();
            element.SourceLetter = letter;
            element.Position = new Position(x, y, z);
            return element;
        }
        return null;
    }

    private Dictionary<char, UnityEngine.Object> PresetObjects = new Dictionary<char, UnityEngine.Object>();

    public void RegisterMapElement(MapElement element) {
        if (Elements.Contains(element))
            throw new System.Exception(GetType().Name + ".RegisterMapElement(" + element.GetType().Name + "," + element.Position + ") // Элемент уже присутствует в списке.");
        //Debug.Log(this + ".RegisterMapElement(" + element + ")");
        Elements.Add(element);
        element.transform.parent = this.MapElements.transform;
        element.transform.localScale = new Vector3(ElementSize.x / element.Size.x, ElementSize.y / element.Size.y);
        // element.transform.localPosition = MapToWorld(element.Position); Это из Update-а делалось.
        AddToMap(element);
    }
    public void UnregisterMapElement(MapElement element) {
        if (!Elements.Contains(element))
            throw new System.Exception(GetType().Name + ".UnregisterMapElement(" + element + ") // Элемент отсутствует в списке.");
        //Debug.Log(this + ".UnregisterMapElement(" + element + ")");
        RemoveFromMap(element);
        Elements.Remove(element);
    }

    private void RemoveFromMap(MapElement element) {
        Map[element.Position.Y].XCells[element.Position.X].ZDepths[element.Position.Z] = null;
        if (Map[element.Position.Y].XCells.All(line => !line.ZDepths.Any())) // Если вся строка пустая
            Map.Remove(element.Position.X);
    }
    private void AddToMap(MapElement element) {
        if (!Map.ContainsKey(element.Position.Y))
            Map[element.Position.Y] = new MapLine() { XCells = Enumerable.Range(1, MAP_WIDTH).Select(index => new MapCell()).ToArray() };
            Map[element.Position.Y].XCells[element.Position.X].ZDepths[element.Position.Z] = element;
    }
    public float CalculateYPosition(int x, int y) {
        return x - (MAP_WIDTH - 1) / 2f + y * 2;
    }
    public float CalculateYPosition(Position target) {
        return CalculateYPosition(target.X, target.Y);
    }
    public Vector3 MapToWorld(Position position) {
        var center = (MAP_WIDTH - 1) / 2f;
        var result = new Vector3(
                    (position.X - center) * ElementSize.x,
                    (position.Y * 2 + position.X - center - CurrentMapShift) * ElementSize.y,
                    (-position.Z + MAP_HEIGHT * (position.X + MAP_WIDTH * position.Y)) * ElementSize.z);
        return result;
    }
    public Vector3 MapToWorld(Position position, float deltaZ) {
        return MapToWorld(position) + deltaZ * ElementSize.z * Vector3.back;
    }
    public Vector3 WorldToMap(Vector3 position) {
        var center = (MAP_WIDTH - 1) / 2f;
        var X = position.x / ElementSize.x + center;
        var Y = (position.y / ElementSize.y - (X - center)) / 2;
        var Z = MAP_HEIGHT * (X + MAP_WIDTH * Y) - position.z / ElementSize.z;
        return new Vector3(X,Y,Z);
    }
    public class MapLine {
        // Индекс это координата X
        public MapCell[] XCells;

    }
    public class MapCell {
        /// <summary> Индекс это координата Z </summary>
        public Dictionary<int, MapElement> ZDepths = new Dictionary<int, MapElement>() { { 0, null }, { 1, null } };
    }

    #region Sheep
    public List<SheepStrategy> Sheeps = new List<SheepStrategy>();
    // Использовать автопоиск пути
    public bool UseSheepWayDetection = false;
    public void Repalce(SheepStrategy previous, SheepStrategy next) {
        Sheeps[Sheeps.IndexOf(previous)] = next;
    }

    public SheepStrategy PlaceNextEmptyPosition(float from) {
        //Debug.Log(GetType().Name + ".PlaceNextEmptyPosition(" + from + ")");
        var finded = FindNextEmptyPosition(from + (MAP_WIDTH - 1) / 2f);
        if (finded != null) {
            var sheep = new SheepMoveStrategy() {
                Position = finded.Position,
                MoveTo = finded.MoveTo,
                ToPosition = Target(finded.Position, finded.MoveTo),
                SheepShift = CalculateYPosition(finded.Position) - CurrentMapShift
            };
            sheep.CreateRequiredPrefab(this);
            return sheep;
        }
        return null;
    }
    public OrientedSheepStrategy FindNextEmptyPosition(float from) {
        for (var i = Mathf.FloorToInt(from); i < MAP_LENGTH * 2 + MAP_WIDTH; i++) {
            for (int x = i % 2, y = (i - i % 2) / 2; x < MAP_WIDTH && y < MAP_LENGTH; x += 2, y--) {
                if (0 <= y && y < StoredMap.Count && Possible.Contains(StoredMap[y][0][x])) { // Эта клетка пустая
                    if (x + 1 < MAP_WIDTH && Possible.Contains(StoredMap[y][0][x + 1])) { // А также пустая следующая направо
                        return new OrientedSheepStrategy() {
                            MoveTo = SheepMoveStrategy.Direction.Right,
                            Position = new Position(x, y),
                        };
                    }
                    if (y + 1 < MAP_LENGTH && x - 1 >= 0 && Possible.Contains(StoredMap[y + 1][0][x - 1])) { // Или пустая следующая налево
                        return new OrientedSheepStrategy() {
                            MoveTo = SheepMoveStrategy.Direction.Left,
                            Position = new Position(x, y),
                        };
                    }
                    sheepShift = CalculateYPosition(x, y) - CurrentMapShift; // Иначе просто двигать
                }
            }
        }
        return null;
    }

    public enum SheepActions { Crash, Left, Right };
    public Dictionary<SheepActions, string> Prototypes = new Dictionary<SheepActions, string> {
        { SheepActions.Left, "SheepLeftMove" },
        { SheepActions.Right, "SheepRightMove" },
        { SheepActions.Crash, "SheepCrashMove" }
    };
    public SheepActions sheepState;
    public float SheepCrashPosition;

    public float sheepAcceleration = 0.5f;
    public float targetSheepShift = -5;
    /// <summary> Сдвиг овцы относительно центра карты. </summary>
    public float sheepShift = 0.5f;
    public Position PrevSheepPosition;
    public Position NextSheepPosition;

    private void MoveSheeps() {
        // Приближаем сдвиг овцы к целевому.
        var finishLine = CalculateYPosition(NextSheepPosition);
        if (sheepShift > targetSheepShift) {
            sheepShift = Mathf.Max(sheepShift - _mapSpeed * Time.deltaTime * sheepAcceleration, targetSheepShift);
        } else if (sheepShift < targetSheepShift) {
            sheepShift = Mathf.Min(sheepShift + _mapSpeed * Time.deltaTime * sheepAcceleration, targetSheepShift);
        }
        // Определение не достигла ли овца целевой позиции
        if (finishLine < CurrentMapShift + sheepShift) {
            // Замена собранных элементов
            for (var z = 0; z< MAP_HEIGHT; z++) {
                var collectable = Map[NextSheepPosition.Y].XCells[NextSheepPosition.X].ZDepths[0].GetComponent<CollectableMapElement>();
                if (collectable != null) {
                    collectable.DoSomething();
                    StoredMap[NextSheepPosition.Y][z][NextSheepPosition.X] = collectable.ReplaceElement[0];
                    DestroyElementOnMap(collectable.GetComponent<MapElement>());
                    // Создастся само из апдейта.
                }
            }
        }
    }

    #endregion

    #region Tetris
    public DropedMapParts CurrentTetrisBlock;
    public DropedMapParts NextTetrisBlock;
    public Vector2 PointerOnScreenPosition;
    public Position PointerOnMap;
    public Vector2 AvailableSize = new Vector2(5f, 5f);
    public bool ShowPreview = false;
    private Position PreviewPosition;
    private List<GameObject> PreviewsAdd = new List<GameObject>();
    private List<GameObject> PreviewsRemove = new List<GameObject>();

    protected bool UseDestructionMode = false;

    public void ReciveMouseClick() {
        var elements = CurrentTetrisBlock.Elements.SelectMany(line => line).Where(e => e != null)
            .Select(e => new { e = e, p = e.Position + PreviewPosition })
            .Where(e => e.p.X >= 0 && e.p.Y >= 0 && e.p.X < MAP_WIDTH && e.p.Y < MAP_LENGTH).ToArray();
        if (!UseDestructionMode) {
            foreach (var to in elements) {
                if (Map.ContainsKey(to.p.Y) && Map[to.p.Y].XCells[to.p.X].ZDepths[0] != null) { // То удаление
                    DestroyElementOnMap(Map[to.p.Y].XCells[to.p.X].ZDepths[0]);
                    StoredMap[to.p.Y][0][to.p.X] = ' ';
                } else {
                    StoredMap[to.p.Y][0][to.p.X] = to.e.SourceLetter;
                }
            }
        } else {
            var used = elements.Where(target => Map.ContainsKey(target.p.Y) && Map[target.p.Y].XCells[target.p.X].ZDepths[0] != null).ToArray();
            Debug.Log(used.Count());
            if (used.Any()) { // Если хотя бы одно препятствие, то удаление
                used.Each(box => DestroyElementOnMap(Map[box.p.Y].XCells[box.p.X].ZDepths[0]));
                elements.Each(box => StoredMap[box.p.Y][0][box.p.X] = ' ');
            } else { // Если нет препятствий, то добавление
                elements.Each(box => StoredMap[box.p.Y][0][box.p.X] = box.e.SourceLetter);
            }
        }
        createTetrisBlock();
        ShowPreview = false;
    }
    public void ReciveMouseMove(Vector2 delta) {
        ShowPreview = true;
        PointerOnScreenPosition += delta * 0.02f;
        PointerOnScreenPosition = Position.Between(PointerOnScreenPosition, -AvailableSize, AvailableSize);
        CorrectTetrisPosition();
        CurrentTetrisBlock.transform.position = new Vector3(0, 4f, -4) + new Vector3(PointerOnScreenPosition.x, PointerOnScreenPosition.y) / 4;
    }
    private void CorrectTetrisPosition() {
        var onMap = WorldToMap(PointerOnScreenPosition + CurrentMapShift * Vector2.up * ElementSize.y);
        if (onMap.x < 0) {
            PreviewPosition.X = 0;
            PointerOnScreenPosition.x = MapToWorld(PreviewPosition).x;
        } else {
            var max = MAP_WIDTH - CurrentTetrisBlock.Elements.Max(line => line.Count);
            if (onMap.x > max) {
                PreviewPosition.X = max;
                PointerOnScreenPosition.x = MapToWorld(PreviewPosition).x;
            }
        }
    }
    private void DrawTetris() {
        if (ShowPreview) {
            // Теперь рисуем превьюв
            List<GameObject> Add = new List<GameObject>();
            List<GameObject> Remove = new List<GameObject>();
            for (var y = 0; y < CurrentTetrisBlock.Elements.Count; y++)
                for (var x = 0; x < CurrentTetrisBlock.Elements[y].Count; x++)
                    if (CurrentTetrisBlock.Elements[y][x] != null) {
                        var xOnMap = x + PreviewPosition.X;
                        var yOnMap = y + PreviewPosition.Y;
                        if (xOnMap < MAP_WIDTH && yOnMap < MAP_LENGTH)
                            if (Map.ContainsKey(yOnMap) && Map[yOnMap].XCells[xOnMap].ZDepths[0] != null) { // Значит будет удаление
                                GameObject removeMark;
                                if (PreviewsRemove.Count > 0) {
                                    removeMark = PreviewsRemove[0];
                                    PreviewsRemove.RemoveAt(0);
                                } else {
                                    removeMark = GameObject.Instantiate(Resources.Load("RemoveBlock")) as GameObject;
                                    removeMark.transform.parent = this.Prediction.transform;
                                    removeMark.transform.localScale = Vector3.one;
                                }
                                Remove.Add(removeMark);
                                removeMark.transform.localPosition = MapToWorld(PreviewPosition + new Position(x, y, 2));
                            } else { // Значит будет добавление
                                GameObject addMark;
                                if (PreviewsAdd.Count > 0) {
                                    addMark = PreviewsAdd[0];
                                    PreviewsAdd.RemoveAt(0);
                                } else {
                                    addMark = GameObject.Instantiate(Resources.Load("AddBlock")) as GameObject;
                                    addMark.transform.parent = this.Prediction.transform;
                                    addMark.transform.localScale = Vector3.one;
                                }
                                Add.Add(addMark);
                                addMark.transform.localPosition = MapToWorld(PreviewPosition + new Position(x, y, 2));
                            }
                    }

            PreviewsAdd.Each(go => Destroy(go));
            PreviewsRemove.Each(go => Destroy(go));
            PreviewsRemove = Remove;
            PreviewsAdd = Add;
            var all = PreviewsAdd.Concat(PreviewsRemove);
            var lines = Prediction.GetComponentsInChildren<LineRenderer>();
            lines[0].enabled = true;
            lines[0].SetPosition(0, all.MinElement(go => go.transform.localPosition.x * 10 + go.transform.localPosition.y).transform.position + Vector3.left * ElementSize.x * transform.localScale.x);
            lines[0].SetPosition(1, CurrentTetrisBlock.GuidePoints[0].transform.position + Vector3.left * ElementSize.x * CurrentTetrisBlock.GuidePoints[0].transform.lossyScale.x);
            lines[1].enabled = true;
            lines[1].SetPosition(0, all.MaxElement(go => go.transform.localPosition.x * 10 + go.transform.localPosition.y).transform.position + Vector3.right * ElementSize.x * transform.localScale.x);
            lines[1].SetPosition(1, CurrentTetrisBlock.GuidePoints[1].transform.position + Vector3.right * ElementSize.x * CurrentTetrisBlock.GuidePoints[0].transform.lossyScale.x);
        } else {
            PreviewsAdd.Each(go => Destroy(go));
            PreviewsRemove.Each(go => Destroy(go));
            PreviewsAdd.Clear();
            PreviewsRemove.Clear();
            var lines = Prediction.GetComponentsInChildren<LineRenderer>();
            lines[0].enabled = false;
            lines[1].enabled = false;
        }
    }
    private void createTetrisBlock() {
        var block = Tetrised[UnityEngine.Random.Range(0, Tetrised.Count)];
        if (CurrentTetrisBlock != null)
            Destroy(CurrentTetrisBlock.gameObject);
        if (NextTetrisBlock != null) {
            CurrentTetrisBlock = NextTetrisBlock;
            CurrentTetrisBlock.name = "CurrentTetrisBlock";
            CurrentTetrisBlock.transform.position = new Vector3(0, 4f, -4);
            CurrentTetrisBlock.transform.localScale = Vector3.one / 2;
        }
        var go = new GameObject() { name = "NextTetrisBlock" };
        NextTetrisBlock = go.AddComponent<DropedMapParts>();
        NextTetrisBlock.transform.parent = this.TetrisBlocks.transform;
        NextTetrisBlock.transform.localScale = Vector3.one / 3;
        NextTetrisBlock.transform.position = new Vector3(-1.5f, 4f, -4);
        NextTetrisBlock.SetElements(block);
    }


    #endregion
    public static Position Target(Position position, OrientedSheepStrategy.Direction direction) {
        return position + (direction == OrientedSheepStrategy.Direction.Right ? new Position(+1, 0) : new Position(-1, +1));
    }
    public bool InField(Position target) {
        return target.X >= 0 && target.X < MAP_WIDTH && target.Y < MAP_LENGTH && target.Y >= 0;
    }
}
