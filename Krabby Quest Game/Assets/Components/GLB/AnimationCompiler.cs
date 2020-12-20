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

        AnimatorController currentUnityAnimator;
        UnityEngine.Animator _currentAnimator;

        public AnimationCompiler()
        {

        }

        public GameObject CompileAnimations(string B3DPath, string GLBPath, string TexPath)
        {            
            Target = Importer.LoadFromFile(GLBPath);
            var renderer = Target.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                TargetMesh = renderer.sharedMesh;
                renderer.material = TextureLoader.RequestMaterialTexture(TexPath);
            }
            B3DLoader = new B3D_Loader();
            B3DLoader.LoadB3D(B3DPath);
            LoadObject(Target, "");
            return Target;
            //_currentAnimator.Rebind();
            //_currentAnimator.Play("Bip01");            
            //GLTFObject gltfObject = Exporter.CreateGLTFObject(Target.transform);
			//File.WriteAllText(Path.Combine(Path.GetDirectoryName(GLBPath), "exportGLBanimc.glb"),
             //   JsonConvert.SerializeObject(gltfObject, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
        }

        private void LoadObject(GameObject target, string path)
        {
            string objName = target.name;
            BlitzObject b3dObject = B3DLoader.LoadedObjects.FirstOrDefault(x => x.Name == objName);
            bool pathAppended = false;
            if (b3dObject != null)
            {
                if (b3dObject.HasAnimator)
                {
                    currentAnimator = b3dObject.Animator;
                    currentUnityAnimator = createController($"Compiled {objName} Controller");
                    _currentAnimator = target.AddComponent<UnityEngine.Animator>(); 
                    _currentAnimator.runtimeAnimatorController = currentUnityAnimator as RuntimeAnimatorController;
                    path = "";                    
                }
                if (b3dObject is MeshModel && objName == TargetMesh.name)
                {
                    currentBlitzMesh = b3dObject as MeshModel;
                    var currentBoneWeights = TargetMesh.boneWeights;
                    BoneWeight1[] skinBones = new BoneWeight1[currentBoneWeights.Length];
                    foreach(var bone in currentBlitzMesh.Bones)
                    {
                        
                    }
                }
                if (currentAnimator != null)
                {
                    var currentB3DObj = currentAnimator.Objects.FirstOrDefault(x => x.Name == objName);
                    if (currentB3DObj != null)
                    {
                        int animIndex = currentAnimator.Objects.IndexOf(currentB3DObj);
                        var anim = currentAnimator.Animations.ElementAt(animIndex);
                        var newClip = createClip(anim.keys[0], target.transform, path);
                        newClip.name = currentB3DObj.Name;                        
                        newClip.wrapMode = WrapMode.Loop;
                        var state = makeStateFromMotion(newClip, newClip.name, currentB3DObj);                        
                        state.speed = 14.0f;                                                
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

        private AnimatorState makeStateFromMotion(AnimationClip motion, string name, BlitzObject current)
        {
            float weight = 1.0f;
            //if (current is StinkyFile.Blitz3D.Prim.Pivot)
             //   weight = ((StinkyFile.Blitz3D.Prim.Pivot)current).Weight;
            var controllerLayer = new AnimatorControllerLayer()
            {
                name = name,
                stateMachine = new AnimatorStateMachine() { name = name },   
                defaultWeight = weight,
                blendingMode = AnimatorLayerBlendingMode.Additive
            };
            currentUnityAnimator.AddLayer(controllerLayer);
            var rootStateMachine = controllerLayer.stateMachine;
            var state = rootStateMachine.AddState(name);
            var loopState = rootStateMachine.AddState("Animation Completed");
            var transition = state.AddTransition(loopState);
            transition.hasExitTime = true;
            transition = loopState.AddTransition(state);
            transition.hasExitTime = true;
            state.motion = motion;
            return state;
        }

        private AnimatorController createController(string name)
        {
            // Creates the controller
            Directory.CreateDirectory($"Assets/Resources/Animations/{name}");
            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath($"Assets/Resources/Animations/{name}/AnimatorController.controller");
            controller.name = name;

            // Add parameters
            controller.AddParameter("Blend", AnimatorControllerParameterType.Float);                       

            // Add StateMachines
            var rootStateMachine = controller.layers[0].stateMachine;

            //var defaultMachine = rootStateMachine.AddStateMachine("DefaultMachine");

            /*var exitTransition = stateA1.AddExitTransition();
            exitTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "TransitionNow");
            exitTransition.duration = 0;

            var resetTransition = rootStateMachine.AddAnyStateTransition(stateA1);
            resetTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Reset");
            resetTransition.duration = 0;

            var transitionB1 = stateMachineB.AddEntryTransition(stateB1);
            transitionB1.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "GotoB1");
            stateMachineB.AddEntryTransition(stateB2);
            stateMachineC.defaultState = stateC2;
            var exitTransitionC2 = stateC2.AddExitTransition();
            exitTransitionC2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "TransitionNow");
            exitTransitionC2.duration = 0;

            var stateMachineTransition = rootStateMachine.AddStateMachineTransition(stateMachineA, stateMachineC);
            stateMachineTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "GotoC");
            rootStateMachine.AddStateMachineTransition(stateMachineA, stateMachineB);*/
            return controller;
        }

        private AnimationClip createClip(StinkyFile.Blitz3D.Prim.Animation animation, Transform target, string path)
        {
            var clip = new AnimationClip()
            {
                wrapMode = WrapMode.Default,
                frameRate = 30
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
            return clip;
        }                    
    }
}

