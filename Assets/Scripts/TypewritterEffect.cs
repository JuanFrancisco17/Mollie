using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypewritterEffect : MonoBehaviour
{
    float speed = 20f;
    //diccionario de signos de puntuacion con el tiempo que duran
    private readonly Dictionary<HashSet<char>, float> punctuations = new Dictionary<HashSet<char>, float>()
    {
        {new HashSet<char>(){'.','!','?'}, 0.2f},
        {new HashSet<char>(){',',';',':'}, 0.05f},
    };

    public Coroutine Run(string textToType, TMP_Text textLabel)
    {
        //Devuelve la corrutina que escribe
        return StartCoroutine(TypeText(textToType, textLabel));
    }
    private IEnumerator TypeText(string textToType, TMP_Text textLabel)
    {
        float t = 0;
        //el numero de caracteres escritos
        int charIndex = 0;
        while (charIndex < textToType.Length)
        {
            int lastCharIndex = charIndex;


            t += Time.deltaTime * speed;
            //esto hace que cualquier numero con decimal siempre este redondeado al minimo. eg: 2.6 ----> 2
            //con el clamp obligamos a que no se pase de numero de caracteres
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);

            //leemos el ultimo caracter escrito para ver si es un simbolo de puntuacion
            for (int i = lastCharIndex; i < charIndex; i++)
            {
                //comprobamos si es el ultimo char
                bool isLast = i >= textToType.Length - 1;
                //esto escribira el texto
                textLabel.text = textToType.Substring(0, i + 1);

                //si el char es un signo, no es el ultimo char y el siguiente char no es un signo tb entonces espera un tiempo
                if (IsPunctuation(textToType[i], out float waitTime) && !isLast && !IsPunctuation(textToType[i + 1], out _))
                {
                    yield return new WaitForSeconds(waitTime);
                }

            }
            yield return null;
        }
        textLabel.text = textToType;
    }

    private bool IsPunctuation(char character, out float waitTime)
    {
        //comprueba cada categoria y si contiene el signo de puntuacion entonces establece un tiempo de espera
        foreach (KeyValuePair<HashSet<char>, float> punctuationCategory in punctuations)
        {
            if (punctuationCategory.Key.Contains(character))
            {
                waitTime = punctuationCategory.Value;
                return true;
            }
        }
        waitTime = default;
        return false;
    }
}
