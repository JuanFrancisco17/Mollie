#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace FTG
{
    [System.Serializable]
    public enum AppendageType { Branch, Leaves, SplineLeaf, SDFLeaves, CustomMesh };
    [System.Serializable]

    public abstract class Appendage
    {
        [SerializeField]
        public int IDInBranch;
        [SerializeReference]
        public List<bool> render;
        public int ID;
        [SerializeReference]
        public Branch parent;
        public int depth;
        [SerializeField]
        [Range(0, 1)]
        private float position;
        [SerializeField]
        public AppendageType type;
        [SerializeField]
        public Vector3 offset;
        public int seed;
        public bool dirty;
        public float Position
        {
            get
            {
                return position;
            }
            set
            {
                if ((value > 0) && (value < 1))
                    position = value;
                else if (value <= 0)
                {
                    position = 0.001f;
                }
                else
                {
                    position = 0.999f;
                }
            }
        }

        public Appendage()
        {
            dirty = true;
            render = new List<bool>();
            render.Add(true);
            render.Add(true);
            render.Add(true);

            render.Add(true);
            Position = 0.5f;

        }
        public abstract int GetVertexCount(int index);
        public abstract void AddLoD();
    }
    [System.Serializable]
    public class Branch : Appendage
    {
        [SerializeReference]
        private List<int> numberOfSides;
        [SerializeReference]
        private List<int> numberOfDivisions;
        [SerializeReference]
        public BezierSpline curve;
        public float noiseAmount;

        [SerializeReference]
        public List<Appendage> appendages;

        public void SetBranchDirty()
        {
            this.dirty = true;
            foreach (var item in this.appendages)
            {
                item.dirty = true;
            }
        }

        public int GetSides(int index)
        {
            return numberOfSides[index];
        }

        public int GetDivisions(int index)
        {
            return numberOfDivisions[index];
        }

        public void SetSides(int index, int value)
        {
            if (value >= 2)
                numberOfSides[index] = value;
            else
                numberOfSides[index] = 2;
        }

        public void SetDivisions(int index, int value)
        {
            if (value >= 2)
                numberOfDivisions[index] = value;
            else
                numberOfDivisions[index] = value;
        }

        public Branch() : base()
        {
            appendages = new List<Appendage>();
            noiseAmount = 0;
            seed = 1;
            numberOfSides = new List<int>();
            numberOfSides.Add(6);
            numberOfSides.Add(4);
            numberOfSides.Add(3);
            numberOfSides.Add(3);
            numberOfDivisions = new List<int>();
            numberOfDivisions.Add(5);
            numberOfDivisions.Add(4);
            numberOfDivisions.Add(3);
            numberOfDivisions.Add(2);
        }
        
        public override int GetVertexCount(int index)
        {
            return (GetSides(index) + 1) * (GetDivisions(index));
        }

        public override void AddLoD()
        {
            render.Add(render[render.Count - 1]);
            numberOfSides.Add(3);
            numberOfDivisions.Add(3);
        }
    }
    [System.Serializable]
    public class SplineLeaf : Appendage
    {

        [SerializeReference]
        private List<int> numberOfDivisions;
        [SerializeReference]
        public BezierSpline curve;
        public Quaternion rotation;

        public int GetDivisions(int index)
        {
            return numberOfDivisions[index];
        }

        public void SetDivisions(int index, int value)
        {
            if (value >= 2)
                numberOfDivisions[index] = value;
            else
                numberOfDivisions[index] = value;
        }
        public SplineLeaf() : base()
        {
            seed = 1;
            rotation = Quaternion.identity;
            type = AppendageType.SplineLeaf;
            numberOfDivisions = new List<int>();
            numberOfDivisions.Add(5);
            numberOfDivisions.Add(4);
            numberOfDivisions.Add(3);
            numberOfDivisions.Add(2);
        }
        public SplineLeaf(SplineLeaf leaf) : base()
        {
            numberOfDivisions = leaf.numberOfDivisions;
            Position = leaf.Position;
            rotation = leaf.rotation;
            parent = leaf.parent;
            type = AppendageType.SplineLeaf;
            curve = new BezierSpline(leaf.curve);
        }
        public override int GetVertexCount(int index)
        {
            return 2 * (GetDivisions(index));
        }

        public override void AddLoD()
        {
            render.Add(render[render.Count - 1]);
            numberOfDivisions.Add(3);
        }
    }
    [System.Serializable]
    public class CustomMesh : Appendage
    {
        public GameObject prefabObject;
        public GameObject instancedObject;
        public Vector3 positionOffset;
        public Vector3 meshPosition;
        public Quaternion rotation;
        public Vector3 scale;

        public CustomMesh() : base()
        {
            seed = 1;

            rotation = Quaternion.identity;
            scale = Vector3.one;
            positionOffset = Vector3.zero;


        }

        public override void AddLoD()
        {
            render.Add(render[render.Count - 1]);
        }

        public override int GetVertexCount(int index)
        {
            return 0;
            //This one doesnt need it, no point in making more classes
        }
    }
    [System.Serializable]
    public class Leaves : Appendage
    {

        public Vector2 range;
        public float offsetFromBranch;
        public AnimationCurve sizeVariation;
        public enum RotationDirection { Local, Global };
        public RotationDirection rotationSpace;
        public Vector3 preferredRotation;
        public Vector3 rotationRandomness;
        public Vector3 averagePosition;
        public float length;
        [SerializeReference]
        private List<int> number;
        [SerializeReference]
        private List<float> size;

        public float GetSize(int index)
        {
            return size[index];
        }

        public void SetSize(int index, float value)
        {
            if (value > 0)
                size[index] = value;
            else
                size[index] = 0.001f;
        }

        public int GetNumber(int index)
        {
            return number[index];
        }

        public void SetNumber(int index, int value)
        {
            if (value > 0)
                number[index] = value;
            else
                number[index] = 1;
        }

        public Leaves() : base()
        {
            seed = 1;
            offsetFromBranch = 0.09f;
            range = new Vector2(0, 1);
            type = AppendageType.Leaves;
            number = new List<int>();
            number.Add(20);
            number.Add(15);
            number.Add(10);
            number.Add(7);
            size = new List<float>();
            size.Add(0.5f);
            size.Add(0.5f);
            size.Add(0.6f);
            size.Add(0.7f);
            sizeVariation = AnimationCurve.Constant(0, 1, 1);
            rotationSpace = RotationDirection.Local;
            preferredRotation = Vector3.zero;
            length = 1;
            rotationRandomness = new Vector3(0, 360, 0);
        }

        public Leaves(Leaves copyFrom) : base()
        {
            seed = copyFrom.seed;
            offsetFromBranch = copyFrom.offsetFromBranch;
            range = copyFrom.range;
            type = AppendageType.Leaves;
            number = new List<int>();

            foreach (var item in copyFrom.number)
            {
                number.Add(item);
            }

            size = new List<float>();

            foreach (var item in copyFrom.size)
            {
                size.Add(item);
            }

            sizeVariation = new AnimationCurve(copyFrom.sizeVariation.keys);
            rotationSpace = copyFrom.rotationSpace;
            preferredRotation = copyFrom.preferredRotation;
            length = copyFrom.length;
            rotationRandomness = copyFrom.rotationRandomness;
        }


        public override int GetVertexCount(int index)
        {
            return GetNumber(index) * 4;
        }

        public override void AddLoD()
        {
            number.Add(7);
            size.Add(0.7f);
            render.Add(render[render.Count - 1]);
        }
    }
    [System.Serializable]
    public class SDFLeaves : Appendage
    {
        public enum FormType { Sphere, Cube, Cylinder, Cone, Pyramid, Torus };
        public FormType formType;
        public enum RotationDirection { Local, Global };
        public RotationDirection rotationSpace;
        public bool overrideSphereSpace;
        public Vector3 preferredRotation;
        public Vector3 rotationRandomness;
        public Vector3 positionOffset;
        [FormerlySerializedAs("useFormNorm")]
        public bool useSphereNormals;
        public float cut;
        public bool shell;
        public AnimationCurve density;
        public Quaternion rotation;
        [SerializeField]
        private float radius;

        public float innerRadius;
        [SerializeReference]
        private List<int> number;
        [SerializeReference]
        private List<float> size;
        public Vector3 scale;
        public float length;
        public float smoothness;
        
        public float GetSize(int index)
        {
            return size[index];
        }

        public void SetSize(int index, float value)
        {
            if (value > 0)
                size[index] = value;
            else
                size[index] = 0.001f;
        }

        public int GetNumber(int index)
        {
            return number[index];
        }

        public void SetNumber(int index, int value)
        {
            if (value > 0)
                number[index] = value;
            else
                number[index] = 1;
        }
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (value > 0)
                    radius = value;
                else radius = 0.001f;
            }
        }

        public SDFLeaves() : base()
        {
            smoothness = 0.5f;
            seed = 1;
            type = AppendageType.SDFLeaves;
            shell = true;
            formType = FormType.Sphere;
            overrideSphereSpace = false;
            rotationSpace = RotationDirection.Local;
            useSphereNormals = false;
            preferredRotation = new Vector3(90, 0, 0);
            rotationRandomness = new Vector3(15, 15, 15);
            number = new List<int>();
            number.Add(200);
            number.Add(100);
            number.Add(50);
            number.Add(25);
            size = new List<float>();
            size.Add(0.5f);
            size.Add(0.65f);
            size.Add(0.8f);
            size.Add(0.95f);
            innerRadius = 0.25f;
            cut = 0.5f;
            Radius = 1;
            positionOffset = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one;
            density = AnimationCurve.Linear(0, 1, 0, 1);
            length = 1;
        }

        public SDFLeaves(SDFLeaves copyFrom) : base()
        {
            smoothness = copyFrom.smoothness;
            seed = copyFrom.seed;
            type = AppendageType.SDFLeaves;
            shell = copyFrom.shell;
            formType = copyFrom.formType;
            preferredRotation = copyFrom.preferredRotation;
            rotationRandomness = copyFrom.rotationRandomness;
            number = new List<int>();
            foreach (var item in copyFrom.number)
            {
                number.Add(item);
            }
            size = new List<float>();
            foreach (var item in copyFrom.size)
            {
                size.Add(item);
            }
            cut = copyFrom.cut;
            Radius = copyFrom.Radius;
            positionOffset = copyFrom.positionOffset;
            rotation = copyFrom.rotation;
            scale = copyFrom.scale;
            density = new AnimationCurve(copyFrom.density.keys);
            length = copyFrom.length;
            Position = copyFrom.Position;
            overrideSphereSpace = copyFrom.overrideSphereSpace;
            rotationSpace = copyFrom.rotationSpace;
            useSphereNormals = copyFrom.useSphereNormals;
            innerRadius = copyFrom.innerRadius;
        }

        public override int GetVertexCount(int index)
        {
            return 4 * GetNumber(index);
        }

        public override void AddLoD()
        {
            number.Add(50);
            size.Add(0.75f);
            render.Add(render[render.Count - 1]);
        }
    }
}
#endif