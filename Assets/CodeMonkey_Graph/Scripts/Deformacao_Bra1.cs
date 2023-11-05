using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

namespace grafico
{
    public class Deformacao_Bra1 : MonoBehaviour
    {
        public Text valorDeformacao_Bra1; 

        private mainSerial serialController;

        private void Start()
        {

            serialController = FindObjectOfType<mainSerial>();

  
            StartCoroutine(UpdateDeformacaoText());
        }

        private IEnumerator UpdateDeformacaoText()
        {
            while (true)
            {
                if (serialController != null)
                {
                   
                    float lastDeformacao_Bra1 = serialController.GetLastDeformacao_Bra1();

                    valorDeformacao_Bra1.text = lastDeformacao_Bra1.ToString("F2") + " m"; 

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
