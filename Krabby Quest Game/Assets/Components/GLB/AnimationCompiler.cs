using Newtonsoft.Json;
using Siccity.GLTFUtility;
using StinkyFile.Blitz3D;
using StinkyFile.Blitz3D.B3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.Components.GLB
{
    /// <summary>
    /// Extracts B3D animations and writes them to the GLB file
    /// </summary>
    public class AnimationCompiler
    {
        GameObject Target;        
        B3D_Loader B3DLoader;

        Mesh TargetMesh;
        MeshModel currentBlitzMesh;

        StinkyFile.Blitz3D.Animator currentAnimator;
        List<AnimationClip> loadedClips = new List<AnimationClip>();
        List<StinkyFile.Blitz3D.Animator> animators = new List<StinkyFile.Blitz3D.Animator>();

        AnimatorController currentUnityAnimator;
        UnityEngine.Animator _currentAnimator;

        string parentObjectName;
        string B3DName;
        string WorkspaceDirectory = "";
        bool creatingController = false;
        int currentIndex = 0;

        string[] flag_NotCompile =
        {
            "Scene Root",
            "B3DEXT_BGCOLOR",
            "B3DEXT_AMBIENT"
        };

        List<AnimatorState> SequenceStates = new List<AnimatorState>();

        /// <summary>
        /// A globally-available animation compiler
        /// </summary>
        public static AnimationCompiler GlobalAnimationCompiler
        {
            get; set;
        }

        public AnimationCompiler()
        {

        }

        public void CompileAnimations(string WorkspaceDir, string B3DName, GameObject Target, out IEnumerable<StinkyFile.Blitz3D.Animator> animators)
        {
            this.animators.Clear();
            this.B3DName = Path.GetFileNameWithoutExtension(B3DName);
            string B3DPath = Path.Combine(WorkspaceDir, B3DName);
            WorkspaceDirectory = WorkspaceDir;
            B3DLoader = new B3D_Loader();
            B3DLoader.LoadB3D(B3DPath);
            LoadObject(Target, "");
            animators = this.animators;
        }

        private void LoadObject(GameObject target, string path)
        {
            string objName = target.name;
            BlitzObject b3dObject = B3DLoader.LoadedObjects.FirstOrDefault(x => x.Name == objName);            
            bool pathAppended = false;
            if (b3dObject != null && !flag_NotCompile.Contains(b3dObject.Name))
            {
                if (b3dObject.HasAnimator)
                {
                    currentAnimator = b3dObject.Animator;
                    animators.Add(currentAnimator);
                    try
                    {
                        //attempt to load sequences from file
                        var animDir = Path.Combine(WorkspaceDirectory, "Animations");
                        currentAnimator.LoadSequencesFromPath(animDir, b3dObject.Name, B3DName);
                    }
                    catch
                    {

                    }
                    SequenceStates.Clear();
                    currentIndex = 0;
                    currentUnityAnimator = createController(objName, out creatingController);                   
                    creatingController = !creatingController;
                    _currentAnimator = target.AddComponent<UnityEngine.Animator>();
                    _currentAnimator.runtimeAnimatorController = currentUnityAnimator as RuntimeAnimatorController;
                    parentObjectName = objName;
                    path = "";
                }
                /*if (b3dObject is MeshModel && objName == TargetMesh.name)
                {
                    currentBlitzMesh = b3dObject as MeshModel;
                    var currentBoneWeights = TargetMesh.boneWeights;
                    BoneWeight1[] skinBones = new BoneWeight1[currentBoneWeights.Length];
                    foreach (var bone in currentBlitzMesh.Bones)
                    {

                    }
                }*/
                if (currentAnimator != null)
                {
                    var currentB3DObj = currentAnimator.Objects.FirstOrDefault(x => x.Name == objName);
                    if (currentB3DObj != null)
                    {
                        if(creatingController)
                            makeLayerForObject(currentB3DObj.Name, 1);
                        currentIndex++;
                        int animIndex = currentAnimator.Objects.IndexOf(currentB3DObj);
                        var anim = currentAnimator.Animations.ElementAt(animIndex);
                        foreach (var sequence in currentAnimator.Sequences)
                        {
                            var clip = createClip(sequence, anim.keys[sequence.ID], target.transform, path, $"Seq{sequence.ID} {currentB3DObj.Name}", out bool clipExisted);
                            var rootLayer = currentUnityAnimator.layers[0];
                            var animatorState = rootLayer.stateMachine.states[3 + sequence.ID];
                            currentUnityAnimator.SetStateEffectiveMotion(animatorState.state, clip, currentIndex);                            
                        }
                    }
                }
            }
            for(int i = 0; i < target.transform.childCount; i++)
            {
                var child = target.transform.GetChild(i);
                var rpath = path + (path != "" ? "/" : "") + $"{child.gameObject.name}";
                LoadObject(child.gameObject, rpath);
            }
        }

        private AnimatorControllerLayer makeLayerForObject(string name, float weight)
        {
            var controllerLayer = new AnimatorControllerLayer()
            {
                name = name,
                //stateMachine = layer,  
                syncedLayerAffectsTiming = true,
                defaultWeight = weight,
                blendingMode = AnimatorLayerBlendingMode.Additive,
                syncedLayerIndex = 0 // sync with base layer
            };
            currentUnityAnimator.AddLayer(controllerLayer);
            return controllerLayer;
        }

        private AnimatorController createController(string name, out bool existing)
        {
            existing = true;            
            string rootPath = "Assets/Models/Animations";
            if (!AssetDatabase.IsValidFolder(rootPath + "/" + name))
                AssetDatabase.CreateFolder(rootPath, name);
            string controllerPath = $"{rootPath}/{name}/AnimatorController.controller";

            //load existing controller
            if (File.Exists(controllerPath))
                return AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            existing = false;

            //create new controller
            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath($"Assets/Models/Animations/{name}/AnimatorController.controller");
            controller.name = name;

            // Add parameters
            controller.AddParameter("CanPlay", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsLooping", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Sequence", AnimatorControllerParameterType.Int);

            //Setup the controller for first time use
            SetupController(controller);

            return controller;
        }

        private void SetupController(AnimatorController controller)
        {
            // Add StateMachines
            var baseLayer = controller.layers[0];
            var rootStateMachine = baseLayer.stateMachine;

            //create base layer

            //Animation enter state
            var enterState = rootStateMachine.AddState("ANIM_ENTER");
            //rootStateMachine.AddEntryTransition(enterState);

            //Animation playing state
            var animationState = rootStateMachine.AddState("ANIM_PLAY");
            var enterToAnimPlay = enterState.AddTransition(animationState);
            enterToAnimPlay.AddCondition(AnimatorConditionMode.If, 1, "CanPlay"); // will only start animation if CanPlay is set in Animator
            enterToAnimPlay.duration = 0;

            //For animation looping
            var loopState = rootStateMachine.AddState("ANIM_LOOP");
            var loopToEnter = loopState.AddTransition(enterState);
            loopToEnter.AddCondition(AnimatorConditionMode.If, 1, "IsLooping"); //if is looping is set, loop back to playing state
            loopToEnter.duration = 0;
            loopState.AddExitTransition().AddCondition(AnimatorConditionMode.If, 0, "IsLooping"); // else exit the animator state            

            //Add sequence states
            foreach (var sequence in currentAnimator.Sequences)
            {
                var name = sequence.Name;
                if (string.IsNullOrEmpty(name))
                    name = "Seq" + sequence.ID;
                var seqState = rootStateMachine.AddState(name);
                animationState.AddTransition(seqState).AddCondition(AnimatorConditionMode.Equals, sequence.ID, "Sequence");
                var seqToLoop = seqState.AddTransition(loopState);
                seqToLoop.hasExitTime = true;
                seqToLoop.duration = 0;
            }            
        }

        private AnimationClip createClip(Seq BaseSequence, StinkyFile.Blitz3D.Prim.Animation animation, Transform target, string path, string name, out bool Existed)
        {
            string baseDir = $"Assets/Models/Animations/{parentObjectName}/{"Sequence " + BaseSequence.ID}/";
            string destPath = baseDir + $"{name}.anim";
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
            if (File.Exists(destPath))
            {
                Existed = true;
                return AssetDatabase.LoadAssetAtPath<AnimationClip>(destPath);
            }
            Existed = false;
            var clip = new AnimationClip()
            {
                wrapMode = WrapMode.Default,
                frameRate = 30,
                name = name
            };

            //position
            int total = animation.numPositionKeys();
            {
                Keyframe[] posXKeys = new Keyframe[total];
                Keyframe[] posYKeys = new Keyframe[total];
                Keyframe[] posZKeys = new Keyframe[total];
                for (int i = 0; i < total; i++)
                {
                    var currentKey = animation.AnimRep.pos_anim[i];
                    posXKeys[i] = new Keyframe(i, currentKey.V.X);
                    posYKeys[i] = new Keyframe(i, currentKey.V.Y);
                    posZKeys[i] = new Keyframe(i, currentKey.V.Z);
                }
                var curve = new AnimationCurve(posXKeys);               
                clip.SetCurve(path, target.GetType(), "localPosition.x", curve);
                curve = new AnimationCurve(posYKeys);
                clip.SetCurve(path, target.GetType(), "localPosition.y", curve);
                curve = new AnimationCurve(posZKeys);
                clip.SetCurve(path, target.GetType(), "localPosition.z", curve);
            }

            //rotation
            total = animation.numRotationKeys();
            {
                Keyframe[] rotXKeys = new Keyframe[total];
                Keyframe[] rotYKeys = new Keyframe[total];
                Keyframe[] rotZKeys = new Keyframe[total];
                Keyframe[] rotWKeys = new Keyframe[total];
                for (int i = 0; i < total; i++)
                {
                    var currentKey = animation.AnimRep.rot_anim[i];
                    Quaternion quat = new Quaternion(currentKey.V.X, currentKey.V.Y, currentKey.V.Z, currentKey.W);
                    quat.Normalize();
                    rotXKeys[i] = new Keyframe(i, quat.x);
                    rotYKeys[i] = new Keyframe(i, quat.y);
                    rotZKeys[i] = new Keyframe(i, quat.z);
                    rotWKeys[i] = new Keyframe(i, quat.w);
                }
                var curve = new AnimationCurve(rotXKeys);
                clip.SetCurve(path, target.GetType(), "localRotation.x", curve);
                curve = new AnimationCurve(rotYKeys);
                clip.SetCurve(path, target.GetType(), "localRotation.y", curve);
                curve = new AnimationCurve(rotZKeys);
                clip.SetCurve(path, target.GetType(), "localRotation.z", curve);
                curve = new AnimationCurve(rotWKeys);
                clip.SetCurve(path, target.GetType(), "localRotation.w", curve);                
            }

            //scale
            total = animation.numScaleKeys();
            {
                Keyframe[] sclXKeys = new Keyframe[total];
                Keyframe[] sclYKeys = new Keyframe[total];
                Keyframe[] sclZKeys = new Keyframe[total];
                for (int i = 0; i < total; i++)
                {
                    var currentKey = animation.AnimRep.scale_anim[i];
                    sclXKeys[i] = new Keyframe(i, currentKey.V.X);
                    sclYKeys[i] = new Keyframe(i, currentKey.V.Y);
                    sclZKeys[i] = new Keyframe(i, currentKey.V.Z);
                }
                var curve = new AnimationCurve(sclXKeys);
                clip.SetCurve(path, target.GetType(), "localScale.x", curve);
                curve = new AnimationCurve(sclYKeys);
                clip.SetCurve(path, target.GetType(), "localScale.y", curve);
                curve = new AnimationCurve(sclZKeys);
                clip.SetCurve(path, target.GetType(), "localScale.z", curve);
            }
            AssetDatabase.CreateAsset(clip, destPath);
            return clip;
        }                    
    }
}

