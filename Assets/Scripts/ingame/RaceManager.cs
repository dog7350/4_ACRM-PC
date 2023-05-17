using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    public GameObject Cp;
    public GameObject CheckpointHolder;

    public GameObject[] Cars;
    public Transform[] CheckpointPositions;
    public GameObject[] CheckpointForEachCar;

    private int totalCars;
    private int totalCheckpoints;

    public Text PositionTxt;

    void Start()
    {
        totalCars = Cars.Length;
        totalCheckpoints = CheckpointHolder.transform.childCount;

        setCheckpoints();
        setCarPosition();
    }

    void  setCheckpoints()
    {
        CheckpointPositions = new Transform[totalCheckpoints];

        for(int i = 0; i < totalCheckpoints; i++)
        {
            CheckpointPositions[i] = CheckpointHolder.transform.GetChild(i).transform;
        }

        CheckpointForEachCar = new GameObject[totalCars];

        for(int i = 0; i < totalCars; i++)
        {
            CheckpointForEachCar[i] = Instantiate(Cp, CheckpointPositions[0].position, CheckpointPositions[0].rotation);
            CheckpointForEachCar[i].name = "CP" + i;
            CheckpointForEachCar[i].layer = 6 + i;
        }
    }

    void setCarPosition()
    {

        for (int i = 0; i < totalCars; i++)
        {
            Cars[i].GetComponent<CarCpManager>().CarPosition = i + 1;
            Cars[i].GetComponent<CarCpManager>().CarNumber = i;
        }

        PositionTxt.text = "순위 " + Cars[0].GetComponent<CarCpManager>().CarPosition + "/" + totalCars;
    }

    public void CarCollectedCp(int carNumber, int cpNumber)
    {
        CheckpointForEachCar[carNumber].transform.position = CheckpointPositions[cpNumber].transform.position;
        CheckpointForEachCar[carNumber].transform.rotation = CheckpointPositions[cpNumber].transform.rotation;

        comparePositions(carNumber);
    }

    void comparePositions(int carNumber)
    {
        if(Cars[carNumber].GetComponent<CarCpManager>().CarPosition > 1)
        {
            GameObject currentCar = Cars[carNumber];
            int currentCarPos = currentCar.GetComponent<CarCpManager>().CarPosition;
            int currentCarCp = currentCar.GetComponent<CarCpManager>().cpCrossed;

            GameObject carInFront = null;
            int carInFrontPos = 0;
            int carInFrontCp = 0;

            for(int i = 0; i < totalCars; i++)
            {
                if (Cars[i].GetComponent<CarCpManager>().CarPosition == currentCarPos - 1)
                {
                    carInFront = Cars[i];
                    carInFrontCp = carInFront.GetComponent<CarCpManager>().cpCrossed;
                    carInFrontPos = carInFront.GetComponent<CarCpManager>().CarPosition;
                    break;
                }
            }

            if(currentCarCp > carInFrontCp)
            {
                currentCar.GetComponent<CarCpManager>().CarPosition = currentCarPos - 1;
                carInFront.GetComponent<CarCpManager>().CarPosition = carInFrontPos + 1;
            }

            PositionTxt.text = "순위 " + Cars[0].GetComponent<CarCpManager>().CarPosition + "/" + totalCars;
        }
    }

}
