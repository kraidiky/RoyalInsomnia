using System;
using System.Collections.Generic;
using UniLinq;

public static class LinqStaticExtentions {
    // Эта кучка методов заменяет нам foreach
    /// <summary>Вызвать делегата в отношении каждого экземпляра в последовательности любого типа.</summary>
    /// <param name="source">Перечислитель с исходными данными.</param>
    /// <param name="func">Делегат, который надо вызывать.</param>
    public static IEnumerator<SequenceType> Each<SequenceType>(this IEnumerator<SequenceType> source, Action<SequenceType> func) {
        List<SequenceType> result = new List<SequenceType>();
        while (source.MoveNext())
            result.Add(source.Current);
        for (var i = 0; i < result.Count; i++)
            func(result[i]);
        return result.GetEnumerator();
    }
    /// <summary>Вызвать делегата в отношении каждого экземпляра в последовательности любого типа и передать ему индекс.</summary>
    /// <param name="source">Перечислитель с исходными данными.</param>
    /// <param name="func">Делегат, который надо вызывать. Второй параметр передаваемый делегату - индекс в последовательности. Отсчёт начинается со следующего элемента последовательности. В нетронутой - с начала.</param>
    public static IEnumerator<SequenceType> Each<SequenceType>(this IEnumerator<SequenceType> source, Action<SequenceType, int> func) {
        List<SequenceType> result = new List<SequenceType>();
        while (source.MoveNext())
            result.Add(source.Current);
        for (var i = 0; i < result.Count; i++)
            func(result[i], i);
        return result.GetEnumerator();
    }
    /// <summary>Вызвать делегата в отношении каждого экземпляра в последовательности любого типа.</summary>
    /// <param name="source">Перечислитель с исходными данными.</param>
    /// <param name="func">Делегат, который надо вызывать.</param>
    public static IEnumerable<SequenceType> Each<SequenceType>(this IEnumerable<SequenceType> source, Action<SequenceType> func) {
        var enumerator = source.GetEnumerator();
        var result = new List<SequenceType>();
        while (enumerator.MoveNext())
            result.Add(enumerator.Current);
        for (var i = 0; i < result.Count; i++)
            func.Invoke(result[i]);
        return result;
    }
    /// <summary>Вызвать делегата в отношении каждого экземпляра в последовательности любого типа и передать ему индекс.</summary>
    /// <param name="source">Перечислитель с исходными данными.</param>
    /// <param name="func">Делегат, который надо вызывать. Второй параметр передаваемый делегату - индекс в последовательности. Отсчёт начинается со следующего элемента последовательности. В нетронутой - с начала.</param>
    public static IEnumerable<SequenceType> Each<SequenceType>(this IEnumerable<SequenceType> source, Action<SequenceType, int> func) {
        var enumerator = source.GetEnumerator();
        List<SequenceType> result = new List<SequenceType>();
        while (enumerator.MoveNext())
            result.Add(enumerator.Current);
        for (var i = 0; i < result.Count; i++)
            func(result[i], i);
        return result;
    }

    // Эта кучуа методов добывает доставатель из последовательности элемента с минимальной или максимальной характеристической функцией
    public static SequenceType MinElement<SequenceType>(this IEnumerator<SequenceType> sequence, Func<SequenceType, double> estimate) {
        var selectedElement = sequence.Current;
        var minValue = estimate(selectedElement);
        while (sequence.MoveNext()) {
            var value = estimate(sequence.Current);
            if (value < minValue) {
                selectedElement = sequence.Current;
                minValue = value;
            }
        }
        return selectedElement;
    }
    public static IEnumerable<SequenceType> MinElements<SequenceType>(this IEnumerable<SequenceType> sequence, Func<SequenceType, double> estimate, int count) {
        List<SequenceType> Items = new List<SequenceType>();
        List<double> Values = new List<double>();
        double value, selectedMax = double.MaxValue;
        foreach (var element in sequence)
            if ((value = estimate(element)) > selectedMax || Items.Count < count) {
                for (var i = 0; i <= Items.Count; i++)
                    if (i == Items.Count) {
                        Items.Add(element);
                        Values.Add(value);
                    } else if (Values[i] < value) {
                        Items.Insert(i, element);
                        Values.Insert(i, value);
                    }
                if (Items.Count > count) {
                    Items.RemoveAt(0);
                    Values.RemoveAt(0);
                }
                selectedMax = Values[0];
            }
        return Items;
    }
    public static IEnumerable<SequenceType> MaxElements<SequenceType>(this IEnumerable<SequenceType> sequence, Func<SequenceType, double> estimate, int count) {
        List<SequenceType> Items = new List<SequenceType>();
        List<double> Values = new List<double>();
        double value, selectedMin = double.MinValue;
        foreach (var element in sequence)
            if ((value = estimate(element)) > selectedMin || Items.Count < count) {
                for (var i = 0; i <= Items.Count; i++)
                    if (i == Items.Count) {
                        Items.Add(element);
                        Values.Add(value);
                        break;
                    } else if (Values[i] > value) {
                        Items.Insert(i, element);
                        Values.Insert(i, value);
                        break;
                    }
                if (Items.Count > count) {
                    Items.RemoveAt(0);
                    Values.RemoveAt(0);
                }
                selectedMin = Values[0];
            }
        return Items;
    }
    public static SequenceType MinElement<SequenceType>(this IEnumerator<SequenceType> sequence, Func<SequenceType, int> estimate) {
        var selectedElement = sequence.Current;
        var minValue = estimate(selectedElement);
        while (sequence.MoveNext()) {
            var value = estimate(sequence.Current);
            if (value < minValue) {
                selectedElement = sequence.Current;
                minValue = value;
            }
        }
        return selectedElement;
    }
    public static SequenceType MinElement<SequenceType>(this IEnumerable<SequenceType> sequence, Func<SequenceType, double> estimate) {
        var selectedElement = sequence.First();
        var minValue = estimate(selectedElement);
        foreach (var item in sequence.Skip(1)) {
            var value = estimate(item);
            if (value < minValue) {
                selectedElement = item;
                minValue = value;
            }
        }
        return selectedElement;
    }
    public static SequenceType MinElement<SequenceType>(this IEnumerable<SequenceType> sequence, Func<SequenceType, int> estimate) {
        var selectedElement = sequence.First();
        var minValue = estimate(selectedElement);
        foreach (var item in sequence.Skip(1)) {
            var value = estimate(item);
            if (value < minValue) {
                selectedElement = item;
                minValue = value;
            }
        }
        return selectedElement;
    }
    public static SequenceType MaxElement<SequenceType>(this IEnumerator<SequenceType> sequence, Func<SequenceType, double> estimate) {
        var selectedElement = sequence.Current;
        var minValue = estimate(selectedElement);
        while (sequence.MoveNext()) {
            var value = estimate(sequence.Current);
            if (value > minValue) {
                selectedElement = sequence.Current;
                minValue = value;
            }
        }
        return selectedElement;
    }
    public static SequenceType MaxElement<SequenceType>(this IEnumerator<SequenceType> sequence, Func<SequenceType, int> estimate) {
        var selectedElement = sequence.Current;
        var minValue = estimate(selectedElement);
        while (sequence.MoveNext()) {
            var value = estimate(sequence.Current);
            if (value > minValue) {
                selectedElement = sequence.Current;
                minValue = value;
            }
        }
        return selectedElement;
    }
    public static SequenceType MaxElement<SequenceType>(this IEnumerable<SequenceType> sequence, Func<SequenceType, double> estimate) {
        var selectedElement = sequence.First();
        var minValue = estimate(selectedElement);
        foreach (var item in sequence.Skip(1)) {
            var value = estimate(item);
            if (value > minValue) {
                selectedElement = item;
                minValue = value;
            }
        }
        return selectedElement;
    }
    public static SequenceType MaxElement<SequenceType>(this IEnumerable<SequenceType> sequence, Func<SequenceType, int> estimate) {
        var selectedElement = sequence.First();
        var minValue = estimate(selectedElement);
        foreach (var item in sequence.Skip(1)) {
            var value = estimate(item);
            if (value > minValue) {
                selectedElement = item;
                minValue = value;
            }
        }
        return selectedElement;
    }

    public static string ToJoinedString<SequenceType>(this IEnumerable<SequenceType> sequence, string separator =",") {
        return sequence.Aggregate("", (a, e) => a + (a != "" ? separator : "") + e.ToString());
    }
    public static string ToJoinedString<SequenceType>(this IEnumerable<SequenceType> sequence, Func<SequenceType, object> translator, string separator = ",") {
        return sequence.Aggregate("", (a, e) => a + (a != "" ? separator : "") + translator(e).ToString());
    }
    public static string ToJoinedString<SequenceType>(this IEnumerable<SequenceType> sequence, Func<SequenceType, int, object> translator, string separator = ",") {
        string msg = "";
        int index = 0;
        foreach (SequenceType item in sequence)
            msg += (msg != "" ? separator : "") + translator(item, index++).ToString();
        return msg;
    }
}
