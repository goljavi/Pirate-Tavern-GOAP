using System.Collections.Generic;
using System;

public class Preferences<T>
{
    public List<Func<T, bool>> PreferencesList;

    public Preferences()
    {
        PreferencesList = new List<Func<T, bool>>();
    }

    public void Add(Func<T, bool> func) => PreferencesList.Add(func);

    public bool Satisfies(T state) => PreferencesList.TrueForAll(x => x(state));

    public int SatisfiesQuantity(T state)
    {
        int qty = 0;
        foreach (var preference in PreferencesList)
            if (preference(state)) qty++;

        return qty;
    }

    public static Preferences<T> operator +(Preferences<T> a, Func<T, bool> b)
    {
        a.Add(b);
        return a;
    }
}
