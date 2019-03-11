using Csla.Configuration;
using Csla.Serialization;
using Csla.Serialization.Mobile;
using System;
using System.Diagnostics;
using System.IO;

namespace IMobileObjectTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationManager.AppSettings.Set("CslaSerializationFormatter", "MobileFormatter");

            var id1 = Id.FromGuidWithScope(Guid.NewGuid(), Scope.InScope);
            var copy1 = SerializeAndDeserializeObjects(id1);
            AssertMatch(id1, copy1);

            var id2 = Id.FromGuid(Guid.NewGuid());
            var copy2 = SerializeAndDeserializeObjects(id2);
            AssertMatch(id2, copy2);

            Console.ReadLine();
        }

        private static void AssertMatch(Id id1, Id id2)
        {
            Debug.Assert(id2.Value == id1.Value);
            Debug.Assert(id2.Scope == id1.Scope);

            Console.WriteLine($"Original: {id1.Value},{id1.Scope}{Environment.NewLine}Copy: {id2.Value},{id2.Scope}");
        }

        private static T SerializeAndDeserializeObjects<T>(T original)
        {
            using (var buffer = new MemoryStream())
            {
                var formatter = SerializationFormatterFactory.GetFormatter();

                Debug.Assert(formatter.GetType() == typeof(MobileFormatter));

                formatter.Serialize(buffer, original);
                buffer.Position = 0;
                var copy = formatter.Deserialize(buffer);
                return (T)copy;
            }
        }
    }

    [Serializable]
    public struct Id : IMobileObject
    {
        public Id(Guid guid)
        {
            Value = guid;
            Scope = Scope.Undefined;
        }

        public Guid Value { get; private set; }

        public Scope Scope { get; private set; }

        public static Id FromGuid(Guid guid) => new Id(guid);

        public static Id FromGuidWithScope(Guid guid, Scope scope) => new Id(guid, scope);

        public Id(Guid guid, Scope scope)
        {
            Value = guid;
            Scope = scope;
        }

        public void GetState(SerializationInfo info)
        {
            info.AddValue("Value", Value);
            info.AddValue("Scope", Scope);
        }

        public void GetChildren(SerializationInfo info, MobileFormatter formatter) { }

        public void SetState(SerializationInfo info)
        {
            Value = info.GetValue<Guid>("Value");
            Scope = info.GetValue<Scope>("Scope");
        }

        public void SetChildren(SerializationInfo info, MobileFormatter formatter) { }
    }

    public enum Scope
    {
        Undefined,
        InScope,
        NotInScope
    }
}
