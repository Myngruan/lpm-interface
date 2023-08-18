﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using System.IO.Ports;
using static grafico.Window_Graph_Pressao;

namespace grafico
{
    public class Window_Graph_Pressao : MonoBehaviour, IDataReceiver
    {

        private static Window_Graph_Pressao instance;


        private float yMinimum;
        private float yMaximum;
        private float graphHeight;

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

        private List<int> valueList;
        private IGraphVisual graphVisual;
        private int maxVisibleValueAmount;
        private Func<int, string> getAxisLabelX;
        private Func<float, string> getAxisLabelY;
        private float xSize;
        private bool startYScaleAtZero;

        private mainSerial serialController;
        private List<float> receivedDataList = new List<float>();

        public interface IDataReceiver
        {
            void ReceivePressao(float value);
        }
        public void ReceiveAltura(float altura)
        {

        }
        public void ReceiveData(float value)
        {

            UpdateGraphWithValue(value);
        }
        public void ReceivePressao(float pressao)
        {

        }

        public void SetSerialController(mainSerial controller)
        {
            serialController = controller;
        }

        private float currentValue = 0;
        private float targetValue = 0;

        private IEnumerator StartGraphAnimation(List<int> valueList, IGraphVisual graphVisual, int maxVisiblePoints, Func<int, string> getAxisLabelX, Func<float, string> getAxisLabelY)
        {
            int index = 0;

            while (index < valueList.Count)
            {

                targetValue = valueList[index];

                while (currentValue < targetValue)
                {
                    currentValue += Time.deltaTime * 10f;
                    currentValue = Mathf.Min(currentValue, targetValue);

                    UpdateValue(index, Mathf.RoundToInt(currentValue));
                    yield return null;
                }

                index++;
                yield return new WaitForSeconds(0.1f);
            }
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

            IGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, Color.green, new Color(1, 1, 1, .5f));
            IGraphVisual barChartVisual = new BarChartVisual(graphContainer, Color.white, .8f);


            HideTooltip();

            List<int> valueList = new List<int>() { 0, 0, 0, 0 , 0};

            ShowGraph(valueList, lineGraphVisual, -1, (int _i) => (_i + 1) + "s", (float _f) => Mathf.RoundToInt(_f) + "Pa");

            int index = 0;
            FunctionPeriodic.Create(() => {
                index = (index + 1) % valueList.Count;
            }, .1f);
            FunctionPeriodic.Create(() => {
                //int index = UnityEngine.Random.Range(0, valueList.Count);
                UpdateValue(index, valueList[index] + UnityEngine.Random.Range(1, 3));
            }, .02f);




        }

        public static void ShowTooltip_Static(string tooltipText, Vector2 anchoredPosition)
        {
            instance.ShowTooltip(tooltipText, anchoredPosition);
        }

        private void ShowTooltip(string tooltipText, Vector2 anchoredPosition)
        {

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

        private void SetGetAxisLabelX(Func<int, string> getAxisLabelX)
        {
            ShowGraph(this.valueList, this.graphVisual, this.maxVisibleValueAmount, getAxisLabelX, this.getAxisLabelY);
        }

        private void SetGetAxisLabelY(Func<float, string> getAxisLabelY)
        {
            ShowGraph(this.valueList, this.graphVisual, this.maxVisibleValueAmount, this.getAxisLabelX, getAxisLabelY);
        }

        private void IncreaseVisibleAmount()
        {
            ShowGraph(this.valueList, this.graphVisual, this.maxVisibleValueAmount + 1, this.getAxisLabelX, this.getAxisLabelY);
        }

        private void DecreaseVisibleAmount()
        {
            ShowGraph(this.valueList, this.graphVisual, this.maxVisibleValueAmount - 1, this.getAxisLabelX, this.getAxisLabelY);
        }

        private void SetGraphVisual(IGraphVisual graphVisual)
        {
            ShowGraph(this.valueList, graphVisual, this.maxVisibleValueAmount, this.getAxisLabelX, this.getAxisLabelY);
        }

        private void ShowGraph(List<int> valueList, IGraphVisual graphVisual, int maxVisibleValueAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
        {
            this.valueList = valueList;
            this.graphVisual = graphVisual;
            this.getAxisLabelX = getAxisLabelX;
            this.getAxisLabelY = getAxisLabelY;

            if (maxVisibleValueAmount <= 0)
            {

                maxVisibleValueAmount = valueList.Count;
            }
            if (maxVisibleValueAmount > valueList.Count)
            {

                maxVisibleValueAmount = valueList.Count;
            }

            this.maxVisibleValueAmount = maxVisibleValueAmount;

            if (getAxisLabelX == null)
            {
                getAxisLabelX = delegate (int _i) { return _i.ToString(); };
            }
            if (getAxisLabelY == null)
            {
                getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
            }


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
            CalculateYScale(out yMinimum, out yMaximum);


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

        private void UpdateValue(int index, int value)
        {
            float yMinimumBefore, yMaximumBefore;
            CalculateYScale(out yMinimumBefore, out yMaximumBefore);

            valueList[index] = value;

            float graphWidth = graphContainer.sizeDelta.x;
            float graphHeight = graphContainer.sizeDelta.y;

            float yMinimum, yMaximum;
            CalculateYScale(out yMinimum, out yMaximum);

            bool yScaleChanged = yMinimumBefore != yMinimum || yMaximumBefore != yMaximum;

            if (!yScaleChanged)
            {

                float xPosition = xSize + index * xSize;
                float yPosition = ((value - yMinimum) / (yMaximum - yMinimum)) * graphHeight;


                string tooltipText = getAxisLabelY(value);
                graphVisualObjectList[index].SetGraphVisualObjectInfo(new Vector2(xPosition, yPosition), xSize, tooltipText);
            }
            else
            {

                int xIndex = 0;
                for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
                {
                    float xPosition = xSize + xIndex * xSize;
                    float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;


                    string tooltipText = getAxisLabelY(valueList[i]);
                    graphVisualObjectList[xIndex].SetGraphVisualObjectInfo(new Vector2(xPosition, yPosition), xSize, tooltipText);

                    xIndex++;
                }

                for (int i = 0; i < yLabelList.Count; i++)
                {
                    float normalizedValue = i * 1f / yLabelList.Count;
                    yLabelList[i].GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
                }
            }
        }

        private void CalculateYScale(out float yMinimum, out float yMaximum)
        {

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



            this.yMinimum = yMinimum;
            this.yMaximum = yMaximum;
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

                // Add Button_UI Component which captures UI Mouse Events
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


                    barButtonUI.MouseOverOnceFunc = () => {
                        ShowTooltip_Static(tooltipText, graphPosition);
                    };


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


                    dotButtonUI.MouseOverOnceFunc = () => {
                        ShowTooltip_Static(tooltipText, graphPosition);
                    };


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





        private void Update()
        {
            try
            {
                if (serialController != null)
                {
                    float lastPressao = serialController.GetLastPressao();
                    UpdateGraphWithValue(lastPressao);
                }
            }
            catch (System.Exception)
            {

            }
        }


        private void UpdateGraphWithValue(float value)
        {
            float xPosition = 0;
            float yPosition = ((value - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

            IGraphVisualObject graphVisualObject = graphVisual.CreateGraphVisualObject(new Vector2(xPosition, yPosition), xSize, value.ToString());
            graphVisualObjectList.Add(graphVisualObject);
        }

    }

}