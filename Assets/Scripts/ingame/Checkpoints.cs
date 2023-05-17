using System.Collections.Generic;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    [HideInInspector] // 인스펙터창에 변수가 보이지않게 해줌
    public List<GameObject> chk = new List<GameObject>();


    void Start()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if (this.transform.GetChild(i).CompareTag("Checkpoint") == true)
                chk.Add(this.transform.GetChild(i).gameObject);
        }
    }

    public int ExtractNumberFromString(string s1)
    {
        return System.Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(s1, "[^0-9]", ""));
    }

    public string GetNextCheckpointName(string CurrentCheckpointName)
    {
        int CurrentNumber = ExtractNumberFromString(CurrentCheckpointName);
        int NextNumber = (CurrentNumber + 1 > chk.Count - 1) ? 0 : CurrentNumber + 1;
        string NextCheckpointName = string.Format("chk ({0})", NextNumber);
        return NextCheckpointName;
    }
}
