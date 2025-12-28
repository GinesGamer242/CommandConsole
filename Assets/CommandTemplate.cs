using UnityEngine;

public class CommandTemplate : ICommand1<int>, ICommand0, ICommand2<string, int>
{
    public void Execute(int param)
    {
        Debug.Log($"THIS TEMPLATE EXECUTES WITH 1 PARAM: {param}");
    }

    public void Execute()
    {
        Debug.Log("THIS TEMPLATE EXECUTES WITH 0 PARAMS");
    }

    public void Execute(string param1, int param2)
    {
        Debug.Log($"THIS TEMPLATE EXECUTES WITH 2 PARAMS: {param1}, {param2}");
    }
}
