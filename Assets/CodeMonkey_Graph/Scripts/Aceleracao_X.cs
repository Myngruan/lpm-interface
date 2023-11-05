using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

namespace grafico
{
    public class Aceleracao_X : MonoBehaviour
    {
        public Text valorAceleracao_X; 

        private mainSerial serialController; 

        private void Start()
        {
            
            serialController = FindObjectOfType<mainSerial>();
            StartCoroutine(UpdateAceleracaoText());
        }

        private IEnumerator UpdateAceleracaoText()
        {
            while (true)
            {
                if (serialController != null)
                {
                    float lastAceleracao_X = serialController.GetLastAceleracao_X();

                    valorAceleracao_X.text = lastAceleracao_X.ToString("F2") + " m/s^2"; 

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
