using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

abstract public class SheepStrategy {
    //public SheepStrategy() { Debug.Log("new " + GetType().Name + "()"); }
    public abstract string PrefabAlias { get; }
    public GameObject Prefab { get; set; }
    abstract public void Update(MapControler controller);

    public virtual void CreateRequiredPrefab(MapControler controller) {
        if (Prefab == null) {
            Prefab = GameObject.Instantiate(Resources.Load(PrefabAlias)) as GameObject;
            Prefab.transform.parent = controller.transform;
            Prefab.transform.localPosition = new Vector3(0, 0, -1);
        }
    }
    public void DestroyPrefab() {
        if (Prefab != null) {
            GameObject.Destroy(Prefab);
            Prefab = null;
        }
    }
    protected bool InMap(MapControler controller, Position position) {
        return 0 <= position.X && position.X < controller.MAP_WIDTH
                && 0 <= position.Y && position.Y < controller.MAP_LENGTH;
    }
}

public class SheepCrashStrategy : SheepStrategy {
    public float CrashTime = .5f;
    public Position CrashPosition;
    public override string PrefabAlias { get { return "SheepCrashMove"; } }

    public override void Update(MapControler controller) {
        CrashTime -= Time.deltaTime;
        if (CrashTime > 0) {
            SetPosition(controller);
        } else {
            // Достаточно провалялись лапками кверху. Пошли создавать овцу в новом месте.
            DestroyPrefab();
            controller.Repalce(this, controller.PlaceNextEmptyPosition(controller.CalculateYPosition(CrashPosition)));
        }
    }
    public override void CreateRequiredPrefab(MapControler controller) {
        base.CreateRequiredPrefab(controller);
        SetPosition(controller);
        Prefab.transform.localScale = Vector3.one;
    }
    private void SetPosition(MapControler controller) {
        Prefab.transform.localPosition = controller.MapToWorld(CrashPosition, .5f);
    }
}

/// <summary> Класс также используется в качестве пассивного хранилища данных. Даздравствует говнокодий. </summary>
public class OrientedSheepStrategy : SheepStrategy {
    public enum Direction { Left, Right }
    public Direction MoveTo;
    public Position Position;
    public override string PrefabAlias { get { return MoveTo == Direction.Left ? "SheepLeftMove" : "SheepRightMove"; } }

    public override void Update(MapControler controller) { throw new NotImplementedException(); }

    /// <summary> Создаёт новый Waiting если ей есть за кем пристроиться или новый Move если нету. </summary>
    public static OrientedSheepStrategy CreateNew(MapControler controller) {
        if (controller.Sheeps.OfType<SheepMoveStrategy>().Any()) { // Если есть ещё хотя бы одна ползущая
            var last = controller.Sheeps.OfType<SheepMoveStrategy>().MinElement(sheep => controller.CalculateYPosition(sheep.Position));
            var waiting = new WaitingSheepStrategy() { MoveTo = last.MoveTo, Position = last.Position };
            waiting.CreateRequiredPrefab(controller);
            controller.Sheeps.Add(waiting);
            return waiting;
        } else { // Если нет ни одной ползущей - искать для новой место.
            var from = controller.Sheeps.OfType<SheepCrashStrategy>().Any() ?
                controller.Sheeps.OfType<SheepCrashStrategy>().Min(sheep => controller.CalculateYPosition(sheep.CrashPosition)) : // Если ест дохлые считать начиная от них.
                controller.Sheeps.OfType<WaitingSheepStrategy>().Any() ?
                    controller.Sheeps.OfType<WaitingSheepStrategy>().Min(sheep => controller.CalculateYPosition(sheep.Position)) : // Если хотя бы ждущие, то от ждущих
                    controller.CurrentMapShift + controller.targetSheepShift; // Если нету никаких, то откладываем от желаемой коодинаты.
            var finded = controller.FindNextEmptyPosition(from - controller.MAP_WIDTH/2f);
            if (finded != null) {
                var moving = new SheepMoveStrategy() {
                    MoveTo = finded.MoveTo,
                    Position = finded.Position,
                    ToPosition = MapControler.Target(finded.Position, finded.MoveTo),
                    SheepShift = controller.CalculateYPosition(finded.Position) - controller.CurrentMapShift
                };
                moving.CreateRequiredPrefab(controller);
                controller.Sheeps.Add(moving);
                return moving;
            }
        }
        return null;
    }
}

public class SheepMoveStrategy : OrientedSheepStrategy {
    public Position ToPosition;
    public float SheepShift;
    public override void Update(MapControler controller) {
        if (SheepShift > controller.targetSheepShift) {
            SheepShift = Mathf.Max(SheepShift - controller.MapSpeed * Time.deltaTime * controller.sheepAcceleration, controller.targetSheepShift);
        } else if (SheepShift < controller.targetSheepShift) {
            SheepShift = Mathf.Min(SheepShift + controller.MapSpeed * Time.deltaTime * controller.sheepAcceleration, controller.targetSheepShift);
        }
        // Определение не достигла ли овца целевой позиции
        if (controller.CalculateYPosition(ToPosition) < controller.CurrentMapShift + SheepShift) {
            Interaction(controller, ToPosition);
            if (controller.UseSheepWayDetection) {
                // очень лень мне было по нормальному писать :(
                List<Position[]> ways = new List<Position[]>(), newWays = new List<Position[]>() { new Position[] { ToPosition } };
                while (newWays.Count > 0) {
                    ways = newWays;
                    newWays = new List<Position[]>();
                    foreach (var way in ways) {
                        var right = MapControler.Target(way.Last(), Direction.Right);
                        var left = MapControler.Target(way.Last(), Direction.Left);
                        try {
                            if (InMap(controller, right) && controller.Possible.Contains(controller.StoredMap[right.Y][0][right.X]))
                                newWays.Add(way.Concat(new[] { right }).ToArray());
                            if (InMap(controller, left) && controller.Possible.Contains(controller.StoredMap[left.Y][0][left.X]))
                                newWays.Add(way.Concat(new[] { left }).ToArray());
                        } catch {
                            Debug.Log(left + " " + right);
                        }
                    }
                }
                if (ways[0].Length > 1) {
                    var rnd = ways[(new System.Random()).Next(newWays.Count)];
                    var direction = rnd[1] == MapControler.Target(rnd[0], Direction.Right) ? Direction.Right : Direction.Left;
                    if (direction == MoveTo) { // Значит продолжаем в ту же сторону
                        controller.Repalce(this, new SheepMoveStrategy() { Position = rnd[0], ToPosition = rnd[1], MoveTo = direction, Prefab = Prefab, SheepShift = SheepShift });
                        return;
                    } else { // Значит поворачиваем
                        DestroyPrefab();
                        var strategy = new SheepMoveStrategy() {Position = rnd[0], ToPosition = rnd[1], MoveTo = direction, SheepShift = SheepShift};
                        strategy.CreateRequiredPrefab(controller);
                        controller.Repalce(this, strategy);
                        return;
                    }
                }
            }
            // Первая проверка - можно ли двигаться дальше вперёд?
            var nextPosition = ToPosition + (ToPosition - Position);
            if (InMap(controller, nextPosition) && controller.Possible.Contains(controller.StoredMap[nextPosition.Y][nextPosition.Z][nextPosition.X])) {
                // тогда продолжать в том же направлении.
                controller.Repalce(this, new SheepMoveStrategy() { Position = ToPosition, ToPosition = nextPosition, MoveTo = MoveTo, Prefab = Prefab, SheepShift = SheepShift });
                return;
            }
            // Если нет, то можно ли развернуться?
            var differentDirection = MoveTo == Direction.Left ? Direction.Right : Direction.Left;
            var differentPosition = MapControler.Target(ToPosition, differentDirection);
            if (InMap(controller, differentPosition) && controller.Possible.Contains(controller.StoredMap[differentPosition.Y][differentPosition.Z][differentPosition.X])) {
                // тогда Менять направление движения
                DestroyPrefab();
                var strategy = new SheepMoveStrategy() {
                    Position = ToPosition,
                    ToPosition = differentPosition,
                    MoveTo = differentDirection,
                    SheepShift = SheepShift
                };
                strategy.CreateRequiredPrefab(controller);
                controller.Repalce(this, strategy);
                return;
            }
            // Иначе накрыться медным тазом.
            DestroyPrefab();
            var crash = new SheepCrashStrategy() { CrashPosition = ToPosition };
            crash.CreateRequiredPrefab(controller);
            controller.Repalce(this, crash);
        } else { // Если нет - просто двигаем и всё.
            SetPosition(controller);
        }
    }
    public override void CreateRequiredPrefab(MapControler controller) {
        base.CreateRequiredPrefab(controller);
        SetPosition(controller);
        Prefab.transform.localScale = Vector3.one;
    }
    private void SetPosition(MapControler controller) {
        var finishLine = controller.CalculateYPosition(ToPosition);
        var startLine = controller.CalculateYPosition(Position);
        var proportion = (controller.CurrentMapShift + SheepShift - startLine) / (finishLine - startLine);
        Prefab.transform.localPosition = Vector3.Lerp(controller.MapToWorld(Position), controller.MapToWorld(ToPosition), proportion) + Vector3.back * 3;
        Prefab.transform.localPosition += Vector3.up * Mathf.Abs(0.25f * Mathf.Sin(((controller.CurrentMapShift + SheepShift) * 4) % 1 * Mathf.PI));
    }
    private void Interaction(MapControler controller, Position position) {
        for (var z = 0; z < controller.MAP_HEIGHT; z++)
            if (controller.Interactions.ContainsKey(controller.StoredMap[position.Y][z][position.X])) {
                var element = controller.Map[position.Y].XCells[position.X].ZDepths[z];
                //Debug.Log("element:"+element);
                controller.UnregisterMapElement(element);
                controller.StoredMap[position.Y][z][position.X] = controller.Interactions[controller.StoredMap[position.Y][z][position.X]];
                UnityEngine.Object.Destroy(element.gameObject);
            }
    }
}

public class WaitingSheepStrategy : OrientedSheepStrategy {
    public float Delay = .5f;
    public float CurrentDelay = .5f;
    protected float rotation = 0;
    public override void Update(MapControler controller) {
        // Ждём пока в одном позишене с нами есть хотя бы кто-то, потом ждём ещё  пол блока и пошли.
        if (controller.Sheeps.OfType<SheepMoveStrategy>().Any(sheep => sheep.Position == Position)) {
            CurrentDelay = Delay; // Продолжаем меланхолично ждать.
        } else {
            CurrentDelay -= controller.MapSpeed * Time.deltaTime;
        }
        if (CurrentDelay > 0) { // Это нифига не будет работать из-за округлений если Delay == 0;
            Prefab.transform.localPosition = controller.MapToWorld(Position + new Position(0,0,1));
            Prefab.transform.localScale = Vector3.one / 2;
            rotation += controller.MapSpeed * Time.deltaTime;
            Prefab.transform.localRotation = Quaternion.AngleAxis(rotation * 180, Vector3.back);
        } else { // А вот теперь мы вырываемся из цикла
            Prefab.transform.localScale = Vector3.one;
            Prefab.transform.localRotation = new Quaternion();
            var move = new SheepMoveStrategy() {
                Prefab = Prefab,
                Position = Position,
                MoveTo = MoveTo,
                ToPosition = MapControler.Target(Position, MoveTo),
                SheepShift = controller.CalculateYPosition(Position) - controller.CurrentMapShift
            };
            controller.Repalce(this, move);
        }
    }
}