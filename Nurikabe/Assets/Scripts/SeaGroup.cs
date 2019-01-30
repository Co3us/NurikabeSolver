using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SeaGroup 
{
    public List<Vector2> locations = new List<Vector2>();
    public int label;
    public SeaGroup() { }
    public SeaGroup(List<Vector2> _locations, int _label)
    {
        locations = new List<Vector2>(_locations);
        label = _label;
    }
}
