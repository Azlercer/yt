﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// A 3D surface of a <see cref="Graph"/>
/// </summary>
public class BarDataManager: MonoBehaviour
{
    #region variables
    public Graph plot;
    [SerializeField] PrimerObject barPrefab = null;

    public List<float> values = new List<float>();
    private List<PrimerObject> bars = new List<PrimerObject>();
    public int BarCount {
        get {
            return bars.Count;
        }
        set {
            Debug.LogError("Cannot set bar count directly");
        }
    }
    float barZOffset = 0.0001f;
    // Bar appearance
    // List<Color> barColors = new List<Color>();
    public Color defaultColor = Color.blue;
    public float barWidth = 1;
    public bool showBarNumbers = false;
    public int barNumberPrecision = 3; // The number of places to hold after the decimal point
    public float barNumberScaleFactor = 0.3f;
    private List<PrimerText> barNumbers = new List<PrimerText>();

    #endregion

    void Awake() {
        // plot = transform.parent.gameObject.GetComponent<Graph>();
        bars = new List<PrimerObject>();
    }
    void GenerateBars(int totalBars = 0) {
        if (totalBars == 0) { totalBars = values.Count; }
        // Just makes sure there are enough
        for (int i = 0; i < totalBars; i++)
        {
            if (i == bars.Count) {
                PrimerObject newBar = Instantiate(barPrefab);
                newBar.transform.parent = transform;
                newBar.transform.localPosition = new Vector3(i + 1, 0, barZOffset);
                newBar.transform.localRotation = Quaternion.Euler(0, 0, 0);
                newBar.transform.localScale = new Vector3(barWidth, 0, 1);
                newBar.EmissionColor = defaultColor;
                bars.Add(newBar);
                // if (barColors.Count > 0 && barColors.Count < bars.Count) {
                //     barColors.Add(barColors[barColors.Count - 1]);
                // }

                // Also generate the number labels
                if (showBarNumbers) {
                    PrimerText newBarNumber = Instantiate(SceneManager.instance.primerTextPrefab);
                    newBarNumber.tmpro.alignment = TextAlignmentOptions.Bottom;
                    newBarNumber.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
                    newBarNumber.transform.parent = transform.parent;
                    newBarNumber.transform.localRotation = Quaternion.Euler(0, 0, 0);

                    ParentConstraint pc = newBarNumber.gameObject.AddComponent<ParentConstraint>();
                    ConstraintSource constraintSource = new ConstraintSource();
                    constraintSource.sourceTransform = bars[i].transform;
                    constraintSource.weight = 1;
                    pc.AddSource(constraintSource);
                    pc.SetTranslationOffset(0, Vector3.zero); 
                    pc.translationAxis = Axis.X | Axis.Z;
                    pc.locked = true;
                    pc.constraintActive = true;

                    newBarNumber.transform.localScale = new Vector3(0, 0, 1);
                    barNumbers.Add(newBarNumber);
                }
            }
            else if (i > bars.Count) {
                Debug.LogError("Bar generation is effed.");
            }
        }
    }

    public void AnimateBars(List<float> newVals, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(animateBars(newVals, duration, ease));
    }
    public void AnimateBars(List<int> newVals, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        AnimateBars(IntListToFloat(newVals), duration, ease);
    }
    IEnumerator animateBars(List<float> newVals, float duration, EaseMode ease) {
        GenerateBars(newVals.Count);
        // Make sure to have unused bars disappear
        while (newVals.Count < bars.Count) {
            newVals.Add(0);
        }
        List<float> oldVals = values;
        if (showBarNumbers) {
            for (int i = 0; i < newVals.Count; i++)
            {
                if (i == oldVals.Count) {
                    oldVals.Add(0);
                }
                if (newVals[i] != oldVals[i]) {
                    PrimerText newBarNum = barNumbers[i];
                    newBarNum.MoveTo(new Vector3(0, newVals[i] * transform.localScale.y, 0), duration: duration, ease: ease);
                    newBarNum.ScaleDownToZero(duration: duration, ease: ease);
                }
            }
        }
        float startTime = Time.time;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            for (int i = 0; i < bars.Count; i++)
            {
                if (i == oldVals.Count) {
                    oldVals.Add(0);
                }
                values[i] = Mathf.Lerp(oldVals[i], newVals[i], t);    
            }
            UpdateBars(values);
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < newVals.Count; i++)
        {
            values[i] = newVals[i];
        }
        UpdateBars(values);
        if (showBarNumbers) {
            for (int i = 0; i < newVals.Count; i++)
            {
                PrimerText newBarNum = barNumbers[i];
                if (newVals[i] > 0 && newBarNum.transform.localScale.y == 0) {
                    float displayVal = (float) Math.Round(newVals[i], barNumberPrecision);
                    string displayString = displayVal.ToString();
                    if (newVals[i] != displayVal) {
                        displayString = "~" + displayString;
                    }
                    newBarNum.tmpro.text = displayString;
                    newBarNum.SetIntrinsicScale(Vector3.one * transform.localScale.x * barNumberScaleFactor);
                    newBarNum.ScaleUpFromZero(duration: duration, ease: ease);
                }
            }
        }
    }

    public void UpdateBars(List<float> newVals) {
        values = newVals;
        for (int i = 0; i < values.Count; i++)
        {
            bars[i].transform.localScale = new Vector3(barWidth, values[i], 1);
        }
    }
    public void UpdateBars(List<int> newVals) {
        UpdateBars(IntListToFloat(newVals));
    }
    private List<float> IntListToFloat(List<int> ints) {
        List<float> fvals = new List<float>();
        foreach (int val in ints) {
            fvals.Add(val);
        }
        return fvals;
    }
    public void SetBarColors(List<Color> colors) {
        for (int i = 0; i < bars.Count; i++) {
            Color c = colors[i % colors.Count];
            bars[i].EmissionColor = c;
        }
    }
    public void SetBarColors(Color color) {
        SetBarColors(new List<Color>() {color});
    }
    public void SetBarColor(int barIndex, Color color) {
        bars[barIndex].EmissionColor = color;
    }
    public void AnimateColors(List<Color> colors, float duration = 0.5f, EaseMode ease = EaseMode.None) {
            for (int i = 0; i < bars.Count; i++) {
                // barColors[i] = colors[i];
                bars[i].AnimateEmissionColor(colors[i], duration: duration, ease: ease);
            }
    }
    public void AnimateColor(int barIndex, Color color, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        // barColors[barIndex] = color;
        bars[barIndex].AnimateEmissionColor(color, duration: duration, ease: ease);
    }

    public void RefreshData() {
        transform.localRotation = Quaternion.identity; //Really shouldn't be rotated relative to parent
        transform.localPosition = Vector3.zero; //Shouldn't be displaced relative to parent
        AdjustBarSpaceScale();
        GenerateBars();
        UpdateBars(values);
    }
    public void AdjustBarSpaceScale() {
        float xScale = plot.xLengthMinusPadding / (plot.xMax - plot.xMin);        
        float yScale = plot.yLengthMinusPadding / (plot.yMax - plot.yMin);      
        transform.localScale = new Vector3 (xScale, yScale, 1);
    }
    private void OnApplicationQuit() {
        ForceStopAnimation();
    }

    private void ForceStopAnimation() {
        StopAllCoroutines();
    }
    public static Dictionary<float, string> GenerateIntegerCategories(int num, int min = 0, int step = 1) {
        Dictionary<float, string> categoryDict = new Dictionary<float, string>();
        for (int i = 0; i < num; i++) {
            categoryDict.Add(1 + i, (min + i * step).ToString());
        };
        return categoryDict;
    }
}
