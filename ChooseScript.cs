
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEditor;
using System.Linq;

namespace Manus.Haptics
{


    [DisallowMultipleComponent]
    [AddComponentMenu("Manus/Haptics/Finger (Haptics)")]
    public class ChooseScript : MonoBehaviour
    {

        //variables for experiment
        private float first;
        private float second;   //vibration I change 
        private float step;        //Size of step that changes vibration
        private int switchcount; //The amounts of time I switched from up/down
        private float current; //The float passed to hand haptics
        private float variable = 0; //checks what option is chosen
        private int differencedetected;
        private int iterations;

        //UI VARIABLES
        public Button button1; //left
        public Button button2; //right
        public Button button3; //middle
        public TextMeshProUGUI MyText;
        public float target; //base intensity
        public float start;
        public float initstep;
        GameObject mypanel;
        string path = "";
        int[] movingaverage = new int[10];

        //this is startup for game
        public void Start()
        {
            for(int i = 0; i < movingaverage.Length; i++)
{
                movingaverage[i] = 1;
            }
            iterations = 0;
            differencedetected = 1;
            variable = 0f; //to see which was picked
            current = 0f; //passed to haptic glove
            switchcount = 0;
            first = start; //we start from 1f
            second = target;
            step = initstep; // initial step
            //path = Application.dataPath + "/"+DateTime.Now.ToString("yyyyMMddHHmmss")+".text";
            path = Application.dataPath + "/TARGET="+ target + DateTime.Now.ToString("MMddHHmm") + ".text";
            //StartCoroutine(PlayOptions()); //for delay
            mypanel = GameObject.Find("Panel");
            mypanel.SetActive(true);
            button1.gameObject.SetActive(false);//left
            button2.gameObject.SetActive(false);//right
            button3.gameObject.SetActive(true); //start button
            button3.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
            File.AppendAllText(path, "first\tSecond\tStep\tvariable\tdifferencedetected\n");
            File.AppendAllText(path, first + "\t" + second + "\t" + step + "\t" + variable + "\t"+ differencedetected +"\n");
            
        }
        //Start Game screen
        public void PlayGame()
        {
            iterations++;
            //button3.GetComponentInChildren<TextMeshProUGUI>().text = "Not sure";
            button3.gameObject.SetActive(false);//middle
            button1.gameObject.SetActive(false);//left
            button2.gameObject.SetActive(false);//right
            MyText.text = "Which vibration do you feel as stronger";
            StartCoroutine(PlayOptions());
        }

        //Once play is pressed
        IEnumerator PlayOptions()
        {
            yield return new WaitForSeconds(2);

            MyText.text = "Vibration 1...";
            current = first;
            yield return new WaitForSeconds(1.5f);

            MyText.text = "Or..";
            current = 0f;
            yield return new WaitForSeconds(2.5f);


            MyText.text = "Vibration 2..";
            current = second;
            yield return new WaitForSeconds(1.5f);

            MyText.text = "Choose";
            current = 0f;
            button1.gameObject.SetActive(true);//left
            button2.gameObject.SetActive(true);//right
            button3.gameObject.SetActive(true);//middle
            button3.GetComponentInChildren<TextMeshProUGUI>().text = "Not sure";

        }

        //if button 1 is pressed
        public void option1()
        {
            //choice = first;
            if (first > second) //if answer is correct
            {
                if (differencedetected==1)//if past answer was correct
                {
                    if (first == target) second += step;//we go up towards target
                    else first -= step;
                }
                //else repeat same step
                differencedetected = 1;
                
            }
            else //if second is bigger than first 
            {
                differencedetected = 0;
                switchcount++;
                if (first == target) second += step; //we go back
                else first -= step;
                step *= 0.67f;
            }
            EndOfGame();
        }

        //if button 2 is pressed
        public void option2()
        {
        
            //choice = second;
            if (second > first) //if answer is correct
            {
                if (differencedetected == 1)
                {
                    if (second == target) first += step;
                    else second -= step;
                }
                //else repeat same step
                differencedetected = 1;
                
            }
            else //if first is bigger than second 
            {
                differencedetected = 0;
                switchcount++;
                if (second == target) first += step;//push first back up
                else second -= step; //or push second back down
                step *= 0.67f;
            }
            EndOfGame();
        }
        public void Notsure()
        {
            switchcount++;
            //iterations--; //weird glitch
            if(button3.GetComponentInChildren<TextMeshProUGUI>().text=="Not sure")
            {
                differencedetected = 0;
                //choice = 0;
                if (first > second)
                {
                    if (first == target) second -= step; //go back a step
                    else first += step;
                    step *= 0.67f; //make smaller step
                }
                else //if second bigger than first
                {
                    if (first == target) second += step;
                    else first -= step;
                    step *= 0.67f;
                }
            }
            EndOfGame();
            
        }

        //function for checking if we reached end of game
        public void EndOfGame()
        {
            if (step < 0.01f) step = 0.01f;
            Debug.Log(movingaverage.Length);
            // Shift elements to the right
            Array.Copy(movingaverage, 0, movingaverage, 1, movingaverage.Length-1);

            // Assign new value to the beginning of the array
            movingaverage[0] = differencedetected;

            // Discard last value
            //Array.Resize(ref movingaverage, movingaverage.Length - 1);
            first = Math.Clamp(first, 0, 1);
            second = Math.Clamp(second, 0, 1);
            if (first == target) variable = second;
            else variable = first;

            File.AppendAllText(path, first + "\t" + second + "\t" + step + "\t" + variable + "\t" + differencedetected + "\n");
            //randomize first and second
            float randomFloat = UnityEngine.Random.Range(0f, 1f);
            if (randomFloat > 0.5f)
            {
                //Debug.Log("Swapping");
                float hold = first;
                first = second;
                second = hold;
            }
            //Debug.Log(first + "\t" + second);
            //If game is over
            if (((iterations>=10)&(movingaverage.Sum()<=3))|(iterations>25))
            {
                button1.gameObject.SetActive(false);//left
                button2.gameObject.SetActive(false);//right
                button3.gameObject.SetActive(false);//middle
                MyText.text = "Thank you for taking part in our survey";
                EditorApplication.isPlaying = false;
            }
            else
            {
                PlayGame();
            }
            
        } 

        public float PassData()
        {
            return(current);
        }

    }
}