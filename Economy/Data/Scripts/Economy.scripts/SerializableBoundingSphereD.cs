namespace Economy.scripts
{
    using ProtoBuf;
    using VRage;
    using VRageMath;

    [ProtoContract]
    public class SerializableBoundingSphereD
    {
        [ProtoMember(13)]
        public SerializableVector3D Center;

        [ProtoMember(16)]
        public double Radius;

        public static implicit operator BoundingSphereD(SerializableBoundingSphereD v)
        {
            return new BoundingSphereD(v.Center, v.Radius);
        }

        public static implicit operator SerializableBoundingSphereD(BoundingSphereD v)
        {
            return new SerializableBoundingSphereD
            {
                Center = v.Center,
                Radius = v.Radius
            };
        }
    }
}
