/**
 * Author: Sander Homan
 * Copyright 2012
 **/

using System.Collections;
using UnityEngine;

internal class ConsoleNGUI : MonoBehaviour
{
    public UIRoot root = null;
    public UILabel logLabel = null;
    public UIScrollBar logScroll = null;
    public UIInput commandInput = null;
    public UIPanel objectComponentPanel = null;
    public UIPanel methodPanel = null;
    public UIScrollBar methodPanelScroll = null;
    public int methodPanelButtonVisibleCount = 10;
    public int linesVisible = 10;

    public GameObject buttonPrefab = null;

    private string[] displayObjects;
    private string[] displayComponents;
    private string command;
    private string[] displayMethods;
    private int oldMethodPanelScroll;

    public UIScrollBar componentPanelScroll = null;
    public int componentPanelButtonVisibleCount = 5;
    public UIScrollBar objectPanelScroll = null;
    public int objectPanelButtonVisibleCount = 5;
    private int oldObjectPanelScroll;
    private int oldComponentPanelScroll;

    public bool isOpen = false;

    private void Start()
    {
        //root.gameObject.SetActiveRecursively(isOpen);
        root.gameObject.SetActive(isOpen);
    }

    private void Update()
    {
        if (isOpen)
        {
            var lines = Console.Instance.Lines;
            //int historyScrollValue = Mathf.Clamp((int)((lines.Count() - linesVisible) * (1 - logScroll.scrollValue)), 0, lines.Count());
            int historyScrollValue = Mathf.Clamp((int)((lines.Count() - linesVisible) * (1 - logScroll.scrollValue)), 0, lines.Count());

            // update log
            string logText = "";
            // display last 10 lines
            for (int i = lines.Count() - Mathf.Min(linesVisible, lines.Count()) - historyScrollValue; i < lines.Count() - historyScrollValue; i++)
            {
                logText += lines.GetItemAt(i) + "\n";
            }
            logLabel.text = logText;

            //Console.Instance.Print("logtest" + Time.frameCount);

            // check if command has changed
            //if (commandInput.text != command)
            //{
            //    command = commandInput.text;

            if (commandInput.text != command)
            {
                command = commandInput.text;

                displayObjects = Console.Instance.GetGameobjectsAtPath(command);
                displayComponents = Console.Instance.GetComponentsOfGameobject(command);
                displayMethods = Console.Instance.GetMethodsOfComponent(command);

                int firstDot = command.IndexOf('.');
                if (firstDot == -1 || command.IndexOf('.', firstDot + 1) == -1)
                {
                    //objectComponentPanel.gameObject.SetActiveRecursively(true);
                    //methodPanel.gameObject.SetActiveRecursively(false);
                    objectComponentPanel.gameObject.SetActive(true);
                    methodPanel.gameObject.SetActive(false);
                    StartCoroutine(BuildObjectPanel());
                    StartCoroutine(BuildComponentPanel());
                }
                else
                {
                    //objectComponentPanel.gameObject.SetActiveRecursively(false);
                    //methodPanel.gameObject.SetActiveRecursively(true);
                    objectComponentPanel.gameObject.SetActive(false);
                    methodPanel.gameObject.SetActive(true);
                    StartCoroutine(BuildMethodPanel());
                }
            }

            int newObjectPanelScroll = Mathf.Clamp((int)((displayObjects.Length - objectPanelButtonVisibleCount) *
                //(objectPanelScroll.scrollValue)), 0, displayObjects.Length);
                (objectPanelScroll.scrollValue)), 0, displayObjects.Length);
            if (newObjectPanelScroll != oldObjectPanelScroll)
            {
                oldObjectPanelScroll = newObjectPanelScroll;
                StartCoroutine(BuildObjectPanel());
            }

            int newComponentPanelScroll = Mathf.Clamp((int)((displayComponents.Length - componentPanelButtonVisibleCount) *
                //(componentPanelScroll.scrollValue)), 0, displayComponents.Length);
                (componentPanelScroll.scrollValue)), 0, displayComponents.Length);
            if (newComponentPanelScroll != oldComponentPanelScroll)
            {
                oldComponentPanelScroll = newComponentPanelScroll;
                StartCoroutine(BuildComponentPanel());
            }

            int newMethodPanelScroll = Mathf.Clamp((int)((displayMethods.Length - methodPanelButtonVisibleCount) * //(methodPanelScroll.scrollValue)), 0, displayMethods.Length);
                (methodPanelScroll.scrollValue)), 0, displayMethods.Length);
            if (newMethodPanelScroll != oldMethodPanelScroll)
            {
                oldMethodPanelScroll = newMethodPanelScroll;
                StartCoroutine(BuildMethodPanel());
            }

            if (Input.GetKeyUp(Console.Instance.m_closeKey))
            {
                isOpen = false;
                //root.gameObject.SetActiveRecursively(isOpen);
                root.gameObject.SetActive(isOpen);
            }
        }
        else
        {
            if (Input.GetKeyUp(Console.Instance.m_openKey))
            {
                isOpen = true;
                //root.gameObject.SetActiveRecursively(isOpen);
                root.gameObject.SetActive(isOpen);
            }
        }
    }

    private IEnumerator BuildObjectPanel()
    {
        GameObject buttonPanel = objectComponentPanel.transform.Find("ObjectScrollView/ButtonPanel").gameObject;
        // clear old buttons
        foreach (var b in buttonPanel.GetComponentsInChildren<UIButton>())
        {
            Destroy(b.gameObject);
        }
        yield return null;

        int scrollValue = Mathf.Clamp((int)((displayObjects.Length - objectPanelButtonVisibleCount) *
            //(objectPanelScroll.scrollValue)), 0, displayObjects.Length);
            (objectPanelScroll.scrollValue)), 0, displayObjects.Length);
        for (int i = scrollValue; i < Mathf.Min(scrollValue + objectPanelButtonVisibleCount, displayObjects.Length); i++)
        {
            // add a button
            var button = NGUITools.AddChild(buttonPanel.gameObject, buttonPrefab);
            button.GetComponentInChildren<UILabel>().text = displayObjects[i];
            button.GetComponent<UIButtonMessage>().target = gameObject;
            button.GetComponent<UIButtonMessage>().functionName = "OnObjectButtonClick";
        }

        buttonPanel.GetComponent<UIGrid>().Reposition();
    }

    private IEnumerator BuildComponentPanel()
    {
        Debug.Log("BuildComponentPanel");
        GameObject buttonPanel = objectComponentPanel.transform.Find("ComponentScrollView/ButtonPanel").gameObject;
        // clear old buttons
        foreach (var b in buttonPanel.GetComponentsInChildren<UIButton>())
        {
            Destroy(b.gameObject);
        }
        yield return null;

        int scrollValue = Mathf.Clamp((int)((displayComponents.Length - componentPanelButtonVisibleCount) *
            //(componentPanelScroll.scrollValue)), 0, displayComponents.Length);
            (componentPanelScroll.scrollValue)), 0, displayComponents.Length);
        for (int i = scrollValue; i < Mathf.Min(scrollValue + componentPanelButtonVisibleCount, displayComponents.Length); i++)
        {
            // add a button
            var button = NGUITools.AddChild(buttonPanel.gameObject, buttonPrefab);
            button.GetComponentInChildren<UILabel>().text = displayComponents[i];
            button.GetComponent<UIButtonMessage>().target = gameObject;
            button.GetComponent<UIButtonMessage>().functionName = "OnComponentButtonClick";
        }

        buttonPanel.GetComponent<UIGrid>().Reposition();
    }

    private IEnumerator BuildMethodPanel()
    {
        GameObject buttonPanel = methodPanel.transform.Find("ScrollView/ButtonPanel").gameObject;
        // clear old buttons
        foreach (var b in buttonPanel.GetComponentsInChildren<UIButton>())
        {
            Destroy(b.gameObject);
        }
        yield return null;

        int methodScrollValue = Mathf.Clamp((int)((displayMethods.Length - methodPanelButtonVisibleCount) *
            //(methodPanelScroll.scrollValue)), 0, displayMethods.Length);
            (methodPanelScroll.scrollValue)), 0, displayMethods.Length);
        for (int i = methodScrollValue; i < Mathf.Min(methodScrollValue + methodPanelButtonVisibleCount, displayMethods.Length); i++)
        {
            // add a button
            var button = NGUITools.AddChild(buttonPanel.gameObject, buttonPrefab);
            button.GetComponentInChildren<UILabel>().text = displayMethods[i];
            button.GetComponent<UIButtonMessage>().target = gameObject;
            button.GetComponent<UIButtonMessage>().functionName = "OnMethodButtonClick";
        }

        buttonPanel.GetComponent<UIGrid>().Reposition();
    }

    private void OnSubmit()
    {
        //string command = commandInput.text;
        //commandInput.text = "";
        string command = commandInput.text;
        commandInput.text = "";

        Console.Instance.Print("> " + command);
        Console.Instance.Eval(command);
        command = "";
        displayObjects = Console.Instance.GetGameobjectsAtPath(command);
        displayComponents = Console.Instance.GetComponentsOfGameobject(command);
    }

    private void OnMethodButtonClick(GameObject go)
    {
        int firstDot = command.IndexOf('.');
        //commandInput.text = commandInput.text.Substring(0, commandInput.text.IndexOf('.', firstDot + 1));
        //commandInput.text += "." + go.GetComponentInChildren<UILabel>().text;
        commandInput.text = commandInput.text.Substring(0, commandInput.text.IndexOf('.', firstDot + 1));
        commandInput.text += "." + go.GetComponentInChildren<UILabel>().text;
    }

    private void OnComponentButtonClick(GameObject go)
    {
        int firstDot = command.IndexOf('.');

        //if (firstDot > 0)
        //    commandInput.text = commandInput.text.Substring(0, firstDot);
        //if (commandInput.text.EndsWith("/"))
        //    commandInput.text = commandInput.text.Substring(0, command.Length - 1);

        //commandInput.text += "." + go.GetComponentInChildren<UILabel>().text + ".";

        commandInput.text += "." + go.GetComponentInChildren<UILabel>().text + ".";
        if (firstDot > 0)
            commandInput.text = commandInput.text.Substring(0, firstDot);
        if (commandInput.text.EndsWith("/"))
            commandInput.text = commandInput.text.Substring(0, command.Length - 1);

        commandInput.text += "." + go.GetComponentInChildren<UILabel>().text + ".";
    }

    private void OnObjectButtonClick(GameObject go)
    {
        //if (commandInput.text.LastIndexOf('/') >= 0)
        //    commandInput.text = commandInput.text.Substring(0, command.LastIndexOf('/'));
        //commandInput.text += "/" + go.GetComponentInChildren<UILabel>().text + "/";

        if (commandInput.text.LastIndexOf('/') >= 0)
            commandInput.text = commandInput.text.Substring(0, command.LastIndexOf('/'));
        commandInput.text += "/" + go.GetComponentInChildren<UILabel>().text + "/";
    }
}