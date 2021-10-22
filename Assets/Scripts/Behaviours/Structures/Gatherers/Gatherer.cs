using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatherer : Structure
{
    [Header("Resource Gathering Settings")]
    [SerializeField] [Tooltip("Amount of resurce will be gathered each time")]
    protected int resourceGatheredEachCicle;
    [SerializeField] protected float timeToGatherResource; // Time when the next resources will be gathered
    [SerializeField] protected float timerToGetResource;
}