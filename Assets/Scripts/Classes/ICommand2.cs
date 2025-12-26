using UnityEngine;

public interface ICommand2<T, U>
{
    public abstract void Execute(T param1, U param2);
}
