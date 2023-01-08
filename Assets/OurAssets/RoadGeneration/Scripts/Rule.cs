using UnityEngine;

[CreateAssetMenu(menuName = "ProceduralRoads")]
public class Rule : ScriptableObject
{
    public string triggerLetter;
    
    [SerializeField]
    private string[] results = null;
    [SerializeField]
    private bool randomResult = false;

    public string GetResult()
    {
        int randomIndex = UnityEngine.Random.Range(0, results.Length);
        return results[randomIndex];
    }
   
}
