using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CommandSO", menuName = "Scriptable Objects/CommandSO")]
public class CommandSO : ScriptableObject
{
    public int ID;
    public string command;
    public UnityEvent behaviour;
}
