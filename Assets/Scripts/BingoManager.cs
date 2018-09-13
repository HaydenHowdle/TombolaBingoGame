using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using System.Timers;

public enum GameState
{
	PreGame,
	Game,
	PostGame,
}

/// <summary>
/// Controls the core logic of the game.
/// <para>The numbers that will be called out are generated entirely at the start of the game to avoid any potential issues or delays during a game.</para>
/// <para>If more numbers are needed (Unlikely) more will be generated.</para>
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BingoManager : MonoBehaviour
{
	#region Editor References

	[Tooltip("The text object used to display the callout")]
	public TextMeshProUGUI _calloutText = null;

	[Tooltip("The path that the audio clips for the number callouts are stored.")]
	public string _calloutAudioPath = "Assets/Resources/Audio/Callouts";

	[Tooltip("UI to show when game ends")]
	public GameObject _postGameUI = null;

	[Tooltip("Text box to display winner names")]
	public TextMeshProUGUI _postGameWinnerText = null;

	[Tooltip("UI to display before the game starts")]
	public GameObject _preGameUI = null;

	[Tooltip("Textbox to display remaining time before game starts")]
	public TextMeshProUGUI _preGameCountdownText = null;

	private AudioSource _calloutAudioSource = null;
	#endregion

	#region Private

	private const int _minNumber = 1; //Minimum number that can be called out.
	private const int _maxNumber = 36; //Max number that can be called out.

	private System.Random _rng = new System.Random(Guid.NewGuid().GetHashCode()); //Using a new guid's hash code is less predictable than using current time.

	private List<int> _availableNumbers = Enumerable.Range(_minNumber, _maxNumber).ToList(); //A list containing the numbers 1 - 36.
	private AudioClip[] _numberAudioClips = new AudioClip[_maxNumber]; //Contains a list of the number readout clips, easier to load more numbers this way if ever changed.
	private List<Player> _players = new List<Player>(); //List of players, games may not have a fixed requirement of players to start the game so list is used here.
	private Player[] _winners = null; //List of players who have won, null equals no players have won yet.
	private HashSet<int> _calloutOrder = new HashSet<int>(); //Numbers pre-chosen for the demo;

	private GameState _state = GameState.PreGame; //State used to track which state the game is currently in.
	private bool _isCallingOut = false; //Guard to prevent multiple cooroutines being ran.
	private float _pregameCountdown = 8; //Count down timer for pre-game
	private int _index = 0; //Current number to read out.


	#endregion

	#region Events	

	public event Action<int> onNumberCalled;
	public event Action onGameStart;

	#endregion

	#region Properties

	/// <summary>
	/// Array of numbers that will be called out over the game.
	/// </summary>
	public HashSet<int> CalloutOrder
	{
		get
		{
			return _calloutOrder;
		}
	}

	#endregion

	#region Private Methods

	private void LoadAudioClips()
	{
		for (int i = 0; i < _maxNumber; i++)
		{
			_numberAudioClips[i] = Resources.Load<AudioClip>(string.Format("{0}/{1}", _calloutAudioPath, (i + 1).ToString()));
		}
	}

	/// <summary>
	/// Handles the delay and callout of numbers
	/// </summary>
	/// <returns></returns>
	private IEnumerator CalloutNumber()
	{
		_isCallingOut = true;

		CheckForWinners();

		if (_winners != null)
		{
			_state = GameState.PostGame;
			DisplayPostGameUI(); //Enable the winner UI.
			_isCallingOut = false;
			yield break;
		}


		int num = _calloutOrder.ElementAt(_index);

		yield return new WaitForSeconds(3f);

		if (_calloutAudioSource.isPlaying)
		{
			_calloutAudioSource.Stop();
		}

		_calloutText.text = num.ToString();
		_calloutAudioSource.clip = _numberAudioClips[num - 1]; //Grab the correct track for the current number. 
		_calloutAudioSource.Play(); //Play the track.
		_availableNumbers.RemoveAt(_index);

		onNumberCalled.Invoke(num); //Call all listeners with the number that was called out.

		_index += 1;

		_isCallingOut = false;
	}

	/// <summary>
	/// Checks for winners.
	/// </summary>
	/// <returns></returns>
	private void CheckForWinners()
	{
		int winnerCount = _players.Count(x => x.IsBingo);

		if (winnerCount == 0)
		{
			return; //No winners yet.
		}

		_winners = _players.Where(x => x.IsBingo).ToArray(); //Put all players who have won into the array using a linq predicate match.
	}

	private void DisplayPostGameUI()
	{
		foreach (Player i in _winners) //Print out the player names into the display text
		{
			_postGameWinnerText.text += string.Format("{0}\n", i.Name);
		}

		_postGameUI.SetActive(true); //Enable the UI.
	}

	/// <summary>
	/// Pre-generates the order of numbers to be called out
	/// </summary>
	private void PreGenNumberOrder()
	{
		while(_calloutOrder.Count < _maxNumber)
		{
			_calloutOrder.Add(_availableNumbers[_rng.Next(0, _availableNumbers.Count)]); //Exclusive max.
		}
	}

	#endregion

	#region Public Methods

	public void AddPlayer(Player player)
	{
		_players.Add(player);
	}

	#endregion

	#region Monobehaviour

	private void Awake()
	{
		_calloutAudioSource = GetComponent<AudioSource>();
	}

	private void Start()
	{
		if (_calloutText == null)
		{
			throw new ArgumentNullException("_calloutText"); //The references aren't setup, shouldn't continue.
		}

		PreGenNumberOrder();
		LoadAudioClips();
		_calloutText.text = ""; //Ensure the text is blank until a number is called out.
	}

	private void Update()
	{
		if (_state == GameState.PreGame)
		{
			if (_pregameCountdown <= 0)
			{
				_preGameUI.SetActive(false);
				_state = GameState.Game;
				onGameStart.Invoke();
			}
			else
			{
				_pregameCountdown -= Time.deltaTime;
			}

			_preGameCountdownText.text = ((int)_pregameCountdown).ToString();
		}
		else if (_state == GameState.Game)
		{
			if (!_isCallingOut) //Run only if there isn't one running already
				StartCoroutine(CalloutNumber());
		}
		else
		{
			//Do post-game stuff here
		}
	}

	#endregion
}
