using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ConsoleManager : MonoBehaviour
{
    [SerializeField]
    float m_AnimationDuration;

    [Header("References")]
    [SerializeField]
    TMP_InputField m_ConsoleInputField;
    [SerializeField]
    Animator m_ConsoleAnimator;


    CommandSO[] m_CommandList;
    int m_ParameterLimit = 2;

    private void Start()
    {
        m_CommandList = Resources.FindObjectsOfTypeAll<CommandSO>();
    }

    public void OnCommandEnter(string input)
    {
        m_ConsoleInputField.text = "";
        string[] inputtedParts = input.Split(' ');

        if (inputtedParts.Length == 0 || inputtedParts.Length > m_ParameterLimit + 1)
        {
            Debug.LogWarning("Command length is invalid.");
            return;
        }

        string inputtedKey = inputtedParts[0];
        List<string> inputtedParameters = new List<string>();

        for (int i=1; i < inputtedParts.Length; i++)
        {
            inputtedParameters.Add(inputtedParts[i]);
        }

        CommandSO usedCommand = null;

        foreach (CommandSO command in m_CommandList)
        {
            if (inputtedKey.Equals(command.m_Key))
                usedCommand = command;
        }

        if (usedCommand == null)
        {
            Debug.LogWarning("Inputted command could not be found.");
            return;
        }

        Type usedInterface = null;

        switch (inputtedParameters.Count)
        {
            case 0:
                usedInterface = usedCommand.m_Method.GetClass().GetInterface("ICommand0");
                break;
            case 1:
                usedInterface = usedCommand.m_Method.GetClass().GetInterface("ICommand1`1");
                break;
            case 2:
                usedInterface = usedCommand.m_Method.GetClass().GetInterface("ICommand2`2");
                break;
            default:
                Debug.Log("Written parameters and command parameters don't match.");
                return;
        }

        if (usedInterface == null)
        {
            Debug.LogWarning("Method for inputted command could not be found.");
            return;
        }

        Type[] parameterTypes = usedInterface.GetGenericArguments();

        object[] args = new object[parameterTypes.Length];

        for (int i = 0; i < parameterTypes.Length; i++)
        {
            try
            {
                args[i] = Convert.ChangeType(inputtedParameters[i], parameterTypes[i]);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }
        }

        object instance = Activator.CreateInstance(usedCommand.m_Method.GetClass());

        MethodInfo executeMethod = usedInterface.GetMethod("Execute");
        executeMethod.Invoke(instance, args);
    }

    public void ToggleConsole()
    {
        if (m_ConsoleAnimator.GetBool("OpenConsole"))
        {
            m_ConsoleAnimator.SetBool("OpenConsole", false);
            m_ConsoleInputField.text = "";
            //StartCoroutine(AnimationCoroutine(false));
        }
        else
        {
            m_ConsoleAnimator.SetBool("OpenConsole", true);
            m_ConsoleInputField.text = "";
            //StartCoroutine(AnimationCoroutine(true));
        }
    }

    IEnumerator AnimationCoroutine(bool activatingConsole)
    {
        yield return new WaitForSeconds(m_AnimationDuration);
        m_ConsoleAnimator.gameObject.SetActive(activatingConsole);
    }
}
