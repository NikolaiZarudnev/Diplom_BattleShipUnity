using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGame : MonoBehaviour
{

    //режим игры
    public int GameMode = 0;
    public GameObject PlayerMap, ComputerMap;

    //определяет кто ходит тру- игрок
    public bool whoseMove = true;

    int ShootCount = 0;
    
    //функция проверки попадания по кораблю
    bool isKilled(int X, int Y)
    {
        bool Result = false;
        GameMap playerMap = PlayerMap.GetComponent<GameMap>();
        List<GameMap.Ship> listShips = playerMap.listShips;
        //перебираем корабли и смотрим в какой попали
        foreach (GameMap.Ship ship in listShips)
        {
            //перебираем палубы корабля и смотри попали ли мы в нее
            foreach (GameMap.ShipCell shipCell in ship.shipCells)
            {
                //сравниваем координаты выстрела с координатами палубы 
                if ((shipCell.Position.x == X) && (shipCell.Position.y == Y))
                {
                    int CountKills = 0;
                    //если попал по кобралю то сколько палуб у корабля разрушено?
                    foreach (GameMap.ShipCell hittedShip in ship.shipCells)
                    {
                        if (playerMap.GetIndexCell(hittedShip.Position.x, hittedShip.Position.y) == 3) CountKills++;
                    }
                    //если кол-во палуб равно кол-ву попаданий значит корабль все!
                    if (CountKills == ship.shipCells.Length)
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                    return Result;
                }

            }
        }

        return Result;
    }
    public bool lastHitting = false;
    Vector2Int origHittedCoord, curHittedCoord;
    Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(1, 0) };
    Vector2Int dir;
    void AIrandShoot()
    {
        int ShotX;
        int ShotY;
        int Index;
        GameMap playerMap = PlayerMap.GetComponent<GameMap>();
        do
        {
            ShotX = Random.Range(0, 10);
            ShotY = Random.Range(0, 10);
            Index = playerMap.GetIndexCell(ShotX, ShotY);
        } while (Index == 1 || Index == 3);
        whoseMove = !playerMap.Shoot(ShotX, ShotY);
        if (!whoseMove && !isKilled(ShotX, ShotY))
        {
            lastHitting = true;
            origHittedCoord.x = ShotX;
            origHittedCoord.y = ShotY;
            curHittedCoord = origHittedCoord;
        }
    }
    int GetIndexNextCell(GameMap playerMap)
    {
        if (curdir != Vector2Int.zero)
        {
            dir = curdir;
            return playerMap.GetIndexCell(curHittedCoord.x + dir.x, curHittedCoord.y + dir.y);
        }
        int cell;
        int randDir;
        int dirX = 0,dirY = 0;
        do
        {
            dirX = 0; dirY = 0;
            randDir = Random.Range(0, 2);
            if (randDir == 0)
            {
                while (dirX == 0) dirX = Random.Range(-1, 2);
            }
            else
            {
                while (dirY == 0) dirY = Random.Range(-1, 2);
            }
            cell = playerMap.GetIndexCell(curHittedCoord.x + dirX, curHittedCoord.y + dirY);
        } while (cell == 1 || cell == 3 || cell == -1);
        if (cell == -1)
        {
            Debug.Log("ERROR: cell == -1");
        }
        dir = new Vector2Int(dirX, dirY);
        return cell;
    }
    Vector2Int curdir = new Vector2Int(0,0);
    void AIlastHitting()
    {
        GameMap playerMap = PlayerMap.GetComponent<GameMap>();
        int cell = GetIndexNextCell(playerMap);

        if (cell == 0 || cell == 2)
        {
            
            whoseMove = !playerMap.Shoot(curHittedCoord.x + dir.x, curHittedCoord.y + dir.y);
        }   
        if (!whoseMove)
        {
            curdir = dir;
            if(isKilled(curHittedCoord.x, curHittedCoord.y))
            {
                curHittedCoord = origHittedCoord = new Vector2Int(-1, -1);
                curdir = Vector2Int.zero;
                lastHitting = false;
            }
            else
            {
                curHittedCoord = curHittedCoord + dir;
                cell = playerMap.GetIndexCell(curHittedCoord.x + dir.x, curHittedCoord.y + dir.y);
                if (cell == 1 || cell == 3 || cell == -1)
                {
                    curdir = Vector2Int.zero;
                    curHittedCoord = origHittedCoord;
                }
            }
        }
        else
        {
            curdir = Vector2Int.zero;
            curHittedCoord = origHittedCoord;
        }
    }
    void ArtificialIntelligence()
    {
        //проверяем можно ли ходить
        if (!whoseMove)
        {
            if (lastHitting)
            {
                AIlastHitting();
            }
            else
            {
                AIrandShoot();
            }
            
            
        }
    }
    
    public Text status;
    void WhoWin()
    {
        //проверяем сколько палуб сталось у пк
        int PC_Ship = ComputerMap.GetComponent<GameMap>().CountLiveShips();
        int Player_Ship = PlayerMap.GetComponent<GameMap>().CountLiveShips();

        //луз ПК
        if (PC_Ship == 0) status.text = "YOU WIN";
        //Луз игрока
        if (Player_Ship == 0) status.text = "YOU LOSE";
    }
    public void UserClick(int X, int Y)
    {
        if (whoseMove)
        {
            //ходит игрок
            //если он попал то функция вернет тру и ход останется за ним
            //а если промах то ход передается пк
            whoseMove = ComputerMap.GetComponent<GameMap>().Shoot(X, Y);
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
        ArtificialIntelligence();
        WhoWin();
    }
}
