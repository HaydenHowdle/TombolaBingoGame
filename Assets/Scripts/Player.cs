using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	#region References

	[Tooltip("The canvas the balls move around in")]
	public Canvas _ballCanvas = null; //The canvas where the balls move

	[Tooltip("The prefab to use for the balls")]
	public GameObject _ballPrefab = null; //The prefab of the ball to spawn

	[Tooltip("A parent GameObject which holds 12 or more spawn positions")]
	public GameObject _spawnPositions = null; //Parent gameobject that holds spawn points for the balls

	[Tooltip("Button that can be used to toggle auto mark")]
	public Button _autoMarkButton = null;

	private BingoManager _manager = null; //Game manager.

	private List<GameObject> _bingoBalls = new List<GameObject>(_maxNumberPicks); //The visual bingo balls that belong to this player

	#endregion

	#region Private

	private string _name = "Default Player"; //Name of the player
	private const int _maxNumberPicks = 12;
	private HashSet<int> _chosenNumbers = new HashSet<int>(); //Hash set is used to prevent duplicate numbers but this could be an array with manual checking.
	private HashSet<RectTransform> _chosenSpawns = new HashSet<RectTransform>();
	private System.Random _rng = new System.Random(Guid.NewGuid().GetHashCode()); //Using a new guid's hash code is less predictable than using current time.

	private bool _autoMark = false;

	#endregion

	#region Properties

	public bool AutoMark
	{
		get
		{
			return _autoMark;
		}
	}

	public bool IsBingo
	{
		get
		{
			return _bingoBalls.Count == 0;
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}

		set
		{
			_name = value;
		}
	}

	#endregion

	#region Private Methods

	private void PickNumbers()
	{
		/* This is what I would use for a non-cheated version of the game outside of demoing.
		while (_chosenNumbers.Count < _maxNumberPicks)
		{
			_chosenNumbers.Add(_rng.Next(1, 37)); //Max is exclusive.
		}*/

		for (int i = 0; i < 12; i++) //Grab first 12 numbers that will be called out as the chosen bingo numbers.
		{
			_chosenNumbers.Add(_manager.CalloutOrder.ElementAt(i));
		}

		for (int i = 0; i < _bingoBalls.Count; i++)
		{
			_bingoBalls[i].GetComponent<BingoBall>().Number = _chosenNumbers.ElementAt(i);
		}
	}

	private void SpawnBalls()
	{
		RectTransform[] childTransforms = _spawnPositions.GetComponentsInChildren<RectTransform>();

		if (childTransforms.Length < 1)
		{
			Debug.Log("No child transforms found");
		}
		else
		{
			Debug.Log(childTransforms.Length);
		}

		while (_chosenSpawns.Count < _maxNumberPicks)
		{
			_chosenSpawns.Add(childTransforms[_rng.Next(0, childTransforms.Length)]); //Max is exclusive.
		}

		for(int i = 0; i < _chosenSpawns.Count; i++)
		{
			_bingoBalls.Add(Instantiate(_ballPrefab, _chosenSpawns.ElementAt(i).position, Quaternion.identity, _ballCanvas.transform)); //Spawn the balls at random positions
			BingoBall current = _bingoBalls[_bingoBalls.Count - 1].GetComponent<BingoBall>();

			current.Manager = _manager; //Assign the manager object
			current.OwningPlayer = this; //Assign the manager object
			current.DisableText();
			current.transform.SetAsFirstSibling();
		}
	}

	#endregion

	#region Public Methods

	public void ToggleAutoMark()
	{
		_autoMark = !_autoMark;

		if (_autoMark)
		{
			_autoMarkButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Auto-mark: On";
		}
		else
		{
			_autoMarkButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Auto-mark: Off";
		}
	}

	public void EnableBallTexts()
	{
		foreach (GameObject i in _bingoBalls)
		{
			i.GetComponent<BingoBall>().EnableText();
		}
	}

	public void RemoveBall(GameObject ball)
	{
		_bingoBalls.Remove(ball);
	}

	#endregion

	#region Monobehaviours

	private void Awake()
	{
		_manager = _ballCanvas.GetComponent<BingoManager>();
		_manager.AddPlayer(this);
		_manager.onGameStart += EnableBallTexts;
		_manager.onGameStart += () =>
		{
			PickNumbers();
		}; //Requires waiting for the bingo manager to initialise before this class can grab the callout number order.

		_autoMarkButton.onClick.AddListener(ToggleAutoMark); //Setup the event listener for button clicks.
		SpawnBalls();
	}

	private void Start()
	{

	}

	private void Update()
	{
	}

	#endregion

}
