using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace grafico
{
    public class Inclinacao_Z : MonoBehaviour
    {
        public Text valorInclinacao_Z;

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
                    float lastInclinacao_Z = serialController.GetLastInclinacao_Z();

                    valorInclinacao_Z.text = lastInclinacao_Z.ToString("F2") + " graus";

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
