using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class EquipmentCombineInventoryUI : BaseInventoryUI
{
    [SerializeField] private UIEquipmentCombine uiEquipmentCombine;

    private CombineManager combineManager;
}