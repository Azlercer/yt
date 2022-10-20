﻿using UnityEngine;

/* TODO
 * (Core)
 * Add precision options for tic labels
 *
 * (When needed)
 * Make padding amount uniform instead of using fraction of each axis length (maybe)
 * Destroy unused tics, animating them to zero scale
 */



[ExecuteInEditMode]
public class Graph2 : PrimerObject
{
    [Header("Other")]
    public float ticLabelDistanceVertical = 0.25f;
    public float ticLabelDistanceHorizontal = 0.65f;
    [Range(0, 0.5f)]
    public float paddingFraction = 0.05f;
    public bool rightHanded = true;

    public GameObject arrowPrefab;
    public PrimerText primerTextPrefab;
    public Tic2 ticPrefab;

    public void Regenerate() {
        foreach (var axis in GetComponents<Axis2>()) {
            axis.Regenerate();
        }
    }

    public void RemoveEditorGeneratedChildren() {
        gameObject.RemoveEditorGeneratedChildren();

        foreach (var axis in GetComponents<Axis2>()) {
            axis.container.gameObject.RemoveEditorGeneratedChildren();
        }
    }
}
