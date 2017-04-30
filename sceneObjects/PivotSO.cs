﻿using System;
using g3;

namespace f3
{
    public class PivotSO : BaseSO
    {
        fGameObject pivotGO;
        fGameObject shapeGO;
        fGameObject frameGO;

        SOMaterial frameMaterial;

        /// <summary>
        /// A standard PivotSO is a "special" scene object, IE it is used more as a UI element.
        /// In that case we by default try to keep it the same "size" on-screen.
        /// Set to false to disable this behavior.
        /// </summary>
        public bool MaintainConsistentViewSize = true;


        public PivotSO()
        {
            
        }

        public virtual PivotSO Create(SOMaterial shapeMaterial, SOMaterial frameMaterial = null, int nShapeLayer = -1)
        {
            // [TODO] replace frame geometry with line renderer ?
            // [TODO] still cast shadows  (semitransparent objects don't cast shadows, apparently)
            // [TODO] maybe render solid when not obscured by objects? use raycast in PreRender?

            AssignSOMaterial(shapeMaterial);       // need to do this to setup BaseSO material stack

            pivotGO = GameObjectFactory.CreateParentGO(UniqueNames.GetNext("Pivot"));

            shapeGO = create_pivot_shape();
            AppendNewGO(shapeGO, pivotGO, false);

            pivotGO.AddChild(shapeGO);

            if (frameMaterial != null) {
                this.frameMaterial = frameMaterial;

                frameGO = UnityUtil.CreateMeshGO("pivotFrame", "icon_meshes/axis_frame", 1.0f,
                    UnityUtil.MeshAlignOption.NoAlignment, MaterialUtil.ToUnityMaterial(frameMaterial), false);
                MaterialUtil.SetIgnoreMaterialChanges(frameGO);
                MaterialUtil.DisableShadows(frameGO);
                AppendNewGO(frameGO, pivotGO, false);
            }

            if (nShapeLayer >= 0)
                shapeGO.SetLayer(nShapeLayer);

            increment_timestamp();
            return this;
        }


        protected virtual fGameObject create_pivot_shape()
        {
            fGameObject go = AppendUnityPrimitiveGO("pivotMesh", UnityEngine.PrimitiveType.Sphere, CurrentMaterial, null, true);
            go.SetLocalScale(0.9f * Vector3f.One);
            return go;
        }




        override public void DisableShadows() {
            MaterialUtil.DisableShadows(shapeGO, true, true);
            if ( frameGO != null )
                MaterialUtil.DisableShadows(frameGO, true, true);
        }




        //
        // SceneObject impl
        //

        override public fGameObject RootGameObject
        {
            get { return pivotGO; }
        }

        override public string Name
        {
            get { return pivotGO.GetName(); }
            set { pivotGO.SetName(value); }
        }

        override public SOType Type { get { return SOTypes.Pivot; } }

        public override bool IsSurface {
            get { return false; }
        }

        public override bool SupportsScaling {
            get { return false; }
        }

        public override void PreRender()
        {
            if (MaintainConsistentViewSize) {
                float fScaling = VRUtil.GetVRRadiusForVisualAngle(
                    pivotGO.GetPosition(),
                    parentScene.ActiveCamera.GetPosition(),
                    SceneGraphConfig.DefaultPivotVisualDegrees);
                fScaling /= parentScene.GetSceneScale();
                pivotGO.SetLocalScale(new Vector3f(fScaling, fScaling, fScaling));
            }
        }


        override public SceneObject Duplicate()
        {
            PivotSO copy = new PivotSO();
            copy.parentScene = this.parentScene;
            copy.Create(this.GetAssignedSOMaterial(), this.frameMaterial, shapeGO.GetLayer());
            copy.SetLocalFrame(
                this.GetLocalFrame(CoordSpace.ObjectCoords), CoordSpace.ObjectCoords);
            copy.MaintainConsistentViewSize = this.MaintainConsistentViewSize;
            return copy;
        }

    }

}
