using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconSelectorItem : MonoBehaviour
{
    public Image img_Frame;
    public Image img_Icon;
    public string iconName;
    public int iconIndex;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void IntThis(string name, int index)
    {
        iconName = name;
        iconIndex = index;
        img_Icon.sprite = Tools.GetIcon(iconName, iconIndex);
    }
    public void TriggerSelected(bool isSelected)
    {
        if(isSelected)
        {
            img_Frame.color = Color.yellow;
        }else
        {
            img_Frame.color = Color.white;
        }
    }
}
