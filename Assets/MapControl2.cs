using UnityEngine;
using System.Collections.Generic;

public class MapControl2 : MonoBehaviour {
    protected Dictionary<int, Dictionary<int, MapBlockModel>> MapStorage = new Dictionary<int, Dictionary<int, MapBlockModel>>();
    public MapBlockModel Map(int x, int y) {
        if (MapStorage.ContainsKey(x) && MapStorage[x].ContainsKey(y))
            return MapStorage[x][y];
        return NullBlockModel.Instance;
    }
    public void Map(int x, int y, MapBlockModel block) {
        if (!MapStorage.ContainsKey(x))
            MapStorage.Add(x, new Dictionary<int, MapBlockModel>());
        if (!MapStorage[x].ContainsKey(y)) {
            MapStorage[x][y].DestroyPresentation();
            MapStorage[x][y] = block;
        } else {
            MapStorage[x].Add(y, block);
        }
    }
    private void LoadMapFromFile() { }
    private void SaveMapToFile() { }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
