using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace ProceduralTools
{
    public class DragCubeTool : MonoBehaviour
    {
        public Part part;
        private int fxLayer;
        private int stall = 0;
        private bool symmetry = false;
        private static bool? _FARinstalled = null;
        public static bool FARinstalled => _FARinstalled ??= AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "FerramAerospaceResearch");

        public void Awake() 
        {
            fxLayer = LayerMask.NameToLayer("TransparentFX");
        }

        public static DragCubeTool UpdateDragCubes(Part p, bool immediate = false, int stall = 0, bool symmetry = false)
        {
            var tool = p.GetComponent<DragCubeTool>();
            if (tool == null)
            {
                tool = p.gameObject.AddComponent<DragCubeTool>();
                tool.part = p;
                tool.stall = stall;
                tool.symmetry = symmetry;
                if (immediate && tool.Ready())
                    tool.UpdateCubes();
            }
            return tool;
        }

        public void FixedUpdate()
        {
            if (Ready())
                UpdateCubes();
        }

        public bool Ready()
        {
            if (HighLogic.LoadedSceneIsFlight)
                return FlightGlobals.ready; //&& !part.packed && part.vessel.loaded;
            if (HighLogic.LoadedSceneIsEditor)
                return part.localRoot == EditorLogic.RootPart && part.gameObject.layer != fxLayer && --stall < 0;
            return true;
        }

        private void UpdateCubes()
        {
            Profiler.BeginSample("PF.DragCubeTool.UpdateCubes");
            Profiler.BeginSample("PF.DragCubeTool.FAR");
            if (FARinstalled)
                part.SendMessage("GeometryPartModuleRebuildMeshData");
            Profiler.EndSample();
            Profiler.BeginSample("PF.DragCubeTool.Render");
            var dragCube = DragCubeSystem.Instance.RenderProceduralDragCube(part);
            Profiler.EndSample();
            Profiler.BeginSample("PF.DragCubeTool.Complete");
            part.DragCubes.ClearCubes();
            part.DragCubes.Cubes.Add(dragCube);
            part.DragCubes.ResetCubeWeights();
            part.DragCubes.ForceUpdate(true, true, false);
            part.DragCubes.SetDragWeights();
            if (symmetry)
            {
                foreach (Part p in part.symmetryCounterparts)
                {
                    p.DragCubes.ClearCubes();
                    p.DragCubes.Cubes.Add(dragCube);
                    p.DragCubes.ResetCubeWeights();
                    p.DragCubes.ForceUpdate(true, true, false);
                    p.DragCubes.SetDragWeights();
                }
            }
            Destroy(this);
            Profiler.EndSample();
            Profiler.EndSample();
        }
    }
}
