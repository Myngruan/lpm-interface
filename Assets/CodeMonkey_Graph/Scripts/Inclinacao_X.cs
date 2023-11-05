using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace grafico
{
    public class Inclinacao_X : MonoBehaviour
    {
        public Text valorInclinacao_X;

        private mainSerial serialController;

        private void Start()
        {

            serialController = FindObjectOfType<mainSerial>();

            StartCoroutine(UpdateInclinacaoText());
        }

        private IEnumerator UpdateInclinacaoText()
        {
            while (true)
            {
                if (serialController != null)
                {
                    float lastInclinacao_X = serialController.GetLastInclinacao_X();

                    valorInclinacao_X.text = lastInclinacao_X.ToString("F2") + " graus";

                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
