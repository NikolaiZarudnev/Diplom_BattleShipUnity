using UnityEngine;

public class Cell : MonoBehaviour
{
    public Sprite[] imgs;
    public int Index = 0;
    public bool HideCell = false;
    public GameObject ship3d;
    public GameObject hittedOver;
    void ChangeImgs()
    {
        if (imgs.Length > Index)
        {
            if (HideCell && Index == 2)
            {
                GetComponent<SpriteRenderer>().sprite = imgs[0];
                if (ship3d != null) ship3d.SetActive(false);
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = imgs[Index];
                
            }
        }
    }
    void Start()
    {
        ChangeImgs();
    }

    void Update()
    {
        ChangeImgs();
    }
}
