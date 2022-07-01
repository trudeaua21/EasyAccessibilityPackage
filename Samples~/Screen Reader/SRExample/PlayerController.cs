using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    private int count;

    public Text countText;

    
    void Start()
    {
        count = 0;
    }

    
    void Update()
    {
        // map a button input to read a game status (the current count, in our case)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //todo
        }
    }

    public void incrementCount()
    {
        count++;
        countText.text = "Count: " + count;
    }
}
