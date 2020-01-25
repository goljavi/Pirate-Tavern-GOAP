using System.Collections.Generic;
using System;

public class Preferences<T>
{
    List<Func<T, bool>> _preferences;

    public Preferences()
    {
        _preferences = new List<Func<T, bool>>();
    }

    public void Add(Func<T, bool> func) => _preferences.Add(func);

    public bool Satisfies(T state) => _preferences.TrueForAll(x => x(state));

    public static Preferences<T> operator +(Preferences<T> a, Func<T, bool> b)
    {
        a.Add(b);
        return a;
    }
}
