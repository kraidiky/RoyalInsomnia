using UnityEngine;
using System.Collections.Generic;

public class MapBlockModel {
    public GameObject GO;
    public char AliasInStorage;
    public string PrefabName;

    public virtual void CreatePresentation() {
        GO = GameObject.Instantiate(Resources.Load(PrefabName)) as GameObject;
        
    }
    public virtual void DestroyPresentation() {
        GameObject.Destroy(GO);
        GO = null;
    }
    /// <summary> Если элемент часть тетрисовой фигуры, то тут будет сылка </summary>
    public TetrisElementModel ParentTetrisElement;
    public bool Walkable { get; set; }


    public virtual MapBlockModel Clone() {
        return new MapBlockModel() { Walkable = Walkable, AliasInStorage = AliasInStorage, PrefabName = PrefabName };
    }

    public virtual bool FieldElement { get { return true; } }
}
public class NullBlockModel : MapBlockModel {
    private NullBlockModel() {
        Walkable = false;
        AliasInStorage = ' ';
    }
    private static NullBlockModel _instance;
    public static NullBlockModel Instance { get { return _instance != null ? _instance : _instance = new NullBlockModel(); } }
    public override void CreatePresentation() { } // do nothing
    public override void DestroyPresentation() { } // do nothing
    public override MapBlockModel Clone() {
        return this;
    }
    override public bool FieldElement { get { return false; } }
}
/// <summary> Озхначает, что над данным блоком сверху сейчас движется элемент, поэтому оперировать с ним нельзя. </summary>
public class MovementBlockModel : MapBlockModel {
    private MovementBlockModel() {
        Walkable = false;
        AliasInStorage = '>';
    }
    private static MovementBlockModel _instance;
    public static MovementBlockModel Instance { get { return _instance != null ? _instance : _instance = new MovementBlockModel(); } }
    public override void CreatePresentation() { } // do nothing
    public override void DestroyPresentation() { } // do nothing
    public override MapBlockModel Clone() {
        return this;
    }
    override public bool FieldElement { get { return false; } }
}
