using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using System.IO.Ports;
using static grafico.Window_Graph_Deformacao_Bra1;

namespace grafico
{

    public class Window_Graph_Deformacao_Bra1 : MonoBehaviour
    {

        private static Window_Graph_Deformacao_Bra1 instance;

        [SerializeField] private Sprite dotSprite;
        private RectTransform graphContainer;
        private RectTransform labelTemplateX;
        private RectTransform labelTemplateY;
        private RectTransform dashContainer;
        private RectTransform dashTemplateX;
        private RectTransform dashTemplateY;
        private List<GameObject> gameObjectList;
        private List<IGraphVisualObject> graphVisualObjectList;
        private GameObject tooltipGameObject;
        private List<RectTransform> yLabelList;


        private List<int> dataSeries1 = new List<int>();
        private List<int> dataSeries2 = new List<int>();
        private List<Color> lineColors = new List<Color> { Color.magenta, Color.blue }; // Cores das linhas das duas curvas

        private IGraphVisual graphVisual;
        private int maxVisibleValueAmount;
        private Func<int, string> getAxisLabelX;
        private Func<float, string> getAxisLabelY;
        private float xSize;
        private bool startYScaleAtZero;

        private float receivedDeformacao_Bra1;

        private int currentIndex;
        private float updateInterval = 1f; // Intervalo em segundos para atualizar os valores

        private mainSerial serialController;
        public interface IDataReceiver
        {
            void ReceiveDeformacao_Bra1(float deformacao_bra1);
        }

        public void ReceiveDeformacao_Bra1(float deformacao_bra1)
        {
            receivedDeformacao_Bra1 = deformacao_bra1;
        }
        public void ReceiveAceleracao(float aceleracao)
        {
            
        }
        public void ReceivePressao(float pressao)
        {

        }

        public void SetSerialController(mainSerial controller)
        {
            serialController = controller;
        }

        private void Awake()
        {
            instance = this;

            graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
            labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
            labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
            dashContainer = graphContainer.Find("dashContainer").GetComponent<RectTransform>();
            dashTemplateX = dashContainer.Find("dashTemplateX").GetComponent<RectTransform>();
            dashTemplateY = dashContainer.Find("dashTemplateY").GetComponent<RectTransform>();
            tooltipGameObject = graphContainer.Find("tooltip").gameObject;

            startYScaleAtZero = true;
            gameObjectList = new List<GameObject>();
            yLabelList = new List<RectTransform>();
            graphVisualObjectList = new List<IGraphVisualObject>();

            IGraphVisual lineGraphVisual1 = new LineGraphVisual(graphContainer, dotSprite, Color.magenta, new Color(1, 1, 1, .5f));
            IGraphVisual lineGraphVisual2 = new LineGraphVisual(graphContainer, dotSprite, Color.blue, new Color(1, 1, 1, .5f));

            IGraphVisual barChartVisual1 = new BarChartVisual(graphContainer, Color.magenta, .8f);
            IGraphVisual barChartVisual2 = new BarChartVisual(graphContainer, Color.blue, .8f);
           
            HideTooltip();

            List<int> dataSeries1 = new List<int>() { 0, 0, 0, 0, 0 };
            List<int> dataSeries2 = new List<int>() { 0, 0, 0, 0, 0 };


            // Mostrar o primeiro gráfico
            ShowGraph(dataSeries1, lineGraphVisual1, maxVisibleValueAmount, getAxisLabelX, getAxisLabelY);

            currentIndex = dataSeries1.Count - 1; // Começar pelo último ponto

            FunctionPeriodic.Create(() => {
                UpdateValues(dataSeries1, ref currentIndex, receivedDeformacao_Bra1);
                // Atualizar o primeiro gráfico
                ShowGraph(dataSeries1, lineGraphVisual1, maxVisibleValueAmount, getAxisLabelX, getAxisLabelY);
            }, updateInterval);

            // Criar um segundo gráfico
            ShowGraph(dataSeries2, lineGraphVisual2, maxVisibleValueAmount, getAxisLabelX, getAxisLabelY);

            // Iniciar o segundo gráfico
            FunctionPeriodic.Create(() => {
                UpdateValues(dataSeries2, ref currentIndex, receivedDeformacao_Bra1);
                // Atualizar o segundo gráfico
                ShowGraph(dataSeries2, lineGraphVisual2, maxVisibleValueAmount, getAxisLabelX, getAxisLabelY);
            }, updateInterval);

        }

        public static void ShowTooltip_Static(string tooltipText, Vector2 anchoredPosition)
        {
            instance.ShowTooltip(tooltipText, anchoredPosition);
        }

        private void ShowTooltip(string tooltipText, Vector2 anchoredPosition)
        {
            // Show Tooltip GameObject
            tooltipGameObject.SetActive(true);

            tooltipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

            Text tooltipUIText = tooltipGameObject.transform.Find("text").GetComponent<Text>();
            tooltipUIText.text = tooltipText;

            float textPaddingSize = 4f;
            Vector2 backgroundSize = new Vector2(
                tooltipUIText.preferredWidth + textPaddingSize * 2f,
                tooltipUIText.preferredHeight + textPaddingSize * 2f
            );

            tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().sizeDelta = backgroundSize;

            tooltipGameObject.transform.SetAsLastSibling();
        }

        public static void HideTooltip_Static()
        {
            instance.HideTooltip();
        }

        private void HideTooltip()
        {
            tooltipGameObject.SetActive(false);
        }

        private void SetGetAxisLabelX(List<int> valueList, Func<int, string> getAxisLabelX)
        {
            ShowGraph(valueList, this.graphVisual, this.maxVisibleValueAmount, getAxisLabelX, this.getAxisLabelY);
        }

        private void SetGetAxisLabelY(List<int> valueList, Func<float, string> getAxisLabelY)
        {
            ShowGraph(valueList, this.graphVisual, this.maxVisibleValueAmount, this.getAxisLabelX, getAxisLabelY);
        }

        private void IncreaseVisibleAmount(List<int> valueList)
        {
            ShowGraph(valueList, this.graphVisual, this.maxVisibleValueAmount + 1, this.getAxisLabelX, this.getAxisLabelY);
        }

        private void DecreaseVisibleAmount(List<int> valueList)
        {
            ShowGraph(valueList, this.graphVisual, this.maxVisibleValueAmount - 1, this.getAxisLabelX, this.getAxisLabelY);
        }

        private void SetGraphVisual(List<int> valueList, IGraphVisual graphVisual)
        {
            ShowGraph(valueList, graphVisual, this.maxVisibleValueAmount, this.getAxisLabelX, this.getAxisLabelY);
        }

        private void ShowGraph(List<int> valueList, IGraphVisual graphVisual, int maxVisibleValueAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
        {
            this.graphVisual = graphVisual;
            this.getAxisLabelX = getAxisLabelX;
            this.getAxisLabelY = getAxisLabelY;

            if (maxVisibleValueAmount <= 0)
            {
                // Show all if no amount specified
                maxVisibleValueAmount = valueList.Count;
            }
            if (maxVisibleValueAmount > valueList.Count)
            {
                // Validate the amount to show the maximum
                maxVisibleValueAmount = valueList.Count;
            }

            this.maxVisibleValueAmount = maxVisibleValueAmount;

            // Test for label defaults
            if (getAxisLabelX == null)
            {
                getAxisLabelX = delegate (int _i) { return _i.ToString(); };
            }
            if (getAxisLabelY == null)
            {
                getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
            }

            // Clean up previous graph
            foreach (GameObject gameObject in gameObjectList)
            {
                Destroy(gameObject);
            }
            gameObjectList.Clear();
            yLabelList.Clear();

            foreach (IGraphVisualObject graphVisualObject in graphVisualObjectList)
            {
                graphVisualObject.CleanUp();
            }
            graphVisualObjectList.Clear();

            graphVisual.CleanUp();

            float graphWidth = graphContainer.sizeDelta.x;
            float graphHeight = graphContainer.sizeDelta.y;

            float yMinimum, yMaximum;
            CalculateYScale(dataSeries1, out yMinimum, out yMaximum);
            CalculateYScale(dataSeries2, out yMinimum, out yMaximum);

            xSize = graphWidth / (maxVisibleValueAmount + 1);


            int xIndex = 0;
            for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
            {
                float xPosition = xSize + xIndex * xSize;
                float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;


                string tooltipText = getAxisLabelY(valueList[i]);
                IGraphVisualObject graphVisualObject = graphVisual.CreateGraphVisualObject(new Vector2(xPosition, yPosition), xSize, tooltipText);
                graphVisualObjectList.Add(graphVisualObject);


                RectTransform labelX = Instantiate(labelTemplateX);
                labelX.SetParent(graphContainer, false);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPosition, -7f);
                labelX.GetComponent<Text>().text = getAxisLabelX(i);
                gameObjectList.Add(labelX.gameObject);


                RectTransform dashX = Instantiate(dashTemplateX);
                dashX.SetParent(dashContainer, false);
                dashX.gameObject.SetActive(true);
                dashX.anchoredPosition = new Vector2(xPosition, -3f);
                gameObjectList.Add(dashX.gameObject);

                xIndex++;
            }


            int separatorCount = 10;
            for (int i = 0; i <= separatorCount; i++)
            {

                RectTransform labelY = Instantiate(labelTemplateY);
                labelY.SetParent(graphContainer, false);
                labelY.gameObject.SetActive(true);
                float normalizedValue = i * 1f / separatorCount;
                labelY.anchoredPosition = new Vector2(-7f, normalizedValue * graphHeight);
                labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
                yLabelList.Add(labelY);
                gameObjectList.Add(labelY.gameObject);

                RectTransform dashY = Instantiate(dashTemplateY);
                dashY.SetParent(dashContainer, false);
                dashY.gameObject.SetActive(true);
                dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
                gameObjectList.Add(dashY.gameObject);
            }
        }

        private void UpdateValues(List<int> valueList, ref int currentIndex, float receivedValue)
        {
            // Atualiza apenas o último valor da lista
            valueList[currentIndex] = Mathf.RoundToInt(receivedValue);

            // Propaga os valores para os pontos anteriores
            for (int i = currentIndex + 1; i < valueList.Count; i++)
            {
                valueList[i] = valueList[i - 1];
            }

            // Decrementa o índice para atualizar o próximo ponto anterior
            currentIndex = (currentIndex - 1 + valueList.Count) % valueList.Count;
        }

        private void CalculateYScale(List<int> valueList, out float yMinimum, out float yMaximum)
        {
            if (valueList.Count == 0)
            {
                yMinimum = 0;
                yMaximum = 0;
                return;
            }

            yMaximum = valueList[0];
            yMinimum = valueList[0];

            for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
            {
                int value = valueList[i];
                if (value > yMaximum)
                {
                    yMaximum = value;
                }
                if (value < yMinimum)
                {
                    yMinimum = value;
                }
            }

            float yDifference = yMaximum - yMinimum;
            if (yDifference <= 0)
            {
                yDifference = 5f;
            }
            yMaximum = yMaximum + (yDifference * 0.2f);
            yMinimum = yMinimum - (yDifference * 0.2f);

            if (startYScaleAtZero)
            {
                yMinimum = 0f;
            }
        }


        private interface IGraphVisual
        {

            IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
            void CleanUp();

        }

        private interface IGraphVisualObject
        {

            void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
            void CleanUp();

        }

        private class BarChartVisual : IGraphVisual
        {

            private RectTransform graphContainer;
            private Color barColor;
            private float barWidthMultiplier;

            public BarChartVisual(RectTransform graphContainer, Color barColor, float barWidthMultiplier)
            {
                this.graphContainer = graphContainer;
                this.barColor = barColor;
                this.barWidthMultiplier = barWidthMultiplier;
            }

            public void CleanUp()
            {
            }

            public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
            {
                GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth);

                BarChartVisualObject barChartVisualObject = new BarChartVisualObject(barGameObject, barWidthMultiplier);
                barChartVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);

                return barChartVisualObject;
            }

            private GameObject CreateBar(Vector2 graphPosition, float barWidth)
            {
                GameObject gameObject = new GameObject("bar", typeof(Image));
                gameObject.transform.SetParent(graphContainer, false);
                gameObject.GetComponent<Image>().color = barColor;
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0f);
                rectTransform.sizeDelta = new Vector2(barWidth * barWidthMultiplier, graphPosition.y);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(.5f, 0f);

                Button_UI barButtonUI = gameObject.AddComponent<Button_UI>();

                return gameObject;
            }


            public class BarChartVisualObject : IGraphVisualObject
            {

                private GameObject barGameObject;
                private float barWidthMultiplier;

                public BarChartVisualObject(GameObject barGameObject, float barWidthMultiplier)
                {
                    this.barGameObject = barGameObject;
                    this.barWidthMultiplier = barWidthMultiplier;
                }

                public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
                {
                    RectTransform rectTransform = barGameObject.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0f);
                    rectTransform.sizeDelta = new Vector2(graphPositionWidth * barWidthMultiplier, graphPosition.y);

                    Button_UI barButtonUI = barGameObject.GetComponent<Button_UI>();

                    // Show Tooltip on Mouse Over
                    barButtonUI.MouseOverOnceFunc = () => {
                        ShowTooltip_Static(tooltipText, graphPosition);
                    };

                    // Hide Tooltip on Mouse Out
                    barButtonUI.MouseOutOnceFunc = () => {
                        HideTooltip_Static();
                    };
                }

                public void CleanUp()
                {
                    Destroy(barGameObject);
                }


            }

        }

        private class LineGraphVisual : IGraphVisual
        {

            private RectTransform graphContainer;
            private Sprite dotSprite;
            private LineGraphVisualObject lastLineGraphVisualObject;
            private Color dotColor;
            private Color dotConnectionColor;

            public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite, Color dotColor, Color dotConnectionColor)
            {
                this.graphContainer = graphContainer;
                this.dotSprite = dotSprite;
                this.dotColor = dotColor;
                this.dotConnectionColor = dotConnectionColor;
                lastLineGraphVisualObject = null;
            }

            public void CleanUp()
            {
                lastLineGraphVisualObject = null;
            }


            public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
            {
                GameObject dotGameObject = CreateDot(graphPosition);


                GameObject dotConnectionGameObject = null;
                if (lastLineGraphVisualObject != null)
                {
                    dotConnectionGameObject = CreateDotConnection(lastLineGraphVisualObject.GetGraphPosition(), dotGameObject.GetComponent<RectTransform>().anchoredPosition);
                }

                LineGraphVisualObject lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnectionGameObject, lastLineGraphVisualObject);
                lineGraphVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);

                lastLineGraphVisualObject = lineGraphVisualObject;

                return lineGraphVisualObject;
            }

            private GameObject CreateDot(Vector2 anchoredPosition)
            {
                GameObject gameObject = new GameObject("dot", typeof(Image));
                gameObject.transform.SetParent(graphContainer, false);
                gameObject.GetComponent<Image>().sprite = dotSprite;
                gameObject.GetComponent<Image>().color = dotColor;
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = new Vector2(11, 11);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);

                // Add Button_UI Component which captures UI Mouse Events
                Button_UI dotButtonUI = gameObject.AddComponent<Button_UI>();

                return gameObject;
            }

            private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
            {
                GameObject gameObject = new GameObject("dotConnection", typeof(Image));
                gameObject.transform.SetParent(graphContainer, false);
                gameObject.GetComponent<Image>().color = dotConnectionColor;
                gameObject.GetComponent<Image>().raycastTarget = false;
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                Vector2 dir = (dotPositionB - dotPositionA).normalized;
                float distance = Vector2.Distance(dotPositionA, dotPositionB);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.sizeDelta = new Vector2(distance, 3f);
                rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
                rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
                return gameObject;
            }


            public class LineGraphVisualObject : IGraphVisualObject
            {

                public event EventHandler OnChangedGraphVisualObjectInfo;

                private GameObject dotGameObject;
                private GameObject dotConnectionGameObject;
                private LineGraphVisualObject lastVisualObject;

                public LineGraphVisualObject(GameObject dotGameObject, GameObject dotConnectionGameObject, LineGraphVisualObject lastVisualObject)
                {
                    this.dotGameObject = dotGameObject;
                    this.dotConnectionGameObject = dotConnectionGameObject;
                    this.lastVisualObject = lastVisualObject;

                    if (lastVisualObject != null)
                    {
                        lastVisualObject.OnChangedGraphVisualObjectInfo += LastVisualObject_OnChangedGraphVisualObjectInfo;
                    }
                }

                private void LastVisualObject_OnChangedGraphVisualObjectInfo(object sender, EventArgs e)
                {
                    UpdateDotConnection();
                }

                public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
                {
                    RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = graphPosition;

                    UpdateDotConnection();

                    Button_UI dotButtonUI = dotGameObject.GetComponent<Button_UI>();

                    // Show Tooltip on Mouse Over
                    dotButtonUI.MouseOverOnceFunc = () => {
                        ShowTooltip_Static(tooltipText, graphPosition);
                    };

                    // Hide Tooltip on Mouse Out
                    dotButtonUI.MouseOutOnceFunc = () => {
                        HideTooltip_Static();
                    };

                    if (OnChangedGraphVisualObjectInfo != null) OnChangedGraphVisualObjectInfo(this, EventArgs.Empty);
                }

                public void CleanUp()
                {
                    Destroy(dotGameObject);
                    Destroy(dotConnectionGameObject);
                }

                public Vector2 GetGraphPosition()
                {
                    RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                    return rectTransform.anchoredPosition;
                }

                private void UpdateDotConnection()
                {
                    if (dotConnectionGameObject != null)
                    {
                        RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
                        Vector2 dir = (lastVisualObject.GetGraphPosition() - GetGraphPosition()).normalized;
                        float distance = Vector2.Distance(GetGraphPosition(), lastVisualObject.GetGraphPosition());
                        dotConnectionRectTransform.sizeDelta = new Vector2(distance, 3f);
                        dotConnectionRectTransform.anchoredPosition = GetGraphPosition() + dir * distance * .5f;
                        dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
                    }
                }

            }

        }

    }


}