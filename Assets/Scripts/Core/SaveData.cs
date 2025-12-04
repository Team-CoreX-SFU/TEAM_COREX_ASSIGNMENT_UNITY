using System;
using UnityEngine;

/// <summary>
/// Serializable data structure to hold rope cut state
/// </summary>
[Serializable]
public class SaveData
{
    // Rope / handcuff cut state
    // When true, the rope has already been cut and should not appear again on load.
    public bool ropeCut;
}

