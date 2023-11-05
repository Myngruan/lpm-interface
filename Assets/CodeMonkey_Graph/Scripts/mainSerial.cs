using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Globalization;

namespace grafico
{
    public class mainSerial : MonoBehaviour, IDataReceiver
    {
        private SerialPort serialPort;
        public string serialPortName = "COM3"; 
        public int baudRate = 115200; 

        private Window_Graph_Aceleracao_X graphAceleracao_X;
        private Window_Graph_Aceleracao_Y graphAceleracao_Y;
        private Window_Graph_Aceleracao_Z graphAceleracao_Z;
        private Window_Graph_Deformacao_Bra1 graphDeformacao_Bra1;
        private Window_Graph_Deformacao_Bra2 graphDeformacao_Bra2;
        private Window_Graph_Pressao graphPressao;
        private Window_Graph_Inclinacao_X graphInclinacao_X;
        private Window_Graph_Inclinacao_Y graphInclinacao_Y;
        private Window_Graph_Inclinacao_Z graphInclinacao_Z;

        private float lastDeformacao_Bra1;
        private float lastDeformacao_Bra2;
        private float lastAceleracao_X;
        private float lastAceleracao_Y;
        private float lastAceleracao_Z;
        private float lastPressao;
        private float lastInclinacao_X;
        private float lastInclinacao_Y;
        private float lastInclinacao_Z;
        private float lastTemperatura;


        public event System.Action<float, float, float> OnDataReceived;

        private void Start()
        {
           
            serialPort = new SerialPort(serialPortName, baudRate);
            try
            {
                serialPort.Open();
            }
            catch (System.Exception)
            {
                Debug.LogError("Erro ao abrir Porta Serial " + serialPortName);
            }


            graphDeformacao_Bra1 = FindObjectOfType<Window_Graph_Deformacao_Bra1>();
            graphDeformacao_Bra2 = FindObjectOfType<Window_Graph_Deformacao_Bra2>();
            graphInclinacao_X = FindObjectOfType<Window_Graph_Inclinacao_X>();
            graphInclinacao_Y = FindObjectOfType<Window_Graph_Inclinacao_Y>();
            graphInclinacao_Z = FindObjectOfType<Window_Graph_Inclinacao_Z>();
            graphAceleracao_X = FindObjectOfType<Window_Graph_Aceleracao_X>();
            graphAceleracao_Y = FindObjectOfType<Window_Graph_Aceleracao_Y>();
            graphAceleracao_Z = FindObjectOfType<Window_Graph_Aceleracao_Z>();

            graphPressao = FindObjectOfType<Window_Graph_Pressao>();



            if (serialPort.IsOpen)
            {
                Debug.Log("PORTA ABERTA");
                try
                {
                    string data = serialPort.ReadLine();
                    Debug.Log("Dados reccebidos: " + data);
                                                         
                }
                catch (System.Exception)
                {
                    Debug.Log("Erro ao abrir Porta Serial");
                }
            }
            }

        private void Update()
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    string data = serialPort.ReadLine();
                    Debug.Log("Dados recebidos: " + data); 
                    string[] values = data.Split(',');

                    if (values.Length >= 10) 
                    {

                        float deformacao_bra1 = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                        float deformacao_bra2 = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                        float inclinacaox = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                        float inclinacaoy = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                        float inclinacaoz = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);

                        float aceleracaox = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                        float aceleracaoy = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                        float aceleracaoz = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);

                        float pressao = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);

                        float temperatura = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);

                        Debug.Log("Deformacao_braco1: " + deformacao_bra1 + "Deformacao_braco2: " + deformacao_bra2 + "Inclinacao_X: " + inclinacaox + "Inclinacao_Y: " + inclinacaoy + "Inclinacao_Z: " + inclinacaoz+ " Aceleracao_X: " + aceleracaox + " Aceleracao_Y: " + aceleracaoy + " Aceleracao_Z: " + aceleracaoz + " Pressão: " + pressao + "Temperatura" + temperatura );

                        lastDeformacao_Bra1= deformacao_bra1;
                        lastDeformacao_Bra2 = deformacao_bra2;
                        lastInclinacao_X = inclinacaox;
                        lastInclinacao_Y = inclinacaoy;
                        lastInclinacao_Z = inclinacaoz;

                        lastAceleracao_X = aceleracaox;
                        lastAceleracao_Y = aceleracaoy;
                        lastAceleracao_Z = aceleracaoz;
                        lastPressao = pressao;
                        lastTemperatura = temperatura;

                        graphDeformacao_Bra1.ReceiveDeformacao_Bra1(deformacao_bra1);
                        graphDeformacao_Bra2.ReceiveDeformacao_Bra2(deformacao_bra2);   
                        graphInclinacao_X.ReceiveInclinacao_X(inclinacaox);
                        graphInclinacao_Y.ReceiveInclinacao_Y(inclinacaoy);
                        graphInclinacao_Z.ReceiveInclinacao_Z(inclinacaoz);

                        graphAceleracao_X.ReceiveAceleracao_X(aceleracaox);
                        graphAceleracao_Y.ReceiveAceleracao_Y(aceleracaoy);
                        graphAceleracao_Z.ReceiveAceleracao_Z(aceleracaoz);

                        graphPressao.ReceivePressao(pressao);

                        // Notifica os observadores sobre os dados recebidos
                        OnDataReceived?.DynamicInvoke(deformacao_bra1, deformacao_bra2, inclinacaox, inclinacaoy, inclinacaoz, aceleracaox, aceleracaoy, aceleracaoz, pressao);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Erro ao ler dados da porta serial: " + ex.Message);
                }
            }
        }

        // Métodos para obter o último valor 
        public float GetLastDeformacao_Bra1()
        {
            return lastDeformacao_Bra1;
        }
        public float GetLastDeformacao_Bra2()
        {
            return lastDeformacao_Bra2;
        }
        public float GetLastInclinacao_X()
        {
            return lastInclinacao_X;
        }
        public float GetLastInclinacao_Y()
        {
            return lastInclinacao_Y;
        }
        public float GetLastInclinacao_Z()
        {
            return lastInclinacao_Z;
        }


        public float GetLastAceleracao_X()
        {
            return lastAceleracao_X;
        }

        public float GetLastAceleracao_Y()
        {
            return lastAceleracao_Y;
        }
        public float GetLastAceleracao_Z()
        {
            return lastAceleracao_Z;
        }

        public float GetLastPressao()
        {
            return lastPressao;
        }
        public float GetLastTemperatura()
        {
            return lastTemperatura;
        }

        public void ReceiveDeformacao_Bra1(float deformacao_bra1)
        {
        }
        public void ReceiveDeformacao_Bra2(float deformacao_bra2)
        {
        }

        public void ReceiveInclinacao_X(float inclinacao_X)
        {
        }
        public void ReceiveInclinacao_Y(float inclinacao_Y)
        {
        }
        public void ReceiveInclinacao_Z(float inclinacao_Z)
        {
        }
        public void ReceiveAceleracao_X(float aceleracao_X)
        {
        }

        public void ReceiveAceleracao_Y(float aceleracao_Y)
        {
        }

        public void ReceiveAceleracao_Z(float aceleracao_Z)
        {
        }
        public void ReceivePressao(float pressao)
        {
        }

        private void OnDestroy()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

    }
}
