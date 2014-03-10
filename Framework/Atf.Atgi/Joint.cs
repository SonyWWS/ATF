//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// Interface for ATGI joint. Represents the jointType in atgi.xsd, which extends the nodeType complex type.</summary>
    public class Joint : Node, IJoint//, IHierarchical, IListable
    {
        /// <summary>
        /// Performs custom actions on NodeSet events.
        /// Called after successfully attaching to internal DOM object.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            // Initialize scale to (1, 1, 1) if missing
            DomNode.SetAttributeIfDefault(Schema.jointType.scaleAttribute, new Vec3F(1, 1, 1));
        }

        /// <summary>
        /// Gets or sets a flag indicating which axes are free to rotate</summary>
        public EulerAngleChannels Freedoms
        {
            get
            {
                DomNode freedoms = DomNode.GetChild(Schema.jointType.freedomsChild);
                string channels = freedoms.GetAttribute(Schema.jointType_freedoms.channelsAttribute).ToString();
                EulerAngleChannels value;
                EnumUtil.TryParse(channels, out value);
                return value;
            }
            set
            {
                DomNode freedoms = DomNode.GetChild(Schema.jointType.freedomsChild);
                freedoms.SetAttribute(Schema.jointType_freedoms.channelsAttribute, value);
            }
        }

        /// <summary>
        /// Gets or sets a EulerAngleLimits indicating the minimum rotation allowed on axes</summary>
        public EulerAngleLimits MinRotation
        {
            get
            {
                DomNode minRotNode = DomNode.GetChild(Schema.jointType.minrotationChild);
                string channelsString = minRotNode.GetAttribute(Schema.jointType_minrotation.channelsAttribute).ToString();
                EulerAngleChannels channels;
                EnumUtil.TryParse(channelsString, out channels);
                float[] vals = minRotNode.GetAttribute(Schema.jointType_minrotation.Attribute) as float[];
                return new EulerAngleLimits(vals, channels);
            }
            set
            {
                DomNode minRotNode = DomNode.GetChild(Schema.jointType.minrotationChild);
                minRotNode.SetAttribute(Schema.jointType_minrotation.channelsAttribute, value.Channels.ToString());
                minRotNode.SetAttribute(Schema.jointType_minrotation.Attribute, value.Angles);
            }
        }

        /// <summary>
        /// Gets or sets a EulerAngleLimits indicating the maximum rotation allowed on axes</summary>
        public EulerAngleLimits MaxRotation
        {
            get
            {
                DomNode maxRotNode = DomNode.GetChild(Schema.jointType.maxrotationChild);
                string channelsString = maxRotNode.GetAttribute(Schema.jointType_maxrotation.channelsAttribute).ToString();
                EulerAngleChannels channels;
                EnumUtil.TryParse(channelsString, out channels);
                float[] vals = maxRotNode.GetAttribute(Schema.jointType_maxrotation.Attribute) as float[];
                return new EulerAngleLimits(vals, channels);
            }
            set
            {
                DomNode maxRotNode = DomNode.GetChild(Schema.jointType.maxrotationChild);
                maxRotNode.SetAttribute(Schema.jointType_maxrotation.channelsAttribute, value.Channels.ToString());
                maxRotNode.SetAttribute(Schema.jointType_maxrotation.Attribute, value.Angles);
            }
        }

        #region IJoint Members

        /// <summary>
        /// Gets or sets the rotation freedom in x</summary>
        public bool RotationFreedomInX
        {
            get { return Freedoms.FreedomInX(); }
            set
            {
                EulerAngleChannels freedom = Freedoms;
                if (value)
                    freedom |= EulerAngleChannels.X;
                else
                    freedom &= ~EulerAngleChannels.X;
                Freedoms = freedom;
            }
        }

        /// <summary>
        /// Gets or sets the rotation freedom in y</summary>
        public bool RotationFreedomInY
        {
            get { return Freedoms.FreedomInY(); }
            set
            {
                EulerAngleChannels freedom = Freedoms;
                if (value)
                    freedom |= EulerAngleChannels.Y;
                else
                    freedom &= ~EulerAngleChannels.Y;
                Freedoms = freedom;
            }
        }

        /// <summary>
        /// Gets or sets the rotation freedom in z</summary>
        public bool RotationFreedomInZ
        {
            get { return Freedoms.FreedomInZ(); }
            set
            {
                EulerAngleChannels freedom = Freedoms;
                if (value)
                    freedom |= EulerAngleChannels.Z;
                else
                    freedom &= ~EulerAngleChannels.Z;
                Freedoms = freedom;
            }
        }

        /// <summary>
        /// Gets whether the joint has a rotation minimum in x</summary>
        public bool HasRotationMinX
        {
            get
            {
                EulerAngleLimits minRot = MinRotation;
                return minRot.HasRotationX;
            }
        }
        /// <summary>
        /// Gets whether the joint has a rotation minimum in y</summary>
        public bool HasRotationMinY
        {
            get
            {
                EulerAngleLimits minRot = MinRotation;
                return minRot.HasRotationY;
            }
        }
        /// <summary>
        /// Gets whether the joint has a rotation minimum in z</summary>
        public bool HasRotationMinZ
        {
            get
            {
                EulerAngleLimits minRot = MinRotation;
                return minRot.HasRotationZ;
            }
        }

        /// <summary>
        /// Gets or sets joint rotation minimum in x</summary>
        public float RotationMinX
        {
            get
            {
                EulerAngleLimits minRot = MinRotation;
                return minRot.X;
            }
            set
            {
                EulerAngleLimits minRot = MinRotation;
                minRot.X = value;
                MinRotation = minRot;            
            }
        }

        /// <summary>
        /// Gets or sets joint rotation minimum in y</summary>
        public float RotationMinY
        {
            get
            {
                EulerAngleLimits minRot = MinRotation;
                return minRot.Y;
            }
            set
            {
                EulerAngleLimits minRot = MinRotation;
                minRot.Y = value;
                MinRotation = minRot;            
            }
        }

        /// <summary>
        /// Gets or sets joint rotation minimum in z</summary>
        public float RotationMinZ
        {
            get
            {
                EulerAngleLimits minRot = MinRotation;
                return minRot.Z;
            }
            set
            {
                EulerAngleLimits minRot = MinRotation;
                minRot.Z = value;
                MinRotation = minRot;            
            }
        }

        /// <summary>
        /// Gets whether the joint has a rotation maximum in x</summary>
        public bool HasRotationMaxX
        {
            get
            {
                EulerAngleLimits maxRot = MaxRotation;
                return maxRot.HasRotationX;
            }
        }

        /// <summary>
        /// Gets whether the joint has a rotation maximum in y</summary>
        public bool HasRotationMaxY
        {
            get
            {
                EulerAngleLimits maxRot = MaxRotation;
                return maxRot.HasRotationY;
            }
        }

        /// <summary>
        /// Gets whether the joint has a rotation maximum in z</summary>
        public bool HasRotationMaxZ
        {
            get
            {
                EulerAngleLimits maxRot = MaxRotation;
                return maxRot.HasRotationZ;
            }
        }

        /// <summary>
        /// Gets or sets the joint rotation maximum in x</summary>
        public float RotationMaxX
        {
            get
            {
                EulerAngleLimits maxRot = MaxRotation;
                return maxRot.X;
            }
            set
            {
                EulerAngleLimits maxRot = MaxRotation;
                maxRot.X = value;
                MaxRotation = maxRot;
            }
        }

        /// <summary>
        /// Gets or sets the joint rotation maximum in y</summary>
        public float RotationMaxY
        {
            get
            {
                EulerAngleLimits maxRot = MaxRotation;
                return maxRot.Y;
            }
            set
            {
                EulerAngleLimits maxRot = MaxRotation;
                maxRot.Y = value;
                MaxRotation = maxRot;
            }
        }

        /// <summary>
        /// Gets or sets the joint rotation maximum in z</summary>
        public float RotationMaxZ
        {
            get
            {
                EulerAngleLimits maxRot = MaxRotation;
                return maxRot.Z;
            }
            set
            {
                EulerAngleLimits maxRot = MaxRotation;
                maxRot.Z = value;
                MaxRotation = maxRot;
            }
        }

        /// <summary>
        /// Gets or sets the additional rotation applied after the normal node rotation</summary>
        public EulerAngles3F JointOrientEul
        {
            get 
            {
                DomNode rotNode = DomNode.GetChild(Schema.jointType.jointOrientEulChild);
                string rotOrdString = rotNode.GetAttribute(Schema.jointType_jointOrientEul.rotOrdAttribute) as string;
                EulerAngleOrder rotOrd;
                EnumUtil.TryParse(rotOrdString, out rotOrd);
                float[] values = rotNode.GetAttribute(Schema.jointType_jointOrientEul.Attribute) as float[];
                return new EulerAngles3F(new Vec3F(values), rotOrd);
            }
            set 
            {
                DomNode rotNode = DomNode.GetChild(Schema.jointType.jointOrientEulChild);
                rotNode.SetAttribute(Schema.jointType_jointOrientEul.rotOrdAttribute, value.RotOrder.ToString());
                rotNode.SetAttribute(Schema.jointType_jointOrientEul.Attribute, value.Angles.ToArray());
            }
        }

        /// <summary>
        /// Gets or sets whether the joint should compensate for scale</summary>
        public bool ScaleCompensate
        {
            get 
            {
                bool scaleCompNode = (bool)DomNode.GetAttribute(Schema.jointType.scaleCompensateAttribute);
                return scaleCompNode;
            }
            set 
            {
                DomNode.SetAttribute(Schema.jointType.scaleCompensateAttribute, value);
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the node rotation. Includes JointOrientEul so that IRenderableNode can be
        /// implemented correctly and that no additional interfaces are needed everywhere that
        /// IRenderableNode is used.</summary>
        public override Vec3F Rotation
        {
            get
            {
                // {base rotation} * jointRot = totalRot
                Matrix3F totalRot = new Matrix3F();
                totalRot.Rotation(base.Rotation);

                EulerAngles3F jointOrientation = JointOrientEul;
                Matrix3F jointRot = jointOrientation.CalculateMatrix();

                totalRot.Mul(totalRot, jointRot);

                float x, y, z;
                totalRot.GetEulerAngles(out x, out y, out z);

                return new Vec3F(x, y, z);
            }
            set
            {
                // baseRot * jointRot = totalRot
                // We need to find 'baseRot'.
                // baseRot * (jointRot * jointRotInverse) = totalRot * jointRotInverse
                // baseRot = totalRot * jointRotInverse
                Matrix3F totalRot = new Matrix3F();
                totalRot.Rotation(value);

                EulerAngles3F jointOrientation = JointOrientEul;
                Matrix3F jointRot = jointOrientation.CalculateMatrix();

                Matrix3F jointRotInverse = new Matrix3F();
                jointRotInverse.Invert(jointRot);

                Matrix3F baseRot = new Matrix3F();
                baseRot.Mul(totalRot, jointRotInverse);

                // test: testTotalRot was the same as totalRot on 10/27/2009
                //Matrix3F testTotalRot = new Matrix3F();
                //testTotalRot.Mul(baseRot, jointRot);

                float x, y, z;
                baseRot.GetEulerAngles(out x, out y, out z);

                base.Rotation = new Vec3F(x, y, z);
            }
        }

        //#region IListable Members

        ///// <summary>
        ///// Gets display info for Dom object</summary>
        ///// <param name="info">Item info, to be filled out</param>
        //public override void GetInfo(Sce.Atf.Applications.ItemInfo info)
        //{
        //    info.Label = "Joint";// (string)InternalObject.GetAttribute(Node.NameAttribute);
        //    info.ImageIndex = info.GetImageList().Images.IndexOfKey(StandardIcon.Data);
        //}
        //#endregion

    }
}
