using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public Button buttonGenerator;

    public InputField xSizeInput;

    public InputField ySizeInput;

    public InputField widthInput;

    public InputField heightInput;

    public MazeGeneration mazeGenerator;

    // Start is called before the first frame update
    void Start()
    {
        buttonGenerator.onClick.AddListener(delegate { OnGenerate(buttonGenerator); });
        xSizeInput.onValueChanged.AddListener(delegate (string text) { ValidateFields(xSizeInput, text); });
        ySizeInput.onValueChanged.AddListener(delegate (string text) { ValidateFields(ySizeInput, text); });
        widthInput.onValueChanged.AddListener(delegate (string text) { ValidateFields(widthInput, text); });
        heightInput.onValueChanged.AddListener(delegate (string text) { ValidateFields(heightInput, text); });
    }

    // validate against 0 or negative numbers
    void ValidateFields(InputField inputField, string text)
    {
        try
        {
            int number = Convert.ToInt32(text);
            if (number < 1)
            {
                inputField.text = "";
            }
        }
        catch (Exception)
        {
            inputField.text = "";
        }
    }

    void OnGenerate(Button button)
    {
        mazeGenerator.generateMaze(Convert.ToUInt32(xSizeInput.text), Convert.ToUInt32(ySizeInput.text), Convert.ToUInt32(widthInput.text), Convert.ToUInt32(heightInput.text));
        GetComponent<Canvas>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GetComponent<Canvas>().enabled = !GetComponent<Canvas>().enabled;
        }
    }
}
