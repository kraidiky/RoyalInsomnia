using UnityEngine;
using System.Collections;

public class MapElement : MonoBehaviour {
    /// <summary> Визуальный размер элемента </summary>
    public Vector2 Size;
    /// <summary> Положение данного элемента на карте </summary>
    public Position Position;
    /// <summary> Для независимой настройки Z </summary>
    public int Z = 0;
    public char SourceLetter;

    public MapControler SlowGetMapControler() {
        return null;
    }
    
    void Awake() {
        //Debug.Log(gameObject.name + ".Awake()"); // Загрузилося
    }
    void OnEnable() {
        //Debug.Log(gameObject.name + ".OnEnable()"); // Активировался объект
    }
	// Use this for initialization
    void Start () {
        //Debug.Log(gameObject.name + ".Start() // " + transform.position + " " + transform.localPosition); // Перед первый UPDATE
    }
    // Update is called once per frame
    void Update () {
        //Debug.Log(gameObject.name + ".Update() // " + transform.position + " " + transform.localPosition);
    }
    void OnDisable() {
        //Debug.Log(gameObject.name + ".OnDisable()"); // Деактивировался объект, включая перед перекомпиляцией
    }
    void OnDestroy() {
        //Debug.Log(gameObject.name + ".OnDestroy()"); // Перед уничтожением
    }
}
