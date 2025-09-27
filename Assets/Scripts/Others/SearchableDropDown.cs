using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class SearchableDropDown : MonoBehaviour
{

    [SerializeField] private ScrollRect scrollRect = null;
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private Transform content = null;

    [SerializeField] private Button blockerButton;
    [SerializeField] private GameObject buttonsPrefab = null;
    [SerializeField] private int maxScrollRectSize = 180;
    [SerializeField] private List<string> avlOptions = new List<string>();




    private Button ddButton = null;
    private RectTransform scrollRectTrans;
    private bool isContentHidden = true;
    private List<Button> initializedButtons = new List<Button>();

    public delegate void OnValueChangedDel(string val);
    public OnValueChangedDel OnValueChangedEvt;


    private readonly string[] regionCodes =
{
        "R_hungary",
        "R_europe",
        "R_asia",
        "R_africa",
        "R_namerica",
        "R_samerica",
        "R_australia"
    };

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// Initilize all the Fields
    /// </summary>
    private void Init()
    {
        ddButton = this.GetComponentInChildren<Button>();
        scrollRectTrans = scrollRect.GetComponent<RectTransform>();

        //blocker is a button added and scaled it to screen size so that we can close the dd on clicking outside
        blockerButton.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        blockerButton.gameObject.SetActive(false);
        blockerButton.transform.SetParent(this.GetComponentInParent<Canvas>().transform);

        blockerButton.onClick.AddListener(OnBlockerButtClick);
        ddButton.onClick.AddListener(OnDDButtonClick);
        scrollRect.onValueChanged.AddListener(OnScrollRectvalueChange);
        inputField.onValueChanged.AddListener(OnInputvalueChange);
        inputField.onEndEdit.AddListener(OnEndEditing);

        AddItemToScrollRect(avlOptions);

    }

    /// <summary>
    /// public method to get the selected value
    /// </summary>
    /// <returns></returns>
    public string GetValue()
    {
        return inputField.text;
    }

    public void ResetDropDown()
    {
        inputField.text = string.Empty;
        
    }

    //call this to Add items to Drop down
    public void AddItemToScrollRect(List<string> options)
    {
        foreach (var option in options)
        {
            var buttObj = Instantiate(buttonsPrefab, content);
            buttObj.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.GetLocalizedValue(option);
            buttObj.name = option;
            buttObj.SetActive(true);
            var butt = buttObj.GetComponent<Button>();
            butt.onClick.AddListener(delegate { OnItemSelected(buttObj); });
            initializedButtons.Add(butt);
        }
        ResizeScrollRect();
        scrollRect.gameObject.SetActive(false);
    }


    /// <summary>
    /// listner To Input Field End Editing
    /// </summary>
    /// <param name="arg"></param>
    private void OnEndEditing(string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            Debug.Log("no value entered ");
            return;
        }
        StartCoroutine(CheckIfValidInput(arg));
    }

    /// <summary>
    /// Need to wait as end inputField and On option button  Contradicted and message was poped after selection of button
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    IEnumerator CheckIfValidInput(string arg)
    {
        yield return new WaitForSeconds(1);
        if (!avlOptions.Contains(arg))
        {
           // Message msg = new Message("Invalid Input!", "Please choose from dropdown",
           //                 this.gameObject, Message.ButtonType.OK);
           //
           //             if (MessageBox.instance)
           //                 MessageBox.instance.ShowMessage(msg); 

            inputField.text = String.Empty;
        }
        //else
        //    Debug.Log("good job " );
        OnValueChangedEvt?.Invoke(inputField.text);
    }
    /// <summary> /// Called ever time on Drop down value is changed to resize it /// </summary> 
    /// <summary>
    /// Called every time the dropdown value is changed to resize it
    /// </summary>
    private void ResizeScrollRect()
    {
        // Force Unity to rebuild the layout immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);

        // Get total height of content
        var length = content.GetComponent<RectTransform>().sizeDelta.y;

        // Clamp height to maxScrollRectSize
        scrollRectTrans.sizeDelta = length > maxScrollRectSize
            ? new Vector2(scrollRectTrans.sizeDelta.x, maxScrollRectSize)
            : new Vector2(scrollRectTrans.sizeDelta.x, length + 5);
    }


    /// <summary>
    /// listner to the InputField
    /// </summary>
    /// <param name="arg0"></param>
    private void OnInputvalueChange(string arg0)
    {
        if (!avlOptions.Contains(arg0))
        {
            FilterDropdown(arg0);
        }
    }

    /// <summary>
    /// remove the elements from the dropdown based on Filters
    /// </summary>
    /// <param name="input"></param>
    public void FilterDropdown(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            foreach (var button in initializedButtons)
                button.gameObject.SetActive(true);
            ResizeScrollRect();
            scrollRect.gameObject.SetActive(false);
            return;
        }

        var count = 0;
        foreach (var button in initializedButtons)
        {
            if (!button.name.ToLower().Contains(input.ToLower()))
            {
                button.gameObject.SetActive(false);
            }
            else
            {
                button.gameObject.SetActive(true);
                count++;
            }
        }

        SetScrollActive(count > 0);
        ResizeScrollRect();
    }

    /// <summary>
    /// Listner to Scroll rect
    /// </summary>
    /// <param name="arg0"></param>
    private void OnScrollRectvalueChange(Vector2 arg0)
    {
        //Debug.Log("scroll ");
    }

    /// <summary>
    /// Listner to option Buttons
    /// </summary>
    /// <param name="obj"></param>
    private void OnItemSelected(GameObject obj)
    {
        inputField.text = obj.name;

        // Save to PlayerPrefs
        PlayerPrefs.SetString("PlayerRegion", obj.name); // <-- replace "PlayerRegion" with your PlayerPrefsRegionKey
        PlayerPrefs.Save();

        Debug.Log($"Region saved: {obj.name}");

        foreach (var button in initializedButtons)
            button.gameObject.SetActive(true);

        isContentHidden = false;
        OnDDButtonClick();
        StopAllCoroutines();
        StartCoroutine(CheckIfValidInput(obj.name));
    }

    /// <summary>
    /// listner to arrow button on input field
    /// </summary>
    private void OnDDButtonClick()
    {
        if(GetActiveButtons()<=0)
            return;
        ResizeScrollRect();
        SetScrollActive(isContentHidden);
    }
    private void OnBlockerButtClick()
    {
        SetScrollActive(false);
    }

    /// <summary>
    /// respondisble to enable and disable scroll rect component 
    /// </summary>
    /// <param name="status"></param>
    private void SetScrollActive(bool status)
    {
        scrollRect.gameObject.SetActive(status);
        blockerButton.gameObject.SetActive(status);
        isContentHidden = !status;
        ddButton.transform.localScale = status ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Return numbers of active buttons in the dropdown
    /// </summary>
    /// <returns></returns>
    private float GetActiveButtons()
    {
        var count = content.transform.Cast<Transform>().Count(child => child.gameObject.activeSelf);
        var length = buttonsPrefab.GetComponent<RectTransform>().sizeDelta.y * count;
        return length;
    }

    public void SetValue(string regionCode)
    {
        // Translate region code to display label
        string label = LocalizationManager.Instance.GetLocalizedValue(regionCode);

        if (inputField != null)
        {
            inputField.text = label; // sets visible text
        }

        // If you want, also trigger your delegate/event
        OnValueChangedEvt?.Invoke(regionCode);
    }

    public void RefreshDropdown(List<string> codes)
    {

        // Update button texts and names
        for (int i = 0; i < initializedButtons.Count; i++)
        {
            var butt = initializedButtons[i];

            if (i < codes.Count)
            {
                // Update text & name for this button
                butt.gameObject.SetActive(true);
                butt.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.GetLocalizedValue(codes[i]);
                butt.name = codes[i];
            }
            else
            {
                // Hide extra buttons if any
                butt.gameObject.SetActive(false);
            }
        }

        // Resize scroll rect AFTER updating buttons
        scrollRect.gameObject.SetActive(true);
    }



}
