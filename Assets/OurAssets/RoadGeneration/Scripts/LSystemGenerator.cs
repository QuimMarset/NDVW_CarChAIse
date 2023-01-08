using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LSystemGenerator : MonoBehaviour
{

    public Rule[] rules;
    public string initialSentence;
    [Range(0, 20)]
    public int iterationLimit;

    // Set a probability to ignore creating a new branch (not applying a rule result)
    [Range(0, 1)]
    public float probabilityToIgnoreARule;

    public string finalSentence;


    private void Awake()
    {
        finalSentence = GenerateSentence();
    }


    public string GenerateSentence()
    {
        return GrowRecursive(initialSentence, 0);
    }

    private string GrowRecursive(string word, int currentIteration)
    {
        if (currentIteration >= iterationLimit)
        {
            return word;
        }

        StringBuilder newWord = new();

        foreach (char character in word)
        {
            newWord.Append(character);
            ProcessRulesRecursively(newWord, character, currentIteration);
        }

        return newWord.ToString(); 
    }

    private void ProcessRulesRecursively(StringBuilder newWord, char character, int currentIteration)
    {
        foreach (Rule rule in rules)
        {
            if (rule.triggerLetter == character.ToString())
            {
                if (currentIteration > 1 && UnityEngine.Random.value < probabilityToIgnoreARule)
                {
                    return;
                }
                newWord.Append(GrowRecursive(rule.GetResult(), currentIteration + 1));
            }
        }
    }
}
