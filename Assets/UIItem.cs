using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private RectTransform bar;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject premium;
    [SerializeField] private int index;

    public int Index { get => index;}

    public void FillData(BuildingData data)
    {
        itemName.text = data.name;
        itemPrice.text = data.buildingCost.ToString();
        description.text = data.description;
        bar.gameObject.SetActive(false);
        icon.sprite = data.icon;
        index = data.Index;
    }

    public void FillData(ComponentData data)
    {
        itemName.text = data.displayName;
        itemPrice.text = data.cost.ToString();
        description.text = data.description;
        bar.gameObject.SetActive(false);
        icon.sprite = data.icon;
        index = data.Index;
    }
}
