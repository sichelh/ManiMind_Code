using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneLoader : SceneOnlySingleton<BattleSceneLoader>
{
    HashSet<string> bundles = new HashSet<string>();
    HashSet<string> assets = new HashSet<string>();
   public void LoadAssets()
   {
       foreach (Unit unit in BattleManager.Instance.AllUnits)
       {
           assets.Add(unit.UnitSo.AttackType.AttackSound.ToString());
           assets.Add(unit.UnitSo.AttackType.HitSound.ToString().ToString());
           assets.Add(unit.UnitSo.AttackVoiceSound.ToString().ToString());
           assets.Add(unit.UnitSo.HitVoiceSound.ToString().ToString());
           
           // LoadAssetManager.Instance.LoadAudioClipAsync(unit.UnitSo.AttackType.AttackSound.ToString(), null);
           // LoadAssetManager.Instance.LoadAudioClipAsync(unit.UnitSo.AttackType.HitSound.ToString(), null);
           // LoadAssetManager.Instance.LoadAudioClipAsync(unit.UnitSo.AttackVoiceSound.ToString(), null);
           // LoadAssetManager.Instance.LoadAudioClipAsync(unit.UnitSo.HitVoiceSound.ToString(), null);
           if (unit.UnitSo is EnemyUnitSO)
           {
               EnemyUnitSO monsterSO = unit.UnitSo as EnemyUnitSO;
               if (monsterSO.monsterType != MonsterType.None)
               {
                   // LoadAssetManager.Instance.LoadAssetBundle(monsterSO.monsterType.ToString()+"Sound");
                   bundles.Add(monsterSO.monsterType.ToString()+"Sound");
               }
           }
           foreach (SkillData skill in unit.SkillController.skills)
           {
               if(skill==null || skill.skillSo.SFX == SFXName.None) continue;
               // LoadAssetManager.Instance.LoadAudioClipAsync(skill.skillSo.SFX.ToString(), null);
               assets.Add(skill.skillSo.SFX.ToString());
           }
       }

       foreach (var bundle in bundles)
       {
           LoadAssetManager.Instance.LoadAssetBundle(bundle);
       }

       foreach (var asset in assets)
       {
           LoadAssetManager.Instance.LoadAudioClipAsync(asset,null);
       }

   }
   
}
