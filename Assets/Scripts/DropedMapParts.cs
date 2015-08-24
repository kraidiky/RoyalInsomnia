using UnityEngine;
using System.Collections.Generic;
using UniLinq;

public class DropedMapParts : MonoBehaviour {
    // Этот элемент содержи описание элемента тетриса, который можно ронять.
    public List<List<MapElement>> Elements;
    public List<MapElement> GuidePoints = new List<MapElement>() { null, null };
    public GameObject Target;
    public List<List<GameObject>> Masks;

    public void SetElements(List<List<char>> map) {
        var controller = GameObject.FindObjectOfType<MapControler>();
        Elements = new List<List<MapElement>>();
        for (var y = 0; y < map.Count; y++) {
            Elements.Add(new List<MapElement>());
            for (var x = 0; x < map[y].Count; x++) {
                var element = controller.ElementsFactory(map[y][x], x, y, 0);
                Elements[y].Add(element);
                if (element != null) {
                    element.transform.parent = this.transform;
                    var ElementSize = controller.ElementSize;
                    element.transform.localScale = new Vector3(ElementSize.x / element.Size.x, ElementSize.y / element.Size.y);
                }
            }
        }
        PlaceElements();
    }
    public void RotateElements() {
        var sizeX = Elements.Count + Elements.Max(line => line.Count);
        var sizeY = sizeX - 1;
        List<List<MapElement>> rotated = Enumerable.Range(0, sizeY).Select(i => new MapElement[sizeX].ToList()).ToList();
        for (var y = 0; y < Elements.Count; y++)
            for (var x = 0; x < Elements[y].Count; x++)
                if (Elements[y][x] != null) {
                    var p = Elements[y][x].Position = new Position(sizeX - 1, 0) + new Position(-1, +1) * x + new Position(-2, +1) * y;
                    rotated[p.Y][p.X] = Elements[y][x];
                }
        // Теперь триммировать этот массивище.
        while (rotated.First().All(e => e == null))
            rotated.RemoveAt(0);
        while (rotated.Last().All(e => e == null))
            rotated.RemoveAt(rotated.Count - 1);
        while (rotated.All(line => line.First() == null))
            rotated.ForEach(line => line.RemoveAt(0));
        while (rotated.All(line => line.Last() == null))
            rotated.ForEach(line => line.RemoveAt(line.Count - 1));
        rotated.Each((line, y) => line.Each((element, x) => { if (element != null) element.Position = new Position(x, y); }));
        Elements = rotated;
        PlaceElements();
    }
    private void PlaceElements() {
        var maxCount = Elements.Max(line => line.Count);
        Elements.SelectMany(line => line).Where(element => element != null).ToList().ForEach(e => e.transform.localPosition = Coordinate(e.Position.X, e.Position.Y, maxCount));
        GuidePoints[0] = Elements.SelectMany(line => line).Where(e => e != null).MinElement(element => (element.Position.X ) * 10 + element.Position.Y);
        GuidePoints[1] = Elements.SelectMany(line => line).Where(e => e != null).MaxElement(element => (element.Position.X ) * 10 - element.Position.Y);
    }
    public Vector3 Coordinate(Position p, int width) {
        return Coordinate(p.X, p.Y, width);
    }
    public Vector3 Coordinate(int x, int y, int width) {
        var controller = GameObject.FindObjectOfType<MapControler>();
        var center = (width - 1) / 2f;
        return new Vector3(
                        (x - center) * controller.ElementSize.x,
                        (y * 2 + x - center) * controller.ElementSize.y,
                        (x + width * y) * controller.ElementSize.z);
    }
}
