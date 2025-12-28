using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.GPUSort;
using static UnityEditor.Rendering.CameraUI;

public class ConsoleManager : MonoBehaviour
{
    [SerializeField]
    int m_RecentInputsListLimit;

    [Header("References")]
    [SerializeField]
    PlayerInput m_PlayerInput;
    [SerializeField]
    TMP_InputField m_ConsoleInputField;
    [SerializeField]
    GameObject m_UnityConsolePanel;
    [SerializeField]
    TextMeshProUGUI m_UnityConsoleText;
    [SerializeField]
    TextMeshProUGUI m_AutocompleteText;
    //[SerializeField]
    //Animator m_ConsoleAnimator;

    List<CommandSO> m_CommandList = new List<CommandSO>();
    List<string> m_RecentInputs = new List<string>();
    int m_CurrentInputIndex = 999;
    int m_ParameterLimit = 2;
    bool m_IsConsoleOpen;

    string m_Output = "";
    string m_Stack = "";
    string m_UnityLog = "";

    private void Start()
    {
        OnCloseConsoles();

        List<CommandSO> unorderedCommandList = new List<CommandSO>();

        // OBTAIN COMMAND LIST LOADING THE SCRIPTABLE OBJECTS FROM THE ASSETS
        string[] commandAssetsGUID = AssetDatabase.FindAssets("t:CommandSO");

        foreach (string assetGUID in commandAssetsGUID)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            unorderedCommandList.Add(AssetDatabase.LoadAssetAtPath<CommandSO>(assetPath));
        }

        // SORT LIST BY KEYWORD LENGTH (SHORTEST TO LONGEST)
        m_CommandList = unorderedCommandList.OrderBy(command => command.m_Keyword).ToList();
        m_CommandList.Reverse();
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        m_Output = logString;
        m_Stack = stackTrace;

        m_UnityLog += $"> {m_Output}\n";
    }

    private void Update()
    {
        // CHECK IF A COMMAND SHORTCUT HAS BEEN PRESEED
        foreach (CommandSO command in m_CommandList)
        {
            if (command.m_ShortcutKey != Key.None &&
                Keyboard.current[command.m_ShortcutKey].wasPressedThisFrame)
            {
                OnShortcutKeyPressed(command);
            }
        }

        // UPDATE UNITY'S CONSOLE LOG
        m_UnityConsoleText.text = m_UnityLog;
    }

    public void OnCommandEnter(string input)
    {
        if (!m_IsConsoleOpen)
            return;

        m_ConsoleInputField.text = "";
        m_ConsoleInputField.Select();

        string[] inputtedParts = input.Split(' ');

        if (inputtedParts.Length == 0 || inputtedParts.Length > m_ParameterLimit + 1)
        {
            Debug.LogWarning("Command length is invalid.");
            return;
        }

        string inputtedKeyword = inputtedParts[0];
        List<string> inputtedParameters = new List<string>();

        for (int i=1; i < inputtedParts.Length; i++)
        {
            inputtedParameters.Add(inputtedParts[i]);
        }

        CommandSO usedCommand = null;

        foreach (CommandSO command in m_CommandList)
        {
            if (inputtedKeyword.Equals(command.m_Keyword))
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

        // SAVE THE INPUTTED COMMAND TO BE ACCESIBLE WITH SHORTCUTS
        if (!m_RecentInputs.Contains(input))
        {
            if (m_RecentInputs.Count < m_RecentInputsListLimit)
            {
                m_RecentInputs.Add(input);
            }
            else
            {
                m_RecentInputs.RemoveAt(0);
                m_RecentInputs.Add(input);
            }
        }

        m_CurrentInputIndex = 999;
    }

    public void ShowAutocomplete()
    {
        if (m_ConsoleInputField.text.Length <= 0)
        {
            m_AutocompleteText.text = "";
            return;
        }

        foreach (CommandSO command in m_CommandList)
        {
            if (m_ConsoleInputField.text.Length <= command.m_Keyword.Length)
            {
                if (command.m_Keyword.Substring(0, m_ConsoleInputField.text.Length).Equals(m_ConsoleInputField.text))
                {
                    m_AutocompleteText.text = command.m_Keyword;
                    return;
                }
            }
            
        }

        m_AutocompleteText.text = "";
    }

    public void ConfirmAutocomplete()
    {
        if (m_AutocompleteText.text.Length <= 0)
            return;

        m_ConsoleInputField.text = m_AutocompleteText.text;
        m_ConsoleInputField.MoveToEndOfLine(false, false);
        m_AutocompleteText.text = "";
    }

    public void OnShortcutKeyPressed(CommandSO commandPressed)
    {
        if (commandPressed.m_Method.GetClass().GetInterface("ICommand0") != null)
        {
            object instance = Activator.CreateInstance(commandPressed.m_Method.GetClass());

            MethodInfo executeMethod = commandPressed.m_Method.GetClass().GetInterface("ICommand0").GetMethod("Execute");
            object[] args = new object[0];

            executeMethod.Invoke(instance, args);
        }
        else
        {
            Debug.LogWarning("Shortcuted command's method must have no parameters.");
        }
    }

    public void OnRecentCommandsUp(InputAction.CallbackContext context)
    {
        if (m_RecentInputs.Count == 0)
            return;

        if (!context.performed)
            return;

        if (m_CurrentInputIndex < m_RecentInputs.Count - 1)
        {
            m_CurrentInputIndex++;
        }
        else
        {
            m_CurrentInputIndex = 0;
        }

        m_ConsoleInputField.text = m_RecentInputs[m_CurrentInputIndex];
    }

    public void OnRecentCommandsDown(InputAction.CallbackContext context)
    {
        if (m_RecentInputs.Count == 0)
            return;

        if (!context.performed)
            return;

        if (m_CurrentInputIndex > 0)
        {
            m_CurrentInputIndex--;
        }
        else
        {
            m_CurrentInputIndex = m_RecentInputs.Count - 1;
        }

        m_ConsoleInputField.text = m_RecentInputs[m_CurrentInputIndex];
    }

    public void OnOpenConsoles()
    {
        m_IsConsoleOpen = true;
        m_ConsoleInputField.gameObject.SetActive(true);
        m_UnityConsolePanel.SetActive(true);
        //m_ConsoleAnimator.SetBool("OpenConsole", true);

        m_ConsoleInputField.text = "";
        m_ConsoleInputField.Select();
        m_PlayerInput.SwitchCurrentActionMap("ConsoleActionMap");
    }

    public void OnCloseConsoles()
    {
        m_IsConsoleOpen = false;
        //m_ConsoleAnimator.SetBool("OpenConsole", false);

        m_ConsoleInputField.text = "";
        m_ConsoleInputField.ReleaseSelection();
        m_PlayerInput.SwitchCurrentActionMap("GameActionMap");

        m_ConsoleInputField.gameObject.SetActive(false);
        m_UnityConsolePanel.SetActive(false);
    }
}
