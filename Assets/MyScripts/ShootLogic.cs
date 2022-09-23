using UnityEngine;
using UnityEngine.UI;
public class ShootLogic : MonoBehaviour
{
    public GameObject Parent = null;
    public int CoorX, CoorY;
    GameMap map;
    int cell;
    public Text statusText;
    //этот скрип передает координаты объекта на который нажали мышью
    private void OnMouseDown()
    {
        if (Parent != null)
        {
            map = Parent.GetComponent<GameMap>();
            cell = map.Map[CoorX, CoorY].GetComponent<Cell>().Index;
            if (cell == 0 || cell == 2)
                map.WhoClick(CoorX, CoorY);
            statusText.text = "HERE PARENT CLICK";
        }
        statusText.text = "HERE NOT PARENT CLICK";
    }
}
