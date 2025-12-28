using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "CommandSO", menuName = "Scriptable Objects/CommandSO")]
public class CommandSO : ScriptableObject
{
    public string m_Keyword;
    public MonoScript m_Method;
    [Tooltip("The command method must have no parameters to be accesible via shortcut")]
    public Key m_ShortcutKey;
}
