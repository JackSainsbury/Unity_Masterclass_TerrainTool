using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attribute
{
	public string name;
	public object value;

	// Attributes added at runtime, initially provide the editor assigned default values, then check if I am to read from or write to the settings file
	public Attribute (string newName, object newValue)
	{
		name = newName;
		value = newValue;
	}
}

[System.Serializable]
public class MessageSocket
{
	public string socketTag;
	public string socketMethod;
}

// Parent class for all widgets
public class Widget : MonoBehaviour 
{
	public string m_managerTag;

	//Set of message sockets to run
	public MessageSocket[] m_valueConfirmSockets;


	public List<Attribute> m_attributesList;

	public Widget()
	{
		m_attributesList = new List<Attribute> ();
	}

	// Adds a new attribute to the attribute list and concretes its value type
	public void AddAttribute <T> (string attributeName, T value)
	{
		// Add a new attribute to the list, getting default value from manager
		m_attributesList.Add (new Attribute (attributeName, value));
	}

	// Run all messages
	public void AllMethodMessages(){
		foreach (MessageSocket socket in m_valueConfirmSockets) {
			GameObject.FindGameObjectWithTag (socket.socketTag).SendMessage (socket.socketMethod);
		}
	}

	// Get an attribute by name AND specified type
	public T GetAttribute<T> (string attributeName)
	{
		int failIndex = -1;

		// If I found the value, set it and bomb out
		for (int i = 0; i < m_attributesList.Count; ++i) 
		{
			if (m_attributesList [i].name == attributeName) 
			{
				if (m_attributesList [i].value.GetType () == typeof(T)) 
				{
					return (T)m_attributesList[i].value;
				} else {
					failIndex = i;
				}
			}
		}

		if (failIndex != -1) 
		{
			Debug.LogError ("Attribute " + attributeName + " was found, value: " + m_attributesList[failIndex].value + " is not of type ( " + typeof(T) + " )");
		} else {
			//attribute was not found
			Debug.LogError ("Could not find attribute: " + attributeName);
		}

		return default(T);
	}

	// General interface function
	public void SetAttribute <T> (string attributeName, T value)
	{
		int failIndex = -1;

		// If I found the value, set it and bomb out
		for (int i = 0; i < m_attributesList.Count; ++i) 
		{
			if (m_attributesList [i].name == attributeName) 
			{
				if (m_attributesList [i].value.GetType () == value.GetType ()) 
				{
					m_attributesList [i].value = value;
					return;
				} else 
				{
					failIndex = i;
				}
			}
		}

		if (failIndex != -1)
		{
			Debug.LogError ("Attribute " + attributeName + " was found, but is of type " + m_attributesList[failIndex].value.GetType() + " failed to assign type of: " + value.GetType () + "( '" + value + "' )");
		} else 
		{
			//attribute was not found
			Debug.LogError ("Could not find attribute");
		}
	}
}
