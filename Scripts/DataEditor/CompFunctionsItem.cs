using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompFunctionsItem : MonoBehaviour
{
    CompFunctionDetail thisFunction;
    public DataEditorMain DataEditor;
    public Text txt_FunctionName;
    public Button btn_SelectFunction;
    public ProceduralImage frame;
    // Start is called before the first frame update
    void Start()
    {
        btn_SelectFunction.onClick.AddListener(OnSelectThis);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitThis(CompFunctionDetail function)
    {
        thisFunction = function;
        txt_FunctionName.text = function.functionName;
    }
    public CompFunctionDetail GetThisFunction()
    {
        return thisFunction;
    }
    void OnSelectThis()
    {
        DataEditor.LoadCompFunctionDetail(this);
        TriggerSelection(true);
    }
    public void TriggerSelection(bool isSelected)
    {
        if(isSelected)
        {
            frame.color = Color.yellow;
        }else
        {
            frame.color = Color.white;
        }
    }
}
