
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

        //variables for data collection algorithm
        private float first;     //first vibration
        private float second;    //second vibration 
        private float step;      //Step size for vibration change
        private int incorrectguess; //Number of incorrect guesses
        private float current;  //The float passed to hand haptics script
        private float variable = 0; //checks what option is chosen
        private int differencedetected; //checks is previous ans was right (1) or wrong (0)
        private int iterations;         //total number of iterations

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
        int[] movingaverage = new int[10]; //Constantly checks the 10 most recent answers


        //We initalise all variables
        public void Start()
        {
            for(int i = 0; i < movingaverage.Length; i++)
            {
                movingaverage[i] = 1;
            }
            iterations = 0;
            differencedetected = 1; 
            variable = 0f; 
            current = 0f;
            incorrectguess = 0;
            first = start;
            second = target;
            step = initstep;
            //Path variable used to label the files of different participants
            path = Application.dataPath + "/TARGET="+ target + DateTime.Now.ToString("MMddHHmm") + ".text";
            mypanel = GameObject.Find("Panel");
            mypanel.SetActive(true);
            button1.gameObject.SetActive(false);//left button
            button2.gameObject.SetActive(false);//right button
            button3.gameObject.SetActive(true); //start(middle) button
            button3.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
            //Here we write labels for data collections
            File.AppendAllText(path, "first\tSecond\tStep\tvariable\tdifferencedetected\n");
            File.AppendAllText(path, first + "\t" + second + "\t" + step + "\t" + variable + "\t"+ differencedetected +"\n");
            
        }

        /*
        Scripts below runs when start game is pressed or when an answer is given.
        The game essentially runs in an infinite loop until an exit condition is met
        */

        public void PlayGame()
        {
            iterations++;
            button3.gameObject.SetActive(false);//middle
            button1.gameObject.SetActive(false);//left
            button2.gameObject.SetActive(false);//right
            MyText.text = "Which vibration do you feel as stronger"; 
            StartCoroutine(PlayOptions());
        }

        /*
        Here user is presented with the 2 different vibration intensities
        */
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

            //After vibrations we go to 'choose' pannel
            MyText.text = "Choose";
            current = 0f;
            button1.gameObject.SetActive(true);//left button (first vibration)
            button2.gameObject.SetActive(true);//right button (second vibration)
            button3.gameObject.SetActive(true);//middle button (not sure)
            button3.GetComponentInChildren<TextMeshProUGUI>().text = "Not sure";

        }


        //if button 1 is pressed (i.e. user chose 'first')
        public void option1()
        {
            if (first > second) //if answer is correct
            {
                if (differencedetected==1)//if past answer was correct
                {
                    if (first == target) second += step; //we go up towards target
                    else first -= step;
                }
                //If we didn't detect this vibration earler we change set flag now
                differencedetected = 1;
                
            }
            else //if second is bigger than first 
            {
                differencedetected = 0;
                incorrectguess++;
                if (first == target) second += step; //we go back
                else first -= step;
                step *= 0.67f;
            }
            EndOfGame(); //Checks for exit conditions and log info
        }


        //if button 2 is pressed (i.e. user chose 'second')
        public void option2()
        {
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
                incorrectguess++;
                if (second == target) first += step;//push first back up
                else second -= step; //or push second back down
                step *= 0.67f;
            }
            EndOfGame();
        }

        public void Notsure()
        {
            incorrectguess++;
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
            if (step < 0.01f) step = 0.01f; //making sure step doesn't get too small

            Debug.Log(movingaverage.Length);
            Array.Copy(movingaverage, 0, movingaverage, 1, movingaverage.Length-1); //shift answers to right
            movingaverage[0] = differencedetected; // Assign new value to the beginning of the array

            //Make sure intensities are within 0-100% range
            first = Math.Clamp(first, 0, 1);
            second = Math.Clamp(second, 0, 1);

            if (first == target) variable = second;
            else variable = first;

            //Log data
            File.AppendAllText(path, first + "\t" + second + "\t" + step + "\t" + variable + "\t" + differencedetected + "\n");
            
            //randomize first and second vibration
            float randomFloat = UnityEngine.Random.Range(0f, 1f);
            if (randomFloat > 0.5f)
            {
                float hold = first;
                first = second;
                second = hold;
            }

            /*
            Checking for exit conditions
            Conditions 1: 30% or less of last 10 answers were correct
            Condition 2: More than 25 iterations have passed
            */
            if (((iterations>=10)&(movingaverage.Sum()<=3))|(iterations>25))
            {
                button1.gameObject.SetActive(false);    //left
                button2.gameObject.SetActive(false);    //right
                button3.gameObject.SetActive(false);    //middle
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