using UnityEngine;
using System;
using System.Collections.Generic;
using UniLinq;

public class TetrisElementModel {
    public Position Position;
    public GameObject GO;
    private MapBlockModel[][] _blocks;
    /// <summary> На самом деле все массивы квадратные, для простоты </summary>
    public MapBlockModel[][] Blocks {
        get { return _blocks; }
        set {
            if (_blocks != value) {
                if (_blocks != null)
                    _blocks.SelectMany(line => line).Where(b => b != null).Each(b => b.ParentTetrisElement = null);
                _blocks = value;
                if (_blocks != null)
                    _blocks.SelectMany(line => line).Where(b => b != null).Each(b => b.ParentTetrisElement = this);
            }
        }
    }

    public TetrisElementModel Clone() {
        return new TetrisElementModel() { Blocks = _blocks.Select(line => line.Select(block => block != null ? block.Clone() : null).ToArray()).ToArray() };
    }
    public void RotateRight() {
        _blocks = _blocks[0].Select((column, y) => _blocks.Reverse().Select((line, x) => line[y]).ToArray()).ToArray();
    }
    public void RotateLeft() {
        _blocks = _blocks[0].Reverse().Select((column, y) => _blocks.Select((line, x) => line[y]).ToArray()).ToArray();
    }
    public void MoveElements() {
        for (var x = 0; x<_blocks.Length; x++)
            for (var y = 0; y< _blocks[0].Length; y++)
                if (_blocks[x][y] != null)
                    _blocks[x][y].GO.transform.localPosition = x * (Vector3.right + Vector3.up / 2) + y * (Vector3.left + Vector3.up / 2);
    }
    /// <summary> Целочисленный </summary>
    public Position Center () { return new Position((_blocks.Length - _blocks[0].Length) / 2, (_blocks.Length + _blocks[0].Length - 1) / 2); }
    /// <summary> Целочисленный </summary>
    public Vector2 CenterVector2 () { return new Vector2((_blocks.Length - _blocks[0].Length) / 2f, (_blocks.Length + _blocks[0].Length - 1) / 2f / 2); } // У сразу приведён
    public virtual void CreatePresentation() {
        GO = new GameObject();
        foreach(var element in _blocks.SelectMany(line => line))
            if (element != null) {
                element.CreatePresentation();
                element.GO.transform.parent = GO.transform;
                element.GO.transform.localScale = Vector3.one;
            }
        MoveElements();
    }
    public virtual void DestroyPresentation() {
        _blocks.SelectMany(line => line).Each(element => { if (element != null) GameObject.Destroy(element.GO); });
        GameObject.Destroy(GO);
    }
}

