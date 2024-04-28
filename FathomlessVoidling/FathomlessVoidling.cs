using BepInEx;
using RoR2;
using RoR2.VoidRaidCrab;
using R2API;
using EntityStates.VoidRaidCrab;
using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API.Utils;
using RoR2.Projectile;
using RoR2.CharacterAI;
using RoR2.Skills;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using System.Diagnostics;
using RoR2.Audio;

namespace FathomlessVoidling
{
  [BepInPlugin("com.Nuxlar.FathomlessVoidling", "FathomlessVoidling", "1.0.0")]

  public class FathomlessVoidling : BaseUnityPlugin
  {
    public static GameObject spawnEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpawnEffect.prefab").WaitForCompletion();
    public static CharacterSpawnCard jointCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/VoidRaidCrab/cscVoidRaidCrabJoint.asset").WaitForCompletion();
    public static GameObject voidRainPortalEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabSpawnEffect.prefab").WaitForCompletion(), "VoidRainPortalEffect");
    public static GameObject laserPortalEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabSpawnEffect.prefab").WaitForCompletion(), "LaserPortalEffect");
    GameEndingDef voidEnding = Addressables.LoadAssetAsync<GameEndingDef>("RoR2/Base/WeeklyRun/PrismaticTrialEnding.asset").WaitForCompletion();
    GameObject voidling = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabBody.prefab").WaitForCompletion();
    GameObject miniVoidling = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
    SpawnCard voidlingCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/VoidRaidCrab/cscVoidRaidCrab.asset").WaitForCompletion();
    GameObject missileProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab").WaitForCompletion();
    GameObject missileProjectileGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileGhost.prefab").WaitForCompletion();
    GameObject missileImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabImpact1.prefab").WaitForCompletion();
    GameObject spinBeamVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamVFX.prefab").WaitForCompletion();
    private GameObject joint = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabJointBody.prefab").WaitForCompletion();
    private static Material voidCylinderMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/matITSafeWardAreaIndicator1.mat").WaitForCompletion();
    public static GameObject chargeVoidRain = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamChargeUp.prefab").WaitForCompletion();
    public static GameObject voidRainWarning = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MultiBeamRayIndicator.prefab").WaitForCompletion();
    public static GameObject voidRainTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/TracerVoidRaidCrabTripleBeamSmall.prefab").WaitForCompletion();
    public static GameObject voidRainExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamExplosion.prefab").WaitForCompletion();
    private static GameObject voidlingMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMaster.prefab").WaitForCompletion();
    private static SkillDef secondaryDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabMultiBeam.asset").WaitForCompletion();
    private static SkillDef utilityDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabSpinBeam.asset").WaitForCompletion();
    private static SkillDef gauntletDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabChannelGauntlet.asset").WaitForCompletion();
    public static GameObject suckSphereEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/KillSphereVfxPlaceholder.prefab").WaitForCompletion(), "WSingularitySphere");
    public static GameObject suckCenterEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSuckLoopFX.prefab").WaitForCompletion(), "WSingularityCenter");
    public static GameObject wSingularityProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab").WaitForCompletion(), "WSingularityProjectile");
    public static GameObject wSingularityGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBombGhost.prefab").WaitForCompletion(), "WSingularityGhost");
    public static LoopSoundDef singularityLSD = Addressables.LoadAssetAsync<LoopSoundDef>("RoR2/DLC1/VoidRaidCrab/lsdVoidRaidCrabVacuumAttack.asset").WaitForCompletion();

    public void Awake()
    {
      Destroy(suckSphereEffect.GetComponent<VFXHelper.VFXTransformController>());
      Destroy(suckCenterEffect.GetComponent<VFXHelper.VFXTransformController>());
      suckSphereEffect.transform.localScale = new Vector3(20f, 20f, 20f);

      foreach (Transform child in wSingularityGhost.transform)
      {
        Destroy(child.gameObject);
      }

      suckCenterEffect.transform.parent = wSingularityGhost.transform;
      suckSphereEffect.transform.parent = wSingularityGhost.transform;
      suckCenterEffect.GetComponent<VFXAttributes>().vfxIntensity = VFXAttributes.VFXIntensity.Low;
      suckSphereEffect.GetComponent<VFXAttributes>().vfxIntensity = VFXAttributes.VFXIntensity.Low;

      foreach (Transform child in wSingularityProjectile.transform)
      {
        Destroy(child.gameObject);
      }
      Destroy(wSingularityProjectile.GetComponent<BoxCollider>());
      Destroy(wSingularityProjectile.GetComponent<ProjectileSingleTargetImpact>());
      wSingularityProjectile.AddComponent<SingularityKillComponent>();
      SphereCollider sphereCollider = wSingularityProjectile.AddComponent<SphereCollider>();
      sphereCollider.radius = 19f;
      sphereCollider.isTrigger = true;
      wSingularityProjectile.GetComponent<Rigidbody>().useGravity = false;
      // wSingularityProjectile.GetComponent<ProjectileSingleTargetImpact>().destroyOnWorld = false;
      ProjectileDirectionalTargetFinder targetFinder = wSingularityProjectile.GetComponent<ProjectileDirectionalTargetFinder>();
      targetFinder.allowTargetLoss = false;
      targetFinder.lookCone = 360f;
      targetFinder.lookRange = 1000f;
      ProjectileController projectileController = wSingularityProjectile.GetComponent<ProjectileController>();
      projectileController.ghostPrefab = wSingularityGhost;
      projectileController.cannotBeDeleted = true;
      projectileController.flightSoundLoop = singularityLSD;
      projectileController.myColliders = new Collider[1] { sphereCollider };
      ProjectileSimple projectileSimple = wSingularityProjectile.GetComponent<ProjectileSimple>();
      projectileSimple.desiredForwardSpeed = 10f;
      projectileSimple.lifetime = 30f;
      wSingularityProjectile.transform.localScale = Vector3.one;

      ContentAddition.AddProjectile(wSingularityProjectile);

      laserPortalEffect.GetComponent<DestroyOnTimer>().duration = 6f;
      foreach (ParticleSystem ps in laserPortalEffect.GetComponentsInChildren<ParticleSystem>())
      {
        var mainModule = ps.main;
        mainModule.startLifetimeMultiplier *= 3f;
      }
      foreach (AISkillDriver skillDriver in voidlingMaster.GetComponent<CharacterMaster>().GetComponents<AISkillDriver>())
      {
        switch (skillDriver.customName)
        {
          case "Channel Gauntlet 1":
            skillDriver.maxUserHealthFraction = 0.66f;
            break;
          case "Channel Gauntlet 2":
            skillDriver.maxUserHealthFraction = 0.33f;
            break;
          case "FireMissiles":
            skillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            break;
          case "FireMultiBeam":
            skillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            break;
          case "SpinBeam":
            skillDriver.maxUserHealthFraction = 1f;
            skillDriver.requiredSkill = null;
            break;
          case "GravityBump":
            skillDriver.maxUserHealthFraction = 1f;
            skillDriver.requiredSkill = null;
            skillDriver.skillSlot = SkillSlot.Utility;
            break;
          case "Vacuum Attack":
            skillDriver.maxUserHealthFraction = 0.9f;
            skillDriver.requiredSkill = null;
            break;
          case "LookAtTarget":
            skillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            break;
        }
      }

      Transform modelTransform = voidling.GetComponent<ModelLocator>().modelTransform;
      Animator animator = modelTransform.gameObject.GetComponent<Animator>();
      animator.applyRootMotion = true;
      animator.avatar = AvatarBuilder.BuildGenericAvatar(animator.gameObject, "ROOT");

      voidRainPortalEffect.transform.localScale /= 2;

      ContentAddition.AddEffect(voidRainPortalEffect);
      ContentAddition.AddEffect(laserPortalEffect);

      ContentAddition.AddEntityState<ChargeVoidRain>(out _);
      ContentAddition.AddEntityState<FireVoidRain>(out _);
      ContentAddition.AddEntityState<PortalBlast>(out _);
      ContentAddition.AddEntityState<WanderingSingularity>(out _);
      ContentAddition.AddEntityState<BetterSpawnState>(out _);
      ContentAddition.AddEntityState<BetterCollapse>(out _);
      ContentAddition.AddEntityState<BetterReEmerge>(out _);

      spinBeamVFX.transform.localScale *= 2;
      voidling.GetComponent<CharacterBody>().baseMaxHealth = 2700f;
      voidling.GetComponent<CharacterBody>().levelMaxHealth = 800;
      voidling.GetComponent<CharacterBody>().baseArmor = 40;

      joint.GetComponent<CharacterBody>().baseMaxHealth = 650f;
      joint.GetComponent<CharacterBody>().levelMaxHealth = 500f;

      spawnEffect.transform.localScale *= 2;

      missileImpact.transform.localScale *= 4;
      missileProjectile.GetComponent<ProjectileSimple>().oscillate = true;
      missileProjectile.GetComponent<ProjectileSimple>().oscillateSpeed = 5f;
      missileProjectile.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 30f;
      missileProjectile.AddComponent<ProjectileImpactExplosion>().blastRadius = 3f;
      missileProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = missileImpact;
      missileProjectile.GetComponent<ProjectileImpactExplosion>().destroyOnWorld = true;
      missileProjectile.GetComponent<ProjectileImpactExplosion>().lifetime = 15f;
      missileProjectile.GetComponent<ProjectileImpactExplosion>().blastDamageCoefficient = 1f;
      missileProjectile.GetComponent<ProjectileImpactExplosion>().falloffModel = BlastAttack.FalloffModel.SweetSpot;
      GameObject.Destroy(missileProjectile.GetComponent<ProjectileSingleTargetImpact>());
      missileProjectile.GetComponent<ProjectileDirectionalTargetFinder>().allowTargetLoss = false;
      missileProjectile.GetComponent<ProjectileDirectionalTargetFinder>().lookRange = 100f;
      missileProjectile.GetComponent<ProjectileDirectionalTargetFinder>().testLoS = true;
      missileProjectile.AddComponent<ProjectileTargetComponent>();
      missileProjectile.transform.localScale *= 4;
      foreach (Transform child in missileProjectileGhost.transform)
      {
        child.localScale *= 4;
      }

      secondaryDef.baseRechargeInterval = 20f;
      secondaryDef.activationState = new SerializableEntityStateType(typeof(ChargeVoidRain));

      voidling.GetComponent<SkillLocator>().primary.skillFamily.variants[0].skillDef.activationState = new SerializableEntityStateType(typeof(WanderingSingularity));
      voidling.GetComponent<SkillLocator>().primary.skillFamily.variants[0].skillDef.baseMaxStock = 1;
      voidling.GetComponent<SkillLocator>().secondary.skillFamily.variants[0].skillDef = secondaryDef;
      voidling.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef = utilityDef;

      GameObject model = voidling.GetComponent<ModelLocator>().modelTransform.gameObject;
      model.AddComponent<PrintController>();

      On.RoR2.Stage.Start += Stage_Start;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
      On.EntityStates.VoidRaidCrab.SpawnState.OnEnter += VoidRaidCrab_SpawnState;
      On.EntityStates.VoidRaidCrab.Collapse.OnEnter += Collapse_OnEnter;
      On.EntityStates.VoidRaidCrab.ReEmerge.OnEnter += ReEmerge_OnEnter;
      On.EntityStates.VoidRaidCrab.VacuumAttack.OnEnter += VacuumAttack_OnEnter;
      On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.OnEnter += BaseSpinBeamAttackState_OnEnter;
      On.EntityStates.EntityState.OnExit += SwapSecondaryOnExit;
      On.EntityStates.VoidRaidCrab.ChargeGauntlet.OnEnter += ChargeGauntlet_OnEnter;
      On.EntityStates.VoidRaidCrab.ChargeWardWipe.OnEnter += ChargeWardWipe_OnEnter;
      On.EntityStates.VoidRaidCrab.ChargeFinalStand.OnEnter += ChargeFinalStand_OnEnter;
      On.EntityStates.VoidRaidCrab.DeathState.OnEnter += DeathState_OnEnter;
      On.EntityStates.VoidRaidCrab.DeathState.OnExit += DeathState_OnExit;
      On.RoR2.VoidRaidCrab.LegController.CompleteBreakAuthority += ChangeSecondaryOnBreak;
    }

    private void SwapSecondaryOnExit(On.EntityStates.EntityState.orig_OnExit orig, EntityStates.EntityState self)
    {
      if (self is FireWardWipe)
      {
        self.characterBody.skillLocator.secondary.SetSkillOverride(self.gameObject, secondaryDef, GenericSkill.SkillOverridePriority.Replacement);
        self.characterBody.skillLocator.secondary.RemoveAllStocks();
      }
      orig(self);
    }

    private void ChangeSecondaryOnBreak(On.RoR2.VoidRaidCrab.LegController.orig_CompleteBreakAuthority orig, RoR2.VoidRaidCrab.LegController self)
    {
      if (self.mainBody)
      {
        float damage = self.jointBody.healthComponent.fullCombinedHealth;
        float healthAfterDamage = self.mainBody.healthComponent.health - damage;
        PhasedInventorySetter inventorySetter = self.mainBody.GetComponent<PhasedInventorySetter>();
        if (healthAfterDamage <= self.mainBody.healthComponent.fullCombinedHealth * 0.66f && inventorySetter && inventorySetter.phaseIndex == 0)
        {
          if (self.mainBody.skillLocator.secondary.skillDef == secondaryDef)
          {
            UnityEngine.Debug.LogWarning("Swapping secondary skill");
            self.mainBody.skillLocator.secondary.SetSkillOverride(self.gameObject, gauntletDef, GenericSkill.SkillOverridePriority.Replacement);
            self.mainBody.skillLocator.secondary.stock = self.mainBody.skillLocator.secondary.maxStock;
          }
        }
      }
      orig(self);
    }

    public static void CreateTube()
    {
      GameObject gameObject = new("WallHolder");
      gameObject.transform.position = new Vector3(-2.5f, 0.0f, 0.0f);
      GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      primitive.GetComponent<MeshRenderer>().material = voidCylinderMat;
      UnityEngine.Object.Destroy(primitive.GetComponent<CapsuleCollider>());
      MeshCollider collider = primitive.AddComponent<MeshCollider>();
      // primitive.AddComponent<ReverseNormals>();
      primitive.transform.localScale = new Vector3(110f, 1250f, 110f);
      primitive.name = "Cheese Deterrent";
      primitive.transform.SetParent(gameObject.transform);
      primitive.transform.localPosition = Vector3.zero;
      primitive.layer = LayerIndex.world.intVal;

      GameObject disableCollisions = new("DisableCollisions");
      disableCollisions.transform.parent = primitive.transform;
      disableCollisions.transform.localScale = new Vector3(0.0091f, 0.0008f, 0.0091f);
      disableCollisions.layer = LayerIndex.entityPrecise.intVal;
      DisableCollisionsIfInTrigger disableTrigger = disableCollisions.AddComponent<DisableCollisionsIfInTrigger>();
      disableTrigger.colliderToIgnore = collider;
      SphereCollider sphereCollider = disableCollisions.GetComponent<SphereCollider>();
      sphereCollider.radius = 85f;
      sphereCollider.isTrigger = true;
    }

    private void VacuumAttack_OnEnter(On.EntityStates.VoidRaidCrab.VacuumAttack.orig_OnEnter orig, VacuumAttack self)
    {
      VacuumAttack.killRadiusCurve = AnimationCurve.Linear(0, 0, 1, 100);
      VacuumAttack.pullMagnitudeCurve = AnimationCurve.Linear(0, 0, 1, 45);
      orig(self);
    }


    private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
    {
      orig(self, body);
      if (body.isPlayerControlled && SceneManager.GetActiveScene().name == "voidraid" && body.HasBuff(RoR2Content.Buffs.Immune))
      {
        GameObject crab = GameObject.Find("VoidRaidCrabBody(Clone)");
        if (crab)
          crab.GetComponent<VoidRaidCrabHealthBarOverlayProvider>().OnEnable();
      }
    }

    private void BaseSpinBeamAttackState_OnEnter(On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.orig_OnEnter orig, BaseSpinBeamAttackState self)
    {
      SpinBeamAttack.beamRadius = 32f;
      self.headForwardYCurve = AnimationCurve.Linear(0, 0, 10, 0);
      orig(self);
    }

    private void DeathState_OnEnter(On.EntityStates.VoidRaidCrab.DeathState.orig_OnEnter orig, DeathState self)
    {
      // Body TrueDeath TrueDeath.playbackRate 5
      self.animationStateName = "ChargeWipe";
      self.animationPlaybackRateParam = "Wipe.playbackRate";
      self.addPrintController = false;
      orig(self);
      PrintController printController = self.modelTransform.gameObject.AddComponent<PrintController>();
      printController.printTime = self.printDuration;
      printController.enabled = true;
      printController.startingPrintHeight = 200f;
      printController.maxPrintHeight = 500f;
      printController.startingPrintBias = self.startingPrintBias;
      printController.maxPrintBias = self.maxPrintBias;
      printController.disableWhenFinished = false;
      printController.printCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1f, 1f);
    }

    private void DeathState_OnExit(On.EntityStates.VoidRaidCrab.DeathState.orig_OnExit orig, DeathState self)
    {
      orig(self);
      GameObject[] joints = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == "VoidRaidCrabJointBody(Clone)" || go.name == "VoidRaidCrabJointMaster(Clone)").ToArray();
      foreach (GameObject joint in joints)
      {
        if (NetworkServer.active)
          NetworkServer.Destroy(joint);
        else
          Destroy(joint);
      }
      Run.instance.BeginGameOver(voidEnding);
      GameObject phases = GameObject.Find("EncounterPhases");
      if (phases)
        phases.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
    }

    private void ChargeGauntlet_OnEnter(On.EntityStates.VoidRaidCrab.ChargeGauntlet.orig_OnEnter orig, ChargeGauntlet self)
    {
      self.outer.SetState(new ChargeWardWipe());
    }

    private void ChargeWardWipe_OnEnter(On.EntityStates.VoidRaidCrab.ChargeWardWipe.orig_OnEnter orig, ChargeWardWipe self)
    {
      orig(self);
      PhasedInventorySetter component1 = self.GetComponent<PhasedInventorySetter>();
      if ((bool)component1 && NetworkServer.active)
        component1.AdvancePhase();
      ChargeGauntlet gauntlet = new ChargeGauntlet();
      if (!(bool)gauntlet.nextSkillDef)
        return;
      GenericSkill skillByDef = self.skillLocator.FindSkillByDef(gauntlet.skillDefToReplaceAtStocksEmpty);
      if (!(bool)skillByDef || skillByDef.stock != 0)
        return;
      skillByDef.SetBaseSkill(gauntlet.nextSkillDef);
    }

    private void ChargeFinalStand_OnEnter(On.EntityStates.VoidRaidCrab.ChargeFinalStand.orig_OnEnter orig, ChargeFinalStand self)
    {
      GameObject phases = GameObject.Find("EncounterPhases");
      if (phases)
        phases.transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
      orig(self);
    }

    private void Collapse_OnEnter(On.EntityStates.VoidRaidCrab.Collapse.orig_OnEnter orig, Collapse self)
    {
      self.outer.SetState(new BetterCollapse());
    }

    private void ReEmerge_OnEnter(On.EntityStates.VoidRaidCrab.ReEmerge.orig_OnEnter orig, ReEmerge self)
    {
      self.outer.SetState(new BetterReEmerge());
    }

    private void VoidRaidCrab_SpawnState(On.EntityStates.VoidRaidCrab.SpawnState.orig_OnEnter orig, SpawnState self)
    {
      if (self.characterBody.name == "VoidRaidCrabBody(Clone)")
        self.outer.SetState(new BetterSpawnState());
      else
        orig(self);
    }

    private void Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
    {
      orig(self);
      if (self.sceneDef.cachedName == "voidraid")
      {
        GameObject phases = GameObject.Find("EncounterPhases");
        Transform cam = GameObject.Find("RaidVoid").transform.GetChild(8);
        if (phases)
        {
          GameObject p3Music = phases.transform.GetChild(2).GetChild(1).gameObject;
          p3Music.name = "MusicEnd";
          p3Music.transform.parent = phases.transform.GetChild(0);
          if (NetworkServer.active)
          {
            NetworkServer.Destroy(phases.transform.GetChild(1).gameObject);
            NetworkServer.Destroy(phases.transform.GetChild(2).gameObject);
          }
          else
          {
            Destroy(phases.transform.GetChild(1).gameObject);
            Destroy(phases.transform.GetChild(2).gameObject);
          }
          Transform phase1 = phases.transform.GetChild(0);
          phase1.GetChild(0).position = new Vector3(0, -300, 0);
          phase1.GetComponent<ScriptedCombatEncounter>().spawns = new ScriptedCombatEncounter.SpawnInfo[1] { new ScriptedCombatEncounter.SpawnInfo() { spawnCard = voidlingCard, explicitSpawnPosition = phase1.GetChild(0) } };
          if (cam)
          {
            Transform curve = cam.GetChild(2);
            curve.GetChild(0).position = new Vector3(-0.2f, 217.13f, -442.84f);
            curve.GetChild(1).position = new Vector3(-12.5f, 29.7f, -181.4f);
          }
        }
      }
    }
  }
}