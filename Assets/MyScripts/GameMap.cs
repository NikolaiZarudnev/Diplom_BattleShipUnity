using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMap : MonoBehaviour
{
	//корабли
	public struct ShipCell
	{
		public Vector2Int Position;
		public bool isHead;
	}
	public struct Ship
	{
		public int shipType;
		public ShipCell[] shipCells;
		public Vector2Int shipDirection;
		public bool isKilled;
	}
	public List<Ship> listShips = new List<Ship>();
	public int[] countShips = { 0, 4, 3, 2, 1 };

	//игровое поле
	public GameObject cell, letter, number, GameMain;
	public bool HideShip = false;
	private GameObject[] arrayNumbers;
	private GameObject[] arrayLetters;
	public GameObject[,] Map;

	//int Time=100, DeltaTime=0;
	int mapSize = 10;
	void CreateMap(Vector2 startPosition)
	{
		//создает вокруг поля буквы и цифры 
		//Vector2 startPosition = transform.position;
		float XX = startPosition.x + 2;
		float YY = startPosition.y - 2;
		arrayLetters = new GameObject[mapSize];
		arrayNumbers = new GameObject[mapSize];
		for (int i = 0; i < mapSize; i++)
		{
			arrayLetters[i] = Instantiate(letter);
			arrayLetters[i].transform.position = new Vector2(XX, startPosition.y);
			arrayLetters[i].GetComponent<Cell>().Index = i;
			XX += 2;
			arrayNumbers[i] = Instantiate(number);
			arrayNumbers[i].transform.position = new Vector2(startPosition.x, YY);
			arrayNumbers[i].GetComponent<Cell>().Index = i;
			YY -= 2;
		}
		XX = startPosition.x + 2;
		YY = startPosition.y - 2;
		//создает на поле клетки и задает им координаты
		//индекс 0 значит, что это пустая клетка
		//индекс 1 - клетка с кораблем
		Map = new GameObject[mapSize, mapSize];

		for (int Y = 0; Y < mapSize; Y++)
		{
			for (int X = 0; X < mapSize; X++)
			{
				Map[X, Y] = Instantiate(cell);
				Map[X, Y].GetComponent<Cell>().Index = 0;
				Map[X, Y].GetComponent<Cell>().HideCell = HideShip;
				Map[X, Y].transform.position = new Vector2(XX, YY);
				if (HideShip) Map[X, Y].GetComponent<ShootLogic>().Parent = this.gameObject;

				Map[X, Y].GetComponent<ShootLogic>().CoorX = X;
				Map[X, Y].GetComponent<ShootLogic>().CoorY = Y;

				XX += 2;
			}
			XX = startPosition.x + 2;
			YY -= 2;
		}

	}
	void disableStatus()
	{
		if (statusActive != false)
		{
			status.SetActive(false);
		}
		statusActive = false;
	}
	public void ClearMap()
	{
		disableStatus();
		countShips = new int[] { 0, 4, 3, 2, 1 };
		listShips.Clear();
		for (int Y = 0; Y < mapSize; Y++)
		{
			for (int X = 0; X < mapSize; X++)
			{
				Map[X, Y].GetComponent<Cell>().Index = 0;
				Destroy(Map[X, Y].GetComponent<Cell>().ship3d);
			}
		}

	}
	bool hasShip()
	{
		//переменная для подсчета кораблей
		int shipsLeft = 0;
		//суммируем все значения
		foreach (int countShipAlive in countShips)
		{
			shipsLeft += countShipAlive;
		}
		//если сумма не ноль, значит можно ставить дальше
		if (shipsLeft != 0)
		{
			return true;
		}
		//иначе нет
		return false;
	}
	bool TryEnterShipCell(int X, int Y)
	{
		if ((X > -1) && (Y > -1) && (X < 10) && (Y < 10))
		{
			for (int i = 0; i < 3; i++)
			{
				for (int k = 0; k < 3; k++)
				{
					if ((X - 1 + k > -1) && (Y - 1 + i > -1) && (X - 1 + k < 10) && (Y - 1 + i < 10))
					{
						if (GetIndexCell(X - 1 + k, Y - 1 + i) != 0) return false;
					}
				}
			}
			return true;
		}
		return false;
	}
	//проверяем установку палуб в опред. направлении
	Ship TryEnterShipDirect(int shipType, int XD, int YD, int X, int Y)
	{
		//массив для результата
		ShipCell shipCell;
		shipCell.Position = new Vector2Int();
		shipCell.isHead = false;

		Ship ship;
		ship.shipType = shipType;
		ship.shipCells = new ShipCell[shipType];
		ship.shipDirection = new Vector2Int(XD, YD);
		ship.isKilled = false;
		for (int i = 0; i < shipType; i++)
		{
			if (TryEnterShipCell(X, Y))
			{
				//запоминаем значение координат
				shipCell.Position.x = X;
				shipCell.Position.y = Y;
				if (i == 0 && shipType != 1)
				{
					shipCell.isHead = true;
				}
				else
				{
					shipCell.isHead = false;
				}
					
			}
			else
			{
				ship.shipCells = null;
				return ship;
			}
			X += XD;
			Y += YD;
			ship.shipCells[i] = shipCell;
		}

		return ship;
	}
	Ship TryEnterShip(int shipType, int Direction, int X, int Y)
	{
		Ship ship;
		ship.shipType = shipType;
		ship.shipCells = new ShipCell[shipType];
		ship.shipDirection = new Vector2Int();
		ship.isKilled=false;
		switch (Direction)
		{
			case 0:
				//пробуем установить палубы в положительном направлении Х
				//shipType - размер корабля потом передается в TestEnterShipDirect
				ship = TryEnterShipDirect(shipType, 1, 0, X, Y);
				//Если не вышло поставить корабль
				if (ship.shipCells == null) ship = TryEnterShipDirect(shipType, -1, 0, X, Y);
				break;
			case 1:
				//пробуем посавить в положжительном направлении У
				ship = TryEnterShipDirect(shipType, 0, 1, X, Y);
				//если не получилось
				if (ship.shipCells == null) ship = TryEnterShipDirect(shipType, 0, -1, X, Y);
				break;

		}
		return ship;
	}
	//если все условия сработали ставит корабль
	//индекс 0 значит, что это пустая клетка
	//индекс 1 - клетка с кораблем
	public GameObject prefShip3d, prefShip3dHead;
	Quaternion targetQuaternionHead(Vector2Int dir)
    {
		Quaternion target;

		if (dir == Vector2Int.down)
        {
			target = Quaternion.Euler(-90, 180, 180);
		}
		else if (dir == Vector2Int.up)
        {
			target = Quaternion.Euler(90, 0, 180);
		}
        else if (dir == Vector2Int.right)
        {
			target = Quaternion.Euler(0, 90, -90);
		}
        else
        {
			target = Quaternion.Euler(180, 90, -90);
		}
		return target;
	}
	bool EnterShip(int ShipType, int Direction, int X, int Y)
	{
		Ship ship = TryEnterShip(ShipType, Direction, X, Y);
		if (ship.shipCells != null)
		{
			foreach (ShipCell T in ship.shipCells)
			{

				Map[T.Position.x, T.Position.y].GetComponent<Cell>().Index = 2;
				
				if (T.isHead)
				{
					Quaternion target = targetQuaternionHead(ship.shipDirection);
					Vector3 pos = Map[T.Position.x, T.Position.y].transform.position;
					Map[T.Position.x, T.Position.y].GetComponent<Cell>().ship3d = Instantiate(prefShip3dHead, pos, target);
				}
				else
				{
					Map[T.Position.x, T.Position.y].GetComponent<Cell>().ship3d = Instantiate(prefShip3d,
																						Map[T.Position.x, T.Position.y].transform.position,
																						Map[T.Position.x, T.Position.y].transform.rotation);
				}
			}
			//сохраняем корабль в список
			listShips.Add(ship);
			return true;
		}
		return false;
	}
	//случайная расстановка кораблей 
	public void EnterRandomShip()
	{
		ClearMap();
		//тип коробля: 0 - одно палубый, 3 - 4рех плубный
		int selectShip = 4;

		//координаты по которым будет установлен корабль
		int X, Y;

		//положение корабля вертикаль или горизонталь
		int Direction;
		while (hasShip())
		{

			//генерирум координаты для постановки корабля
			X = Random.Range(0, 10);
			Y = Random.Range(0, 10);
			Direction = Random.Range(0, 2);
			if (EnterShip(selectShip, Direction, X, Y))
			{
				countShips[selectShip]--;
				if (countShips[selectShip] == 0)
				{
					selectShip--;
				}
			}
		}
	}
	public int gameMode = 0;// offline
	void Start()
	{
		Vector2 startPositionPlayer = transform.position;
		if (gameMode == 0) CreateMap(startPositionPlayer);
	}

	public int CountLiveShips()
	{
		//подсчет кол-во кораблей
		int Count = 0;

		foreach (Ship ship in listShips)
		{
			foreach (ShipCell shipCell in ship.shipCells)
			{
				if (GetIndexCell(shipCell.Position.x, shipCell.Position.y) == 2) Count++;
			}
		}

		return Count;
	}

	public void WhoClick(int X, int Y)
	{
		if (GameMain != null) GameMain.GetComponent<MainGame>().UserClick(X, Y);
	}
	public int GetIndexCell(int X, int Y)
	{
		if (X > -1 && X < 10 && Y > -1 && Y < 10)
		{
			return Map[X, Y].GetComponent<Cell>().Index;
		}
		return -1;
	}
	//Выстрел по клетке
	public Text statusText;
	public GameObject status;
	bool statusActive = false;
	void enableStatus()
    {

		if (statusActive != true)
        {
			status.SetActive(true);
		}
		statusActive = true;
	}
	public bool Shoot(int X, int Y)
	{
		enableStatus();
		int MapSelect = GetIndexCell(X, Y);
		bool Result = false;
		switch (MapSelect)
		{
			//промах
			case 0:
				Map[X, Y].GetComponent<Cell>().Index = 1;
				if (HideShip)
				{
					statusText.text = "MISSED";
					Debug.Log("MISSED");
				}
				Result = false;
				break;
			//попал
			case 2:
				Map[X, Y].GetComponent<Cell>().Index = 3;
				Map[X, Y].GetComponent<Cell>().hittedOver = Instantiate(cell);
				Map[X, Y].GetComponent<Cell>().hittedOver.GetComponent<Cell>().Index = 4;
				Map[X, Y].GetComponent<Cell>().hittedOver.GetComponent<Cell>().HideCell = false;
				Vector3 newpos = Map[X, Y].transform.position;
				newpos.z = (float)-1.1;
				Map[X, Y].GetComponent<Cell>().hittedOver.transform.position = newpos;
				Map[X, Y].GetComponent<Cell>().hittedOver.SetActive(false);
				//Destroy(Map[X, Y].GetComponent<Cell>().ship3d);
				Result = true;
				if (isHit(X, Y))
				{
					if (HideShip)
					{
						statusText.text = "KILLED";
						Debug.Log("KILLED");

					}
					
					HitAroundShip(X, Y);
				}
				else
				{
					if (HideShip)
					{
						statusText.text = "HITTED";
						Debug.Log("HITTED");
					}
				}
				break;
		}
		return Result;
	}
	void HitAroundShip(int X, int Y)
	{
		//перебираем корабли и смотрим в какой попали
		foreach (Ship ship in listShips)
		{
			//перебираем палубы корабля
			foreach (ShipCell shipCell in ship.shipCells)
			{
				if ((shipCell.Position.x == X) && (shipCell.Position.y == Y))
				{
					foreach (ShipCell killedCellShip in ship.shipCells)
					{
						for (int i = 0; i < 3; i++)
						{
							for (int k = 0; k < 3; k++)
							{
								if ((killedCellShip.Position.x - 1 + k > -1) && (killedCellShip.Position.y - 1 + i > -1) && (killedCellShip.Position.x - 1 + k < 10) && (killedCellShip.Position.y - 1 + i < 10))
								{
									if (GetIndexCell(killedCellShip.Position.x - 1 + k, killedCellShip.Position.y - 1 + i) == 0)
									{
										Map[killedCellShip.Position.x - 1 + k, killedCellShip.Position.y - 1 + i].GetComponent<Cell>().Index = 1;
									}
								}
							}
						}
						Map[killedCellShip.Position.x, killedCellShip.Position.y].GetComponent<Cell>().hittedOver.SetActive(true);
						if (Map[killedCellShip.Position.x, killedCellShip.Position.y].GetComponent<Cell>().ship3d != null)
							Map[killedCellShip.Position.x, killedCellShip.Position.y].GetComponent<Cell>().ship3d.SetActive(true);
					}
					return;
				}
			}

		}
	}
	//функция проверки попадания по кораблю
	bool isHit(int X, int Y)
	{
		bool Result = false;
		//перебираем корабли и смотрим в какой попали
		for (int i = 0; i < listShips.Count; i++)
		{
			//перебираем палубы корабля и смотри попали ли мы в нее
			foreach (ShipCell cellShip in listShips[i].shipCells)
			{
				//сравниваем координаты выстрела с координатами палубы 
				if ((cellShip.Position.x == X) && (cellShip.Position.y == Y))
				{
					int CountKills = 0;
					//если попал по кобралю то сколько палуб у корабля разрушено?
					foreach (ShipCell hittedShip in listShips[i].shipCells)
					{
						if (GetIndexCell(hittedShip.Position.x, hittedShip.Position.y) == 3) CountKills++;
					}
					//если кол-во палуб равно кол-ву попаданий значит корабль все!
					if (CountKills == listShips[i].shipCells.Length)
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
}
