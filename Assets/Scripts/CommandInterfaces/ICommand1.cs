using UnityEngine;

public interface ICommand1<T>
{
    public abstract void Execute(T param);
}
