using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.VoidRaidCrab;
using R2API;
using EntityStates.VoidRaidCrab;
using EntityStates;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;
using RoR2.CharacterAI;
using RoR2.Skills;
using RoR2.Audio;
using System.Collections;

namespace FathomlessVoidling
{
  [BepInPlugin("com.Nuxlar.FathomlessVoidling", "FathomlessVoidling", "0.9.11")]

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
    public static GameObject bigLaserWarning = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MultiBeamRayIndicator.prefab").WaitForCompletion(), "BigLaserIndicatorNux");
    public static GameObject voidRainTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/TracerVoidRaidCrabTripleBeamSmall.prefab").WaitForCompletion();
    public static GameObject voidRainExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamExplosion.prefab").WaitForCompletion();
    private static GameObject voidlingMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMaster.prefab").WaitForCompletion();
    private static SkillDef primaryDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabEyeMissiles.asset").WaitForCompletion();
    private static SkillDef secondaryDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabMultiBeam.asset").WaitForCompletion();
    private static SkillDef utilityDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabSpinBeam.asset").WaitForCompletion();
    private static SkillDef specialDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabVacuumAttack.asset").WaitForCompletion();
    private static SkillDef gauntletDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabChannelGauntlet.asset").WaitForCompletion();
    public static GameObject suckSphereEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/KillSphereVfxPlaceholder.prefab").WaitForCompletion(), "WSingularitySphere");
    public static GameObject suckCenterEffect = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSuckLoopFX.prefab").WaitForCompletion(), "WSingularityCenter");
    public static GameObject wSingularityProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab").WaitForCompletion(), "WSingularityProjectile");
    public static GameObject wSingularityGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBombGhost.prefab").WaitForCompletion(), "WSingularityGhost");
    public static LoopSoundDef singularityLSD = Addressables.LoadAssetAsync<LoopSoundDef>("RoR2/DLC1/VoidRaidCrab/lsdVoidRaidCrabVacuumAttack.asset").WaitForCompletion();
    public static GameObject voidEyeModel = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabBody.prefab").WaitForCompletion().GetComponent<ModelLocator>().modelTransform.GetChild(4).gameObject, "mdlVoidEye");
    public static GameObject voidEye = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ArtifactShell/ArtifactShellBody.prefab").WaitForCompletion(), "VoidEyeBody");
    public static GameObject voidEyeMaster = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ArtifactShell/ArtifactShellMaster.prefab").WaitForCompletion(), "VoidEyeMaster");
    public static Material voidEyeMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidRaidCrab/matVoidRaidCrabEye.mat").WaitForCompletion();
    public static Material voidEyeMat2 = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidRaidCrab/matVoidRaidCrabEyeOverlay1.mat").WaitForCompletion();
    public static Material voidEyeMat3 = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidRaidCrab/matVoidRaidCrabEyeOverlay2.mat").WaitForCompletion();
    public static Material voidEyeMat4 = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidRaidCrab/matVoidRaidCrabEyeOverlay3.mat").WaitForCompletion();
    public static GameObject meteor = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab").WaitForCompletion(), "VoidMeteor");
    private static GameObject meteorGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab").WaitForCompletion(), "VoidMeteorGhost");
    private static Material boulderMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Grandparent/matGrandparentBoulderProjectile.mat").WaitForCompletion();
    private static Material voidAffixMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat").WaitForCompletion();
    private bool shouldGauntlet = false;
    private int jointCounter = 0;
    private static SkillDef finalStandDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabFinalStand.asset").WaitForCompletion();

    public static ConfigEntry<float> bodyBaseHP;
    public static ConfigEntry<float> bodyLevelHP;
    public static ConfigEntry<float> jointBaseHP;
    public static ConfigEntry<float> jointLevelHP;
    public static ConfigEntry<float> loopMultiplier;
    private static ConfigFile FVConfig { get; set; }

    public void Awake()
    {
      FVConfig = new ConfigFile(Paths.ConfigPath + "\\com.Nuxlar.FathomlessVoidling.cfg", true);
      loopMultiplier = FVConfig.Bind<float>("General", "Loop Multiplier", 1f, "Loop Count x Multiplier x All HP Values. Set to 0 for no multiplying.");
      bodyBaseHP = FVConfig.Bind<float>("Stats", "Main Body Base HP", 500f, "Base HP for the main body, higher main hp and lower joint hp will make joints do less damage. Vanilla: 8000");
      bodyLevelHP = FVConfig.Bind<float>("Stats", "Main Body Level HP", 150f, "Level HP for the main body, higher main hp and lower joint hp will make joints do less damage. Vanilla: 2400");
      jointBaseHP = FVConfig.Bind<float>("Stats", "Joint Base HP", 500f, "Base HP for each joint. Vanilla: 1000");
      jointLevelHP = FVConfig.Bind<float>("Stats", "Joint Level HP", 150f, "Level HP for each joint. Vanilla: 300");
      /*
      Destroy(voidEye.GetComponent<PurchaseInteraction>());
      Destroy(voidEye.GetComponent<Highlight>());
      ModelLocator modelLocator = voidEye.GetComponent<ModelLocator>();
      modelLocator.modelTransform.GetChild(2).localScale = new Vector3(32f, 32f, 32f);
      modelLocator.modelTransform.GetChild(2).gameObject.GetComponent<MeshRenderer>().sharedMaterials = new Material[] { voidEyeMat2, voidEyeMat3, voidEyeMat4, voidEyeMat };
      modelLocator.modelTransform.GetChild(3).gameObject.SetActive(false);
      Destroy(modelLocator.modelTransform.gameObject.GetComponent<SurfaceDefProvider>());
      modelLocator.modelTransform.gameObject.GetComponent<SphereCollider>().radius = 20f;
      HurtBoxGroup hbxGroup = modelLocator.modelTransform.gameObject.AddComponent<HurtBoxGroup>();
      HurtBox mainHurtbox = modelLocator.modelTransform.gameObject.AddComponent<HurtBox>();
      mainHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
      mainHurtbox.healthComponent = voidEye.GetComponent<HealthComponent>();
      mainHurtbox.isBullseye = true;
      mainHurtbox.isSniperTarget = false;
      mainHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
      mainHurtbox.hurtBoxGroup = hbxGroup;
      mainHurtbox.indexInGroup = 0;
      hbxGroup.hurtBoxes = new HurtBox[]
                {
                    mainHurtbox,
                };
      hbxGroup.mainHurtBox = mainHurtbox;
      hbxGroup.bullseyeCount = 1;
      
      SkillDef eyeSkill = voidEye.GetComponent<SkillLocator>().primary.skillFamily.variants[0].skillDef;
      eyeSkill.activationState = new SerializableEntityStateType(typeof(EyeBlast));
      eyeSkill.baseRechargeInterval = 20f;

      CharacterBody eyeBody = voidEye.GetComponent<CharacterBody>();
      eyeBody.baseNameToken = "Void Eye";
      eyeBody.baseMaxHealth = 1000f;
      eyeBody.levelMaxHealth = 300f;
      eyeBody.baseRegen = 0f;
      eyeBody.levelRegen = 0f;
      eyeBody.baseArmor = 10f;
      eyeBody.levelArmor = 0f;
      eyeBody.baseMoveSpeed = 0f;
      eyeBody.levelMoveSpeed = 0f;

      voidEye.GetComponents<EntityStateMachine>().Where(machine => machine.customName == "Life").First().initialStateType = new SerializableEntityStateType(typeof(VoidEyeSpawn));
      AISkillDriver eyeSkillDriver = voidEyeMaster.GetComponents<AISkillDriver>().Where(driver => driver.skillSlot == SkillSlot.Primary).First();
      eyeSkillDriver.requireSkillReady = true;
      eyeSkillDriver.maxUserHealthFraction = 1f;

      voidEyeMaster.GetComponent<CharacterMaster>().bodyPrefab = voidEye;

      ContentAddition.AddBody(voidEye);
      ContentAddition.AddMaster(voidEyeMaster);
      */

      LineRenderer lineRenderer = bigLaserWarning.transform.GetChild(1).GetComponent<LineRenderer>();
      lineRenderer.startWidth = 20f;
      lineRenderer.endWidth = 20f;
      lineRenderer.textureMode = LineTextureMode.Stretch;

      ProjectileController component = meteor.GetComponent<ProjectileController>();
      component.cannotBeDeleted = true;
      meteor.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
      meteorGhost.transform.localScale = new Vector3(2f, 2f, 2f);
      meteorGhost.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = new Material[2]
      {
        boulderMat,
        voidAffixMat
      };
      component.ghost = meteorGhost.GetComponent<ProjectileGhostController>();
      component.ghostPrefab = meteorGhost;
      ProjectileSimple projectileSimple1 = meteor.GetComponent<ProjectileSimple>();
      projectileSimple1.desiredForwardSpeed = 0f;
      projectileSimple1.lifetime = 15f;
      meteor.GetComponent<Rigidbody>().useGravity = false;
      meteor.AddComponent<ProjectileTargetComponent>();
      meteor.AddComponent<ProjectileDirectionalTargetFinder>();
      meteor.AddComponent<ProjectileSteerTowardTarget>();
      meteor.AddComponent<DelayedProjectileFire>();
      ContentAddition.AddProjectile(meteor);

      Destroy(suckSphereEffect.GetComponent<VFXHelper.VFXTransformController>());
      Destroy(suckCenterEffect.GetComponent<VFXHelper.VFXTransformController>());
      suckSphereEffect.transform.localScale = new Vector3(20f, 20f, 20f);

      foreach (Transform child in wSingularityGhost.transform)
      {
        Destroy(child.gameObject);
      }

      suckCenterEffect.transform.parent = wSingularityGhost.transform;
      suckSphereEffect.transform.parent = wSingularityGhost.transform;
      suckSphereEffect.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
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
      projectileSimple.desiredForwardSpeed = 15f;
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
            //skillDriver.maxUserHealthFraction = 0.66f;
            skillDriver.requiredSkill = null;
            break;
          case "Channel Gauntlet 2":
            // skillDriver.maxUserHealthFraction = 0.33f;
            skillDriver.requiredSkill = null;
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
      ContentAddition.AddEntityState<LaserBlast>(out _);
      ContentAddition.AddEntityState<AsteroidBelt>(out _);
      ContentAddition.AddEntityState<PortalBlast>(out _);
      ContentAddition.AddEntityState<WanderingSingularity>(out _);
      ContentAddition.AddEntityState<BetterSpawnState>(out _);
      ContentAddition.AddEntityState<BetterCollapse>(out _);
      ContentAddition.AddEntityState<BetterReEmerge>(out _);

      spinBeamVFX.transform.localScale *= 2;
      voidling.GetComponent<CharacterBody>().baseMaxHealth = bodyBaseHP.Value; // 8000
      voidling.GetComponent<CharacterBody>().levelMaxHealth = bodyLevelHP.Value; // 2400
      voidling.GetComponent<CharacterBody>().baseArmor = 40;
      joint.GetComponent<CharacterBody>().baseMaxHealth = jointBaseHP.Value; // 1000
      joint.GetComponent<CharacterBody>().levelMaxHealth = jointLevelHP.Value; // 300

      spawnEffect.transform.localScale *= 2;

      missileImpact.transform.localScale *= 4;
      // missileProjectile.GetComponent<ProjectileSimple>().oscillate = true;
      // missileProjectile.GetComponent<ProjectileSimple>().oscillateSpeed = 5f;
      missileProjectile.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 60f;
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
      gauntletDef.interruptPriority = InterruptPriority.PrioritySkill;
      secondaryDef.baseRechargeInterval = 20f;
      secondaryDef.activationState = new SerializableEntityStateType(typeof(ChargeVoidRain));
      utilityDef.activationState = new SerializableEntityStateType(typeof(ChargeWardWipe));
      utilityDef.interruptPriority = InterruptPriority.PrioritySkill;
      // voidling.GetComponent<SkillLocator>().primary.skillFamily.variants[0].skillDef.activationState = new SerializableEntityStateType(typeof(AsteroidBelt));
      // voidling.GetComponent<SkillLocator>().primary.skillFamily.variants[0].skillDef.baseMaxStock = 1;
      voidling.GetComponent<SkillLocator>().secondary.skillFamily.variants[0].skillDef = secondaryDef;
      voidling.GetComponent<SkillLocator>().utility.skillFamily.variants[0].skillDef = utilityDef;
      voidling.GetComponent<SkillLocator>().special.skillFamily.variants[0].skillDef.interruptPriority = InterruptPriority.PrioritySkill;

      GameObject model = voidling.GetComponent<ModelLocator>().modelTransform.gameObject;
      model.AddComponent<PrintController>();

      On.RoR2.Stage.Start += Stage_Start;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
      On.RoR2.VoidRaidCrab.LegController.SetJointMaster += SetJointMaster;
      On.EntityStates.VoidRaidCrab.Joint.PreDeathState.OnEnter += AddJointDeathDelay;
      // On.EntityStates.VoidRaidCrab.Joint.PreDeathState.FixedUpdate += AddJointBreakAuthority;
      // On.EntityStates.VoidRaidCrab.Joint.PreDeathState.SpawnJointEffect += SpawnJointEffectMP;
      // On.EntityStates.VoidRaidCrab.Joint.DeathState.OnEnter += SpawnJointDeathEffectMP;
      On.RoR2.VoidRaidCrab.CentralLegController.UpdateLegsAuthority += RemoveStunCollapse;
      On.EntityStates.VoidRaidCrab.SpawnState.OnEnter += VoidRaidCrab_SpawnState;
      On.EntityStates.VoidRaidCrab.Collapse.OnEnter += Collapse_OnEnter;
      On.EntityStates.VoidRaidCrab.ReEmerge.OnEnter += ReEmerge_OnEnter;
      On.EntityStates.VoidRaidCrab.VacuumAttack.OnEnter += VacuumAttack_OnEnter;
      On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.OnEnter += ChangeStateOnPhases;
      On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.OnEnter += BaseSpinBeamAttackState_OnEnter;
      On.EntityStates.EntityState.OnExit += SwapSecondaryOnExit;
      On.EntityStates.VoidRaidCrab.ChargeGauntlet.OnEnter += ChargeGauntlet_OnEnter;
      On.EntityStates.VoidRaidCrab.ChargeWardWipe.OnEnter += ChargeWardWipe_OnEnter;
      On.EntityStates.VoidRaidCrab.ChargeFinalStand.OnEnter += ChargeFinalStand_OnEnter;
      On.EntityStates.VoidRaidCrab.DeathState.OnEnter += DeathState_OnEnter;
      On.EntityStates.VoidRaidCrab.DeathState.OnExit += DeathState_OnExit;
      //  On.RoR2.VoidRaidCrab.LegController.CompleteBreakAuthority += ChangeSecondaryOnBreak;
      On.RoR2.VoidRaidCrab.VoidRaidCrabAISkillDriverController.ShouldUseGauntletSkills += SwapToGauntlet;
      On.RoR2.VoidRaidCrab.VoidRaidCrabAISkillDriverController.ShouldUseFinalStandSkill += SwapToFinalStand;
      On.EntityStates.VoidRaidCrab.ChargeWardWipe.GetMinimumInterruptPriority += IncreaseInterruptPriorityChargeWardWipe;
      On.EntityStates.VoidRaidCrab.FireWardWipe.GetMinimumInterruptPriority += IncreaseInterruptPriorityFireWardWipe;
    }

    private void AddJointDeathDelay(On.EntityStates.VoidRaidCrab.Joint.PreDeathState.orig_OnEnter orig, EntityStates.VoidRaidCrab.Joint.PreDeathState self)
    {
      orig(self);
      if (self.isAuthority)
      {
        ChildLocator childLocator = self.gameObject.GetComponent<ChildLocatorMirrorController>().referenceLocator;
        childLocator.gameObject.GetComponent<LegController>().CompleteBreakAuthority();
      }
    }

    private void RemoveStunCollapse(On.RoR2.VoidRaidCrab.CentralLegController.orig_UpdateLegsAuthority orig, CentralLegController self)
    {
      self.legSupportTracker.allLegsNeededForAnimation = !self.bodyStateMachine.IsInMainState();
      self.legSupportTracker.Refresh();
      bool flag = false;
      for (int index = 0; index < self.legControllers.Length; ++index)
      {
        if (self.legControllers[index].IsBreakPending())
        {
          flag = true;
        }
      }
      if (flag)
      {
        if (!self.legSupportTracker.IsLegStateStable())
        {
          for (int index = 0; index < self.legControllers.Length; ++index)
          {
            if (self.legControllers[index].IsBroken())
              self.legControllers[index].RegenerateServer();
          }
        }
      }
      self.TryNextStompAuthority();
    }

    private bool SetJointMaster(On.RoR2.VoidRaidCrab.LegController.orig_SetJointMaster orig, LegController self, CharacterMaster master, ChildLocator legChildLocator)
    {
      self.legChildLocator = legChildLocator;
      if (!(bool)self.jointMaster)
      {
        self.jointMaster = master;
        if ((bool)self.jointMaster)
        {
          self.jointMaster.onBodyDestroyed += new Action<CharacterBody>(self.OnJointBodyDestroyed);
          self.jointMaster.onBodyStart += new Action<CharacterBody>(self.OnJointBodyStart);
          self.MirrorLegJoints();
        }
        return true;
      }
      return false;
    }

    private void SpawnJointEffectMP(On.EntityStates.VoidRaidCrab.Joint.PreDeathState.orig_SpawnJointEffect orig, EntityStates.VoidRaidCrab.Joint.PreDeathState self, string jointName, ChildLocator childLocator)
    {
      Transform child = childLocator.FindChild(jointName);
      if (!(bool)child)
        return;
      GameObject effect = GameObject.Instantiate(self.jointEffectPrefab, child);
      effect.AddComponent<NetworkIdentity>();
      NetworkServer.Spawn(effect);
      self.jointEffects.Add(effect);
    }

    private void SpawnJointDeathEffectMP(On.EntityStates.VoidRaidCrab.Joint.DeathState.orig_OnEnter orig, EntityStates.VoidRaidCrab.Joint.DeathState self)
    {
      IntPtr ptr = typeof(BaseState).GetMethod(nameof(BaseState.OnEnter)).MethodHandle.GetFunctionPointer();
      Action baseOnEnter = (Action)Activator.CreateInstance(typeof(Action), self, ptr);
      baseOnEnter();

      EffectManager.SimpleMuzzleFlash(self.joint1EffectPrefab, self.gameObject, self.joint1Name, true);
      EffectManager.SimpleMuzzleFlash(self.joint2EffectPrefab, self.gameObject, self.joint2Name, true);
      EffectManager.SimpleMuzzleFlash(self.joint3EffectPrefab, self.gameObject, self.joint3Name, true);
      Transform modelTransform = self.GetModelTransform();
      if ((bool)modelTransform)
        self.characterModel = modelTransform.GetComponent<CharacterModel>();
      if (!(bool)self.characterModel)
        return;
      ++self.characterModel.invisibilityCount;
    }

    private InterruptPriority IncreaseInterruptPriorityFireWardWipe(On.EntityStates.VoidRaidCrab.FireWardWipe.orig_GetMinimumInterruptPriority orig, FireWardWipe self)
    {
      return InterruptPriority.Death;
    }

    private InterruptPriority IncreaseInterruptPriorityChargeWardWipe(On.EntityStates.VoidRaidCrab.ChargeWardWipe.orig_GetMinimumInterruptPriority orig, ChargeWardWipe self)
    {
      return InterruptPriority.Death;
    }

    private bool SwapToFinalStand(On.RoR2.VoidRaidCrab.VoidRaidCrabAISkillDriverController.orig_ShouldUseFinalStandSkill orig, VoidRaidCrabAISkillDriverController self)
    {
      bool shouldUseFinalStand = orig(self);
      if (shouldUseFinalStand)
      {
        self.healthComponent.body.skillLocator.utility.SetSkillOverride(self.gameObject, finalStandDef, GenericSkill.SkillOverridePriority.Replacement);
        self.healthComponent.body.skillLocator.utility.stock = self.healthComponent.body.skillLocator.utility.maxStock;
      }
      return shouldUseFinalStand;
    }

    private void ChangeStateOnPhases(On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.orig_OnEnter orig, BaseVacuumAttackState self)
    {
      PhasedInventorySetter inventorySetter = self.characterBody.GetComponent<PhasedInventorySetter>();
      if (inventorySetter)
      {
        if (inventorySetter.phaseIndex == 0)
        {
          orig(self);
        }
        if (inventorySetter.phaseIndex == 1)
        {
          self.outer.SetState(new WanderingSingularity());
          return;
        }
        if (inventorySetter.phaseIndex == 2)
        {
          self.outer.SetState(new WanderingSingularity());
          return;
        }
      }
      else
        orig(self);
    }

    private bool SwapToGauntlet(On.RoR2.VoidRaidCrab.VoidRaidCrabAISkillDriverController.orig_ShouldUseGauntletSkills orig, VoidRaidCrabAISkillDriverController self)
    {
      bool shouldUseGauntlet = orig(self);
      if (shouldUseGauntlet)
      {
        if (self.healthComponent.body.skillLocator.secondary.skillDef != gauntletDef)
        {
          shouldGauntlet = true;
          self.healthComponent.body.skillLocator.utility.SetSkillOverride(self.gameObject, gauntletDef, GenericSkill.SkillOverridePriority.Replacement);
          self.healthComponent.body.skillLocator.utility.stock = self.healthComponent.body.skillLocator.utility.maxStock;
        }
      }
      return shouldUseGauntlet;
    }

    private void SwapSecondaryOnExit(On.EntityStates.EntityState.orig_OnExit orig, EntityStates.EntityState self)
    {
      if (self is FireWardWipe)
      {
        shouldGauntlet = false;
        self.characterBody.skillLocator.utility.SetSkillOverride(self.gameObject, utilityDef, GenericSkill.SkillOverridePriority.Replacement);
        self.healthComponent.body.skillLocator.utility.stock = self.healthComponent.body.skillLocator.utility.maxStock;
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
      // VacuumAttack.pullMagnitudeCurve = AnimationCurve.Linear(0, 0, 1, 45);
      orig(self);
    }


    private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
    {
      orig(self, body);
      if (body.name == "VoidRaidCrabJointBody(Clone)")
      {
        body.inventory.GiveItemString("AdaptiveArmor");
        self.isBoss = true;
        if (self.isServer)
          return;
        string legName = "";
        switch (this.jointCounter)
        {
          case 0:
            legName = "FrontLegL";
            break;
          case 1:
            legName = "FrontLegR";
            break;
          case 2:
            legName = "MidLegL";
            break;
          case 3:
            legName = "MidLegR";
            break;
          case 4:
            legName = "BackLegL";
            break;
          case 5:
            legName = "BackLegR";
            break;
        }
        this.jointCounter++;
        CharacterBody mainBody = self.GetComponent<MinionOwnership>().ownerMaster.GetBody();
        if (!(bool)mainBody)
          mainBody = GameObject.Find("VoidRaidCrabBody(Clone)").GetComponent<CharacterBody>();
        ChildLocator childLocator = mainBody.modelLocator.modelTransform.GetComponent<ChildLocator>();
        if (!(bool)childLocator)
          return;
        Transform child = childLocator.FindChild(legName);
        if (!(bool)child)
          return;
        LegController legController = child.GetComponent<LegController>();
        if (!(bool)legController)
          return;
        if (this.jointCounter >= 6)
        {
          legController.MirrorLegJoints();
          return;
        }
        legController.SetJointMaster(body.master, child.GetComponent<ChildLocator>());
      }
      if (body.isPlayerControlled && SceneManager.GetActiveScene().name == "voidraid" && body.HasBuff(RoR2Content.Buffs.Immune))
      {
        GameObject crab = GameObject.Find("VoidRaidCrabBody(Clone)");
        if (crab)
          crab.GetComponent<VoidRaidCrabHealthBarOverlayProvider>().OnEnable();
      }
    }

    private void BaseSpinBeamAttackState_OnEnter(On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.orig_OnEnter orig, BaseSpinBeamAttackState self)
    {
      PhasedInventorySetter inventorySetter = self.characterBody.GetComponent<PhasedInventorySetter>();
      if (inventorySetter)
      {
        if (inventorySetter.phaseIndex == 0)
        {
          SpinBeamAttack.beamRadius = 22f;
          self.headForwardYCurve = AnimationCurve.Linear(0, 0, 10, 0);
          orig(self);
        }
        if (inventorySetter.phaseIndex == 1)
        {
          self.outer.SetState(new LaserBlast());
          return;
        }
        if (inventorySetter.phaseIndex == 2)
        {
          self.outer.SetState(new PortalBlast());
          return;
        }
      }
      else
      {
        SpinBeamAttack.beamRadius = 22f;
        self.headForwardYCurve = AnimationCurve.Linear(0, 0, 10, 0);
        orig(self);
      }
    }

    private void DeathState_OnEnter(On.EntityStates.VoidRaidCrab.DeathState.orig_OnEnter orig, DeathState self)
    {
      // Body TrueDeath TrueDeath.playbackRate 5
      if (self.characterBody.name == "VoidRaidCrabBody(Clone)")
      {
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
      else orig(self);
    }

    private void DeathState_OnExit(On.EntityStates.VoidRaidCrab.DeathState.orig_OnExit orig, DeathState self)
    {
      orig(self);
      if (self.characterBody.name == "VoidRaidCrabBody(Clone)")
      {
        GameObject[] joints = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name == "VoidRaidCrabJointBody(Clone)" || go.name == "VoidRaidCrabJointMaster(Clone)").ToArray();
        foreach (GameObject joint in joints)
        {
          if (NetworkServer.active)
            NetworkServer.Destroy(joint);
          else
            Destroy(joint);
        }
        Run.instance.BeginGameOver(voidEnding);
        /*
        GameObject gauntlet = GameObject.Find("HOLDER: Gauntlets");
        if (gauntlet)
          gauntlet.GetComponent<VoidRaidGauntletController>().SpawnOutroPortal();
          */
        GameObject phases = GameObject.Find("EncounterPhases");
        if (phases)
          phases.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
      }
    }

    private void ChargeGauntlet_OnEnter(On.EntityStates.VoidRaidCrab.ChargeGauntlet.orig_OnEnter orig, ChargeGauntlet self)
    {
      self.outer.SetState(new ChargeWardWipe());
    }

    private void ChargeWardWipe_OnEnter(On.EntityStates.VoidRaidCrab.ChargeWardWipe.orig_OnEnter orig, ChargeWardWipe self)
    {
      self.safeWardSpawnCurve = AnimationCurve.Linear(0, 15, 0.25f, 20);
      PhasedInventorySetter inventorySetter = self.characterBody.GetComponent<PhasedInventorySetter>();

      if (inventorySetter)
      {
        if (this.shouldGauntlet)
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
        else
        {
          if (inventorySetter.phaseIndex == 0)
          {
            self.outer.SetState(new SpinBeamEnter());
          }
          if (inventorySetter.phaseIndex == 1)
          {
            self.outer.SetState(new LaserBlast());
          }
          if (inventorySetter.phaseIndex == 2)
          {
            self.outer.SetState(new PortalBlast());
          }
        }
      }
      else
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

    private IEnumerator Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
    {
      yield return orig(self);
      if (self.sceneDef.cachedName == "voidraid")
      {
        if (Run.instance)
        {
          if (Run.instance.loopClearCount > 1)
          {
            float multiplier = Run.instance.loopClearCount * loopMultiplier.Value;
            if (multiplier == 0)
              multiplier = 1;

            voidling.GetComponent<CharacterBody>().baseMaxHealth = bodyBaseHP.Value * multiplier; // 8000
            voidling.GetComponent<CharacterBody>().levelMaxHealth = bodyLevelHP.Value * multiplier; // 2400
            joint.GetComponent<CharacterBody>().baseMaxHealth = jointBaseHP.Value * multiplier; // 1000
            joint.GetComponent<CharacterBody>().levelMaxHealth = jointLevelHP.Value * multiplier; ; // 300
          }
          else
          {
            voidling.GetComponent<CharacterBody>().baseMaxHealth = bodyBaseHP.Value; // 8000
            voidling.GetComponent<CharacterBody>().levelMaxHealth = bodyLevelHP.Value; // 2400
            joint.GetComponent<CharacterBody>().baseMaxHealth = jointBaseHP.Value; // 1000
            joint.GetComponent<CharacterBody>().levelMaxHealth = jointLevelHP.Value; ; // 300
          }
        }
        jointCounter = 0;
        shouldGauntlet = false;
        GameObject phases = GameObject.Find("EncounterPhases");
        Transform cam = GameObject.Find("RaidVoid").transform.GetChild(5);
        if (phases)
        {
          GameObject p3Music = phases.transform.GetChild(2).GetChild(1).gameObject;
          p3Music.name = "MusicEnd";
          p3Music.transform.parent = phases.transform.GetChild(0);
          GameObject swapMusic = new GameObject("MusicSwap");
          swapMusic.AddComponent<NetworkIdentity>();
          MusicTrackOverride trackOverride = swapMusic.AddComponent<MusicTrackOverride>();
          trackOverride.priority = 1;
          trackOverride.track = MusicTrackCatalog.FindMusicTrackDef("muGameplayDLC1_08");
          swapMusic.transform.parent = phases.transform.GetChild(0);
          swapMusic.SetActive(false);
          if (NetworkServer.active)
            NetworkServer.Spawn(swapMusic);
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