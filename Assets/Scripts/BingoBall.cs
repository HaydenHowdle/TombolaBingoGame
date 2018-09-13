using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using UnityEngine.UI;

public class BingoBall : MonoBehaviour
{
	#region References

	public TextMeshProUGUI _numberText = null;
	private BingoManager _manager = null; //Game Manager
	private Player _owningPlayer = null; //Player object that owns this

	#endregion

	#region Private

	private int _number = 0; //The assigned number for this 'ball'.

	#endregion

	#region Properties

	public int Number
	{
		get
		{
			return _number;
		}

		set
		{
			_number = value;
			_numberText.text = value.ToString();
		}
	}

	public BingoManager Manager
	{
		get
		{
			return _manager;
		}

		set
		{
			_manager = value;
			_manager.onNumberCalled += HandleNumberCallout;
		}
	}

	public Player OwningPlayer
	{
		get
		{
			return _owningPlayer;
		}

		set
		{
			_owningPlayer = value;
		}
	}

	public void EnableText()
	{
		_numberText.gameObject.SetActive(true);
	}

	public void DisableText()
	{
		_numberText.gameObject.SetActive(false);
	}

	#endregion

	#region Private Methods

	private void HandleNumberCallout(int num)
	{
		if (Number == num)
		{
			if (_owningPlayer.AutoMark)
			{
				_manager.onNumberCalled -= HandleNumberCallout; //Unsubscribe from the event
				_owningPlayer.RemoveBall(gameObject);
				GameObject.Destroy(gameObject); //Destroy itself.
			}
			else
			{
				GetComponentInChildren<Button>().interactable = true; //Enable the button
			}
		}
		else
		{
			GetComponentInChildren<Button>().interactable = false; //Turn off the button
		}
	}

	#endregion

	#region Public Methods

	#endregion

	#region Monobehaviours

	private void Awake()
	{
		GetComponentInChildren<Button>().onClick.AddListener(() =>
		{
			_manager.onNumberCalled -= HandleNumberCallout; //Unsubscribe from the event
			_owningPlayer.RemoveBall(gameObject);
			Destroy(gameObject);
		}
		);
	}

	private void Start()
	{
		
	}

	private void Update()
	{
		
	}

	#endregion

}
