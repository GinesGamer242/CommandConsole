using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public enum ParameterType
{
    NONE,
    INT,
    FLOAT,
    DOUBLE,
    BOOL,
    STRING
}

[CreateAssetMenu(fileName = "CommandSO", menuName = "Scriptable Objects/CommandSO")]
public class CommandSO : ScriptableObject
{
    public int m_ID;
    public string m_Key;
    public MonoScript m_Method;
}
