using UnityEngine;
using System.Collections;

public class MapBlockModel {
    public virtual void DestroyPresentation() {
    }
    /// <summary> Если элемент часть тетрисовой фигуры, то тут будет сылка </summary>
    public TetrisElementModel ParentTetrisElement;

}
public class NullBlockModel : MapBlockModel {
    private NullBlockModel() { }
    private static NullBlockModel _instance;
    public static NullBlockModel Instance { get { return _instance != null ? _instance : _instance = new NullBlockModel(); } }
    public override void DestroyPresentation() { } // do nothing
}
