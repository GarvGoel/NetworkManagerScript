using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class FortuneWheel : MonoBehaviour
{
     public float speed =0;
     bool wheelActive = true;
    public int numberOfSections=1;

    public int answer;
    Dictionary<int, float[]> sections;

    float zRotation;

    float[] finalRange = new float[2];
    bool initiateStopWheel = false;

    


    enum RotationMode { speedUp, speedDown}
    RotationMode rotationMode;

    // Start is called before the first frame update
    void Start()
    {
        rotationMode = RotationMode.speedUp;
        sections = new Dictionary<int, float[]>();
        finalRange = new float[2];

        float value = 360 / numberOfSections;
        float halfValue = value - (value / 2);
        
        for(int i = 0; i < numberOfSections; i++)
        {
            float[] values= new float[2];
            values[0] = (i * value) - halfValue;
            values[1] = (i * value) + halfValue; ;
            sections.Add(i, values);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
        zRotation = transform.rotation.eulerAngles.z;

        if (rotationMode == RotationMode.speedUp)
        {
            speed++;
            if (speed >= 420)
            {
                rotationMode = RotationMode.speedDown;
            }
        }
        else //rotationMode == RotationMode.speedDown
        {
            if (initiateStopWheel == true)
            {
                if (zRotation > finalRange[0] && zRotation < finalRange[1])
                {
                    if (speed > 0)
                        speed = speed - 0.7f;  // this 0.7 speed can be variable for different fortune wheels
                    else
                        speed = 0;


                }
            }

            if (speed <= 51 && !wheelActive)
            {
                if (sections.ContainsKey(answer))
                {
                    // Debug.Log("Contains key " + answer);
                    finalRange = sections[answer];
                    //Debug.Log(finalRange[0] + "," + finalRange[1]);

                    initiateStopWheel = true;


                }
            }
        }
    }

    private void Rotate()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);

        if (!wheelActive)
        {

            if(speed>=50)  //speed decrese krne ko bhi thoda kam kr time ke saath or acha lgega
            {
                if (speed > 400)
                    speed = speed - 0.8f;
                else if (speed > 300)
                    speed = speed - 0.65f;
                else if (speed > 200)
                    speed = speed - 0.50f;
                else if (speed > 100)
                    speed = speed - 0.30f;
                else
                    speed = speed - 0.15f;
            }
        }
    }


    public void ShowAnswer()
    {
        wheelActive=false;

        
        
    }

    public void ResetButton()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        speed = 0;
        wheelActive = true;
        finalRange = new float[2];
        initiateStopWheel = false;
        rotationMode = RotationMode.speedUp;
    }

}
